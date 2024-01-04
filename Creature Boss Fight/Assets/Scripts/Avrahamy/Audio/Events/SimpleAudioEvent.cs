using UnityEngine;
using UnityEngine.Audio;
using Avrahamy.EditorGadgets;
using Avrahamy.Math;

namespace Avrahamy.Audio {
    [CreateAssetMenu(menuName = "Avrahamy/Audio/Events/Simple")]
    public class SimpleAudioEvent : AudioEvent {
        [SerializeField] AudioClip clip;
        [SerializeField] AudioMixerGroup mixerGroup;
        [MinMaxRange(0f, 2f)]
        [SerializeField] FloatRange volume = new FloatRange(1f);
        [MinMaxRange(0.1f, 3f)]
        [SerializeField] FloatRange pitch = new FloatRange(1f);
        [Range(-1f, 1f)]
        [SerializeField] float stereoPan;

        public override float Volume {
            get {
                return RandomUtils.Range(volume);
            }
        }

        public override void Play(ExtendedAudioSource source, float timeScale = 1f, float delay = 0f) {
            DebugAssert.Assert(timeScale > 0);
            source.AudioEvent = this;
            source.Source.clip = clip;
            DebugAssert.WarningAssert(mixerGroup != null, $"{this} doesn't have a mixer group assigned");
            source.Source.outputAudioMixerGroup = mixerGroup;
            source.Source.volume = Volume;
            timeScale = isTimeScaled ? timeScale * Time.timeScale : 1f;
            source.TimeScale = timeScale;
            source.Pitch = RandomUtils.Range(pitch);
            source.Source.panStereo = stereoPan;
            delay += Delay;
            if (delay > 0f) {
                source.Schedule(delay);
            } else {
                source.Source.Play();
            }
        }
    }
}
