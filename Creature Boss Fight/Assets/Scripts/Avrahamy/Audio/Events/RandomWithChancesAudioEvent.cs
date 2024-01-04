using UnityEngine;
using System;
using Avrahamy.Math;

namespace Avrahamy.Audio {
    /// <summary>
    /// Plays a random audio event out of a list of weighted audio events.
    /// </summary>
    [CreateAssetMenu(menuName = "Avrahamy/Audio/Events/Random With Chances")]
    public class RandomWithChancesAudioEvent : AudioEvent {
        [Serializable]
        public class CompositeEntry : RandomUtils.ValueWithChance<AudioEvent> {}

        [SerializeField] CompositeEntry[] entries;

        public override float Volume {
            get {
                return entries[0].value.Volume;
            }
        }

        public override void Play(ExtendedAudioSource source, float timeScale = 1f, float delay = 0f) {
            DebugAssert.Assert(timeScale > 0);
            var audioEvent = entries.ChooseRandomWithChances(true);
            if (audioEvent == null) return;
            delay += Delay;
            audioEvent.Play(source, timeScale, delay);
        }
    }
}
