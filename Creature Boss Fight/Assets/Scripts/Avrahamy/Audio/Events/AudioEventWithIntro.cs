using UnityEngine;
using Avrahamy.Messages;

namespace Avrahamy.Audio {
    [CreateAssetMenu(menuName = "Avrahamy/Audio/Events/With Intro")]
    public class AudioEventWithIntro : AudioEvent {
        [SerializeField] AudioEvent intro;
        [SerializeField] AudioEvent clip;
        [SerializeField] bool isLooping;

        private AudioInstance? clipAudioInstance;

        public override float Volume {
            get {
                return clip.Volume;
            }
        }

        public AudioInstance TakeAudioInstance() {
            var instance = (AudioInstance)clipAudioInstance;
            clipAudioInstance = null;
            return instance;
        }

        public override void Play(ExtendedAudioSource source, float timeScale = 1f, float delay = 0f) {
            clipAudioInstance?.Stop();
            _Play(source, timeScale, false, delay);
        }

        public void PlayManaged(ExtendedAudioSource source, float timeScale = 1f, float delay = 0f) {
            clipAudioInstance?.Stop();
            _Play(source, timeScale, true, delay);
        }

        private void _Play(ExtendedAudioSource source, float timeScale, bool isManaged, float delay) {
            delay += Delay;
            intro.Play(source, timeScale, delay);
            Play(clip, timeScale, isManaged, delay + (float)source.TimeRemaining);
        }

        private void Play(AudioEvent audioEvent, float timeScale, bool isManaged, float delay) {
            DebugAssert.Assert(timeScale > 0);
            if (isManaged) {
                var requestMessage = new RequestPlayManagedAudioMessage(audioEvent, isLooping, -1f, delay);
                GlobalMessagesHub.Instance.Dispatch(requestMessage);
                clipAudioInstance = requestMessage.PlayedAudioInstance;
                return;
            }
            var message = GetAudioSourceMessage.Instance;
            GlobalMessagesHub.Instance.Dispatch(message);
            var source = message.AudioSource;
            if (source != null) {
                DebugLog.Log(LogTag.Audio, $"Play {audioEvent} {delay}");
                audioEvent.Play(source, timeScale, delay);
            }
        }
    }
}
