using UnityEngine;
using System.Collections.Generic;
using System.Text;
using Avrahamy.Collections;
using Avrahamy.Math;
using Avrahamy.Messages;
using BitStrap;

namespace Avrahamy.Audio {
    public class AudioController : MonoBehaviour, IMessageHandler {
        [SerializeField] AudioModel model;
        [SerializeField] GameObject _soundsSources;

        private static AudioController instance;
        // Contains all of the sound sources available for the AudioController.
        // The first few are managed AudioTracks that can have their volume lerped
        // over time, are trimmed or looping. The rest are unmanaged - they play
        // and are done when they are done. AKA simple audio sources.
        private List<ExtendedAudioSource> audioSources;
        // The next index to check when looking for an available simple audio source.
        private int nextSimpleAudioSourceIndex;
        // managedTracks.Count is the first index in audioSources of the simple sound sources.
        private List<AudioTrack> managedTracks;
        // Sources that are used in a "reserved track". These can't be played simultaneously.
        private List<ExtendedAudioSource> reservedSources;
        private float lastTimeScale = 1f;

        public static string GetAudioDebugString() {
            if (instance == null) return string.Empty;
            return instance._GetAudioDebugString();
        }

        public string _GetAudioDebugString() {
            var sb = new StringBuilder($"Managed Tracks: {managedTracks.Count}");
            for (int i = 0; i < audioSources.Count; i++) {
                var soundSource = audioSources[i];
                var clip = soundSource.Source.clip;
                if (clip == null) {
                    sb.Append($"\n{i}. ERROR: {soundSource.Source} clip is null but isPlaying = {soundSource.IsPlaying}");
                    continue;
                }
                sb.Append($"\n{i}. {soundSource.Time:00.00}/{clip.length:00.00} xTime: {soundSource.TimeScale:0.00} Pitch: {soundSource.Source.pitch:0.00} V: {soundSource.Source.volume:0.00} {(soundSource.IsLooping ? "Loop" : "Once")} {(soundSource.IsPlaying ? "Playing" : "Stopped")} {clip.name}");
            }
            return sb.ToString();
        }

        protected void Awake() {
            if (instance != null) {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(this);

            model.Initialize();

            var sources = _soundsSources.GetComponentsInChildren<AudioSource>();
            audioSources = new List<ExtendedAudioSource>(sources.Length);
            foreach (var source in sources) {
                audioSources.Add(new ExtendedAudioSource(source));
            }
            var simpleSourcesStartIndex = Mathf.Min(audioSources.Count, 1);
            nextSimpleAudioSourceIndex = simpleSourcesStartIndex;
            managedTracks = new List<AudioTrack>();
            for (int i = 0; i < simpleSourcesStartIndex; i++) {
                var track = new AudioTrack(audioSources[i]);
                managedTracks.Add(track);
            }
            reservedSources = new List<ExtendedAudioSource>();

            foreach (var soundsSource in audioSources) {
                soundsSource.IsLooping = false;
            }
            
            GlobalMessagesHub.Instance.Subscribe<GetAudioSourceMessage>(this);
            GlobalMessagesHub.Instance.Subscribe<RequestPlayManagedAudioMessage>(this);
            GlobalMessagesHub.Instance.Subscribe<RequestStopAllLoopingAudioMessage>(this);

            foreach (var messageType in model.MessageAndAudioPairs.Keys) {
                GlobalMessagesHub.Instance.Subscribe(messageType, this);
            }
        }

        protected void OnDestroy() {
            GlobalMessagesHub.Instance.Unsubscribe<GetAudioSourceMessage>(this);
            GlobalMessagesHub.Instance.Unsubscribe<RequestPlayManagedAudioMessage>(this);
            GlobalMessagesHub.Instance.Unsubscribe<RequestStopAllLoopingAudioMessage>(this);

            foreach (var messageType in model.MessageAndAudioPairs.Keys) {
                GlobalMessagesHub.Instance.Unsubscribe(messageType, this);
            }
#if UNITY_EDITOR
            instance = null;
#endif
        }

        public void OnMessage(object message) {
            var messageType = message.GetType();

            if (messageType == typeof(GetAudioSourceMessage)) {
                var getAudioSourceMessage = message as GetAudioSourceMessage;
                getAudioSourceMessage.AudioSource = GetAvailableAudioSource(getAudioSourceMessage.Track);
                return;
            }
            if (messageType == typeof(RequestPlayManagedAudioMessage)) {
                var requestPlayMessage = message as RequestPlayManagedAudioMessage;
                requestPlayMessage.PlayedAudioInstance = PlayManagedAudio(
                    requestPlayMessage.Event,
                    requestPlayMessage.Position,
                    requestPlayMessage.StartTime,
                    requestPlayMessage.EndTime,
                    requestPlayMessage.FadeInDuration,
                    requestPlayMessage.IsLooping,
                    requestPlayMessage.Delay);
                return;
            }
            if (messageType == typeof(RequestStopAllLoopingAudioMessage)) {
                StopAllManagedTracks();
                DebugLog.Log(LogTag.Audio, "StopAllManagedTracks");
                return;
            }

            foreach (var type in model.MessageAndAudioPairs.Keys) {
                if (messageType != type) continue;
                var audioEvent = model.MessageAndAudioPairs[type];
                if (audioEvent != null) {
                    audioEvent.Play();
                    DebugLog.Log(LogTag.Audio, $"Got {messageType} - playing {audioEvent}");
                }
                return;
            }
        }

        protected void Update() {
            var now = AudioSettings.dspTime;
            
            var currentTimeScale = Time.timeScale;
            if (!Mathf.Approximately(currentTimeScale, lastTimeScale)) {
                foreach (var track in managedTracks) {
                    track.TimeScale = currentTimeScale;
                    if (track.IsPlaying) {
                        track.Update(now);
                    }
                }
                foreach (var source in audioSources) {
                    source.TimeScale = currentTimeScale;
                }
                lastTimeScale = currentTimeScale;
                return;
            }

            foreach (var track in managedTracks) {
                if (track.IsPlaying) {
                    track.Update(now);
                }
            }
        }

        private void StopAllManagedTracks() {
            foreach (var track in managedTracks) {
                if (track.IsStopping) continue;
                track.Stop();
            }
        }

        private AudioInstance PlayManagedAudio(AudioEvent audioEvent, Vector3? position, float startTime, float endTime, float fadeInDuration, bool isLooping, float delay) {
            // Find an available AudioTrack.
            AudioTrack availableTrack = null;
            var simpleSourcesStartIndex = managedTracks.Count;
            for (int i = 0; i < simpleSourcesStartIndex; i++) {
                var track = managedTracks[i];
                if (track.IsPlaying) continue;
                availableTrack = track;
                break;
            }

            if (availableTrack == null) {
                // No available managed track. Allocate a new managed AudioTrack.
                var audioSourceIndex = GetAvailableAudioSourceIndex();
                DebugAssert.Assert(audioSourceIndex >= 0 && audioSourceIndex < audioSources.Count, $"GetAvailableAudioSourceIndex returned {audioSourceIndex}. There are {audioSources.Count} sources");
                var availableAudioSource = audioSources[audioSourceIndex];
                if (simpleSourcesStartIndex < audioSources.Count) {
                    DebugLog.Log(LogTag.Audio, $"All managed tracks are busy. Managed Tracks: {managedTracks.Count}");
                    for (int i = 0; i < audioSources.Count; i++) {
                        var soundSource = audioSources[i];
                        if (soundSource.Source.clip == null) {
                            continue;
                        }
                        DebugLog.LogError(LogTag.Audio, $"{i}. {soundSource.Source} {soundSource.Source.clip} {soundSource.Time} / {soundSource.Source.clip.length} isPlaying = {soundSource.IsPlaying}");
                    }

                    if (audioSourceIndex != simpleSourcesStartIndex) {
                        // Swap the audio sources so the available audio source is at
                        // managedTracks.Count.
                        audioSources[audioSourceIndex] = audioSources[simpleSourcesStartIndex];
                        audioSources[simpleSourcesStartIndex] = availableAudioSource;
                        DebugLog.Log(LogTag.Audio, $"Swapping audioSources {audioSourceIndex}<>{simpleSourcesStartIndex}");
                    }
                    availableTrack = new AudioTrack(availableAudioSource);

                    managedTracks.Add(availableTrack);
                    DebugLog.Log(LogTag.Audio, $"Added managedTrack count={managedTracks.Count}");
                } else {
                    // Can't add any more managed tracks. All of the audio sources
                    // are managed. Clear up the selected managed track and reuse
                    // it.
                    for (var i = 0; i < managedTracks.Count; i++) {
                        var managedTrack = managedTracks[i];
                        if (managedTrack.IsSameSource(availableAudioSource)) {
                            availableTrack = managedTrack;
                            availableTrack.Stop();
                            DebugLog.LogError(LogTag.Audio, $"Stopped a managedTrack #{i} {availableTrack}");
                            break;
                        }
                    }
                }
            }

            DebugLog.Log(LogTag.Audio, $"PlayManagedAudio {audioEvent} on {availableTrack}. Loop: {isLooping} Delay: {delay}");
            availableTrack.PlayManaged(audioEvent, startTime, endTime, fadeInDuration, isLooping, delay);
            availableTrack.Position = position;
            return audioEvent is CompositeAudioEvent compositeAudioEvent
                ? new AudioInstance(availableTrack, compositeAudioEvent)
                : audioEvent is AudioEventWithIntro audioEventWithIntro
                    ? new AudioInstance(availableTrack, audioEventWithIntro)
                    : new AudioInstance(availableTrack);
        }

        private ExtendedAudioSource GetAvailableAudioSource(int track) {
            if (track > 0 && reservedSources.Count >= track) {
                var reservedTrack = reservedSources[track - 1];
                if (reservedTrack != null) {
                    // Check if reserved source is busy.
                    if (reservedTrack.IsPlaying) return null;
                    // Can play on reserved source.
                    reservedTrack.Source.spatialize = false;
                    reservedTrack.Source.spatialBlend = 0f;
                    DebugLog.Log(LogTag.Audio, $"GetAvailableAudioSource({track}) - returned reserved source");
                    return reservedTrack;
                }
            }
            var index = GetAvailableAudioSourceIndex();
            var soundSource = audioSources[index];
            soundSource.Source.spatialize = false;
            soundSource.Source.spatialBlend = 0f;
            if (track > 0) {
                reservedSources.SetSize(track, null);
                reservedSources[track - 1] = soundSource;
            }
            DebugLog.Log(LogTag.Audio, $"GetAvailableAudioSource({track}) - returned source {index}");
            return soundSource;
        }

        private int GetAvailableAudioSourceIndex() {
            // Look for the next available sound source.
            for (int i = nextSimpleAudioSourceIndex; i < audioSources.Count; i++) {
                var soundSource = audioSources[i];
                if (!soundSource.IsPlaying) {
                    reservedSources.Remove(soundSource);
                    nextSimpleAudioSourceIndex = i;
                    return nextSimpleAudioSourceIndex;
                }
            }
            for (int i = managedTracks.Count; i < nextSimpleAudioSourceIndex; i++) {
                var soundSource = audioSources[i];
                if (!soundSource.IsPlaying) {
                    reservedSources.Remove(soundSource);
                    nextSimpleAudioSourceIndex = i;
                    return nextSimpleAudioSourceIndex;
                }
            }

            DebugLog.Log(LogTag.Audio, $"All simple sound sources are busy. Managed Tracks: {managedTracks.Count}. Next simple index: {nextSimpleAudioSourceIndex}");
            for (int i = 0; i < audioSources.Count; i++) {
                var soundSource = audioSources[i];
                if (soundSource.Source.clip == null) {
                    DebugLog.LogError($"{soundSource} clip is null but isPlaying = {soundSource.IsPlaying}");
                    continue;
                }
                DebugLog.Log(LogTag.Audio, $"{i}. {soundSource} {soundSource.Source.clip} {soundSource.Time} / {soundSource.Source.clip.length} isPlaying = {soundSource.IsPlaying}");
            }

            // No available sources.
            var sourcesToAdd = Mathf.Min(
                model.SoundSourcesToAddWhenNeeded,
                model.MaxSoundSources - audioSources.Count);
            if (sourcesToAdd <= 0) {
                // Can't add new sources. Look for the most completed next source
                // and replace it.
                float maxProgress = 0f;
                for (int j = nextSimpleAudioSourceIndex + 1; j <= 5; j++) {
                    var i = j % audioSources.Count;
                    var soundSource = audioSources[i];
                    var clip = soundSource.Source.clip;
                    var duration = clip == null ? 0f : clip.length;
                    if (duration <= 0) {
                        reservedSources.Remove(soundSource);
                        nextSimpleAudioSourceIndex = i;
                        return nextSimpleAudioSourceIndex;
                    }
                    var progress = soundSource.Time / duration;
                    if (progress > maxProgress) {
                        maxProgress = progress;
                        nextSimpleAudioSourceIndex = i;
                    }
                }
                reservedSources.Remove(audioSources[nextSimpleAudioSourceIndex]);
                return nextSimpleAudioSourceIndex;
            }

            // All sound sources are being used. Create some new sources.
            nextSimpleAudioSourceIndex = audioSources.Count;
            var prototypeSource = audioSources[0].Source.gameObject;
            var parentTransform = prototypeSource.transform.parent;
            for (int i = 0; i < sourcesToAdd; i++) {
                var obj = Instantiate(prototypeSource, parentTransform, false);
                obj.name = "Source" + (nextSimpleAudioSourceIndex + i + 1);
                var newSource = obj.GetComponent<AudioSource>();
                newSource.clip = null;
                newSource.playOnAwake = false;
                newSource.loop = false;
                audioSources.Add(new ExtendedAudioSource(newSource));
                DebugLog.Log(LogTag.Audio, $"Added audioSource count={audioSources.Count}");
            }

            return nextSimpleAudioSourceIndex;
        }

#if UNITY_EDITOR
        [Button(true)]
        private void DebugLogState() {
            DebugLog.LogError($"AudioController has:\n\t{audioSources.Count}/{model.MaxSoundSources} Sources"
                              + $"\n\t{managedTracks.Count}/{audioSources.Count} Managed Tracks"
                              + $"\n\t{reservedSources.Count}/{audioSources.Count} Reserved Sources");
        }
#endif
    }
}
