using UnityEngine;
using Object = UnityEngine.Object;
using System;
using System.Collections.Generic;
using Avrahamy.EditorGadgets;
using Avrahamy.Messages;

namespace Avrahamy.Audio {
    [CreateAssetMenu(menuName = "Avrahamy/Setup/Audio/Audio Model", fileName = "AudioModel")]
    public class AudioModel : ScriptableObject {
        [Serializable]
        public struct MessageAndAudioPair {
            [ImplementsInterface(typeof(IMessagePredicate))]
            public Object message;
            public AudioEvent audioEvent;
        }

        [SerializeField] MessageAndAudioPair[] _messageAndAudioPairs;

        [SerializeField] int soundSourcesToAddWhenNeeded = 3;
        [SerializeField] int maxSoundSources = 20;

        public Dictionary<Type, AudioEvent> MessageAndAudioPairs {
            get {
                return messageAndAudioPairs;
            }
        }

        public int SoundSourcesToAddWhenNeeded {
            get {
                return soundSourcesToAddWhenNeeded;
            }
        }

        public int MaxSoundSources {
            get {
                return maxSoundSources;
            }
        }

        private readonly Dictionary<Type, AudioEvent> messageAndAudioPairs = new();

        public void Initialize() {
            foreach (var pair in _messageAndAudioPairs) {
                var messageType = (pair.message as IMessagePredicate).GetMessageType();
                messageAndAudioPairs[messageType] = pair.audioEvent;
            }
        }
    }
}
