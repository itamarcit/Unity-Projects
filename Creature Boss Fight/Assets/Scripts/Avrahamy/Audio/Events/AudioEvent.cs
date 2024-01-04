using UnityEngine;
using Avrahamy.Messages;

namespace Avrahamy.Audio {
    /// <summary>
    /// Base class for playing an audio clip on an AudioSource.
    /// Provides an implementation to request an audio source from the
    /// AudioController.
    /// </summary>
    public abstract class AudioEvent : ScriptableObject {
        [SerializeField] int track;
        [SerializeField] protected bool isTimeScaled = true;
        [SerializeField] float delay;

        public abstract float Volume { get; }

        public bool IsTimeScaled {
            get {
                return isTimeScaled;
            }
        }

        public float Delay {
            get {
                return delay > 0f ? delay : 0f;
            }
        }

        // NOTE: Not using default value to make it work in Unity events.
        public void Play() {
            Play(1f);
        }

        /// <summary>
        /// Play the audio using the AudioController.
        /// </summary>
        public void Play(float timeScale) {
            var message = track > 0 ? new GetAudioSourceMessage(track) : GetAudioSourceMessage.Instance;
            GlobalMessagesHub.Instance.Dispatch(message);
            var source = message.AudioSource;
            if (source != null) {
                source.Source.spatialize = false;
                source.Source.spatialBlend = 0f;
                DebugAssert.Assert(timeScale > 0);
                Play(source, timeScale);
            }
        }

        public void Play(Transform position) {
            Play(position.position);
        }

        /// <summary>
        /// Play the audio using the AudioController at a certain position.
        /// </summary>
        public void Play(Vector3 position, float timeScale = 1f) {
            var message = GetAudioSourceMessage.Instance;
            GlobalMessagesHub.Instance.Dispatch(message);
            var source = message.AudioSource;
            if (source != null) {
                source.Source.spatialize = true;
                source.Source.spatialBlend = 1f;
                source.Source.transform.position = position;
                DebugAssert.Assert(timeScale > 0);
                Play(source, timeScale);
            }
        }

        /// <summary>
        /// Play the audio through the given AudioSource.
        /// </summary>
        public abstract void Play(ExtendedAudioSource source, float timeScale = 1f, float delay = 0f);
    }
}
