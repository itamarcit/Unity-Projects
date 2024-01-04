using UnityEngine;
using Random = UnityEngine.Random;
using System;
using Avrahamy.Math;
using Avrahamy.Utils;

namespace Avrahamy.Audio {
    /// <summary>
    /// Wraps an (Extended)AudioSource and allows manipulating it over time.
    /// </summary>
    public class AudioTrack : ITimeScaled {
        public const float PLAY_TO_THE_END = -1f;

        /// <summary>
        /// A unique ID used to identify a specific instance of this track.
        /// </summary>
        public int Handle {
            get {
                return handle;
            }
        }

        public float Volume {
            get {
                return source.Volume;
            }
            set {
                source.Volume = value;
            }
        }

        public bool IsPlaying {
            get {
                return source.IsPlaying;
            }
        }

        public bool IsStopping {
            get {
                return IsPlaying && !isLooping && source.StopTime > 0;
            }
        }

        public float Time {
            get {
                return source.Time;
            }
            set {
                source.Time = value;
            }
        }

        public float TimeScale {
            get {
                return timeScale;
            }
            set {
                if (Mathf.Approximately(timeScale, value) && syncedWith == null) return;
                timeScale = value;
                source.TimeScale = timeScale * (syncedWith?.TimeScale ?? 1f);
            }
        }

        public AudioEvent AudioEvent {
            get {
                return source.AudioEvent;
            }
        }

        public Vector3? Position {
            set {
                if (value == null) {
                    source.Source.spatialize = false;
                    source.Source.spatialBlend = 0f;
                    source.Source.transform.position = Vector3.zero;
                    return;
                }
                source.Source.spatialize = true;
                source.Source.spatialBlend = 1f;
                source.Source.transform.position = (Vector3)value;
            }
        }

        public event Action AudioEnded;

        protected readonly ExtendedAudioSource source;
        // A unique ID used to identify a specific instance of this track.
        private int handle;

        private float timeScale = 1f;
        private ITimeScaled syncedWith;

        private float volumeChangeStartValue;
        private float volumeChangeEndValue;
        private double volumeChangeStartTime = -1;
        private double volumeChangeEndTime;

        private bool isLooping;

        public AudioTrack(ExtendedAudioSource source) {
            this.source = source;
        }

        public bool IsSameSource(ExtendedAudioSource source) {
            return this.source == source;
        }

        public void PlayManaged(AudioEvent audioEvent, float clipBeginTime, float clipEndTime, float fadeInDuration, bool isLooping, float delay) {
            Play(audioEvent, clipBeginTime, clipEndTime, fadeInDuration, isLooping, true, delay);
        }

        public void Play(AudioEvent audioEvent, float clipBeginTime, float clipEndTime, float fadeInDuration, bool isLooping, bool isManaged = false, float delay = 0f) {
            handle = Random.Range(int.MinValue, int.MaxValue);
            TimeScale = 1f;
            syncedWith = null;
            source.AudioEvent = audioEvent;
            source.Source.spatialize = false;
            source.Source.spatialBlend = 0f;
            if (isManaged && audioEvent is CompositeAudioEvent compositeAudioEvent) {
                compositeAudioEvent.PlayManaged(source, 1f, delay);
            } else if (isManaged && audioEvent is AudioEventWithIntro audioEventWithIntro) {
                audioEventWithIntro.PlayManaged(source, 1f, delay);
            } else {
                audioEvent.Play(source, 1f, delay);
            }
            if (clipBeginTime > 0f) {
                source.Time = clipBeginTime;
            }
            if (clipEndTime > 0f && source.Source.clip.length < clipEndTime) {
                var duration = clipEndTime - clipBeginTime;
                source.ScheduleStop(delay + duration);
            } else {
                source.UnScheduleStop();
            }

            if (fadeInDuration > 0f) {
                ChangeVolume(0f, 1f, fadeInDuration);
                if (source.StartTime > 0f) {
                    volumeChangeStartTime = AudioSettings.dspTime + delay;
                    volumeChangeEndTime = volumeChangeStartTime + fadeInDuration;
                }
            } else {
                volumeChangeEndTime = -1;
            }

            this.isLooping = audioEvent is not AudioEventWithIntro && isLooping;
            source.IsLooping = this.isLooping;
        }

        public void PlayFromStart() {
            Time = 0f;
        }

        public void Stop(float fadeOutTime = 0f) {
            isLooping = false;
            if (fadeOutTime <= 0.0001f) {
                source.ScheduleStop(0f);
            } else {
                ChangeVolume(Volume, 0f, fadeOutTime);
                source.ScheduleStop(volumeChangeEndTime - AudioSettings.dspTime);
            }
            source.UnSchedule();
            AudioEnded = null;
        }

        public void ChangeVolume(float fromVolume, float targetVolume, float duration) {
            if (Mathf.Approximately(fromVolume, targetVolume)) {
                // Cancel any current request to change volume.
                volumeChangeStartTime = -1;
                volumeChangeEndTime = -1;
                // Make sure we're at the target volume.
                source.Source.volume = targetVolume;
                return;
            }
            DebugLog.Log(LogTag.Audio, $"{source.Source} {source.AudioEvent} ChangeVolume {fromVolume} => {targetVolume} ({duration}s)");
            volumeChangeStartValue = fromVolume * source.AudioEvent.Volume;
            volumeChangeEndValue = targetVolume * source.AudioEvent.Volume;
            volumeChangeStartTime = AudioSettings.dspTime;
            volumeChangeEndTime = volumeChangeStartTime + duration;
        }

        public void Update(double now) {
            if (!isLooping && source.StopTime > 0 && source.StopTime <= now) {
                source.Source.Stop();
                source.IsLooping = false;
                return;
            }
            // Start time is affected by changes to the time scale, so this check
            // has to come before checking the start time.
            if (syncedWith != null && (source.Source.isPlaying || source.StartTime > 0)) {
                TimeScale = timeScale;
            }
            if (!source.Source.isPlaying && source.StartTime > 0f) {
                return;
            }
            if (volumeChangeEndTime < 0) return;
            if (volumeChangeEndTime <= now) {
                // Volume change ended.
                volumeChangeEndTime = -1;
                source.Source.volume = volumeChangeEndValue;
            } else if (volumeChangeStartTime >= 0f) {
                // Volume change in progress.
                var t = MathsUtils.InverseLerp(volumeChangeStartTime, volumeChangeEndTime, now);
                source.Source.volume = Mathf.Lerp(volumeChangeStartValue, volumeChangeEndValue, t);
            }
            if (!isLooping && !IsPlaying) {
                AudioEnded?.Invoke();
            }
        }

        public void DeductFromDelay(float time) {
            source.DeductFromDelay(time);
        }

        public void SyncWith(ITimeScaled timeScaled) {
            syncedWith = timeScaled;
            if (source.Source.isPlaying || source.StartTime > 0) {
                TimeScale = timeScale;
            }
        }

        public override string ToString() {
            return $"Track:{source.Source.name} ({source.AudioEvent}) #{handle} [{GetHashCode()}]";
        }
    }
}
