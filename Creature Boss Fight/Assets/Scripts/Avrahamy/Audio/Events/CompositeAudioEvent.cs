using UnityEngine;
using System;
using System.Collections.Generic;
using Avrahamy.Messages;

namespace Avrahamy.Audio {
    /// <summary>
    /// Plays multiple audio events at once with an optional delay.
    /// </summary>
    [CreateAssetMenu(menuName = "Avrahamy/Audio/Events/Composite (Multiple)")]
    public class CompositeAudioEvent : AudioEvent {
        [Serializable]
        public struct CompositeEntry {
            public float delay;
            public AudioEvent audioEvent;
        }

        [SerializeField] AudioEvent firstEvent;
        [SerializeField] CompositeEntry[] additionalEvents;
        [SerializeField] bool isLooping;

        private readonly List<AudioInstance> lastAudioInstances = new List<AudioInstance>();

        public override float Volume {
            get {
                return firstEvent.Volume;
            }
        }

        public override void Play(ExtendedAudioSource source, float timeScale = 1f, float delay = 0f) {
            lastAudioInstances.Clear();
            _Play(source, timeScale, false, delay);
        }

        public void PlayManaged(ExtendedAudioSource source, float timeScale = 1f, float delay = 0f) {
            lastAudioInstances.Clear();
            _Play(source, timeScale, true, delay);
        }

        public List<AudioInstance> GetLastAudioInstances() {
            return lastAudioInstances;
        }

        private void _Play(ExtendedAudioSource source, float timeScale, bool isManaged, float delay) {
            delay += Delay;
            firstEvent.Play(source, timeScale, delay);
            foreach (var audioEvent in additionalEvents) {
                if (delay <= 0f && audioEvent.delay <= 0f) {
                    Play(audioEvent, timeScale, isManaged, 0f);
                } else {
                    Play(audioEvent, timeScale, isManaged, audioEvent.delay + delay);
                }
            }
        }

        private void Play(CompositeEntry audioEvent, float timeScale, bool isManaged, float delay) {
            DebugAssert.Assert(timeScale > 0);
            if (isManaged) {
                var requestMessage = new RequestPlayManagedAudioMessage(audioEvent.audioEvent, isLooping, -1f, delay);
                GlobalMessagesHub.Instance.Dispatch(requestMessage);
                lastAudioInstances.Add(requestMessage.PlayedAudioInstance);
                return;
            }
            var message = GetAudioSourceMessage.Instance;
            GlobalMessagesHub.Instance.Dispatch(message);
            var source = message.AudioSource;
            if (source != null) {
                DebugLog.Log(LogTag.Audio, $"Play {audioEvent.audioEvent} {delay}");
                audioEvent.audioEvent.Play(source, timeScale, delay);
            }
        }
    }
}
