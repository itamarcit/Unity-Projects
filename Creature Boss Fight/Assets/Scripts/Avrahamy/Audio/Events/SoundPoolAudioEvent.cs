using UnityEngine;
using UnityEngine.Audio;
using Avrahamy.EditorGadgets;
using Avrahamy.Math;

namespace Avrahamy.Audio {
    /// <summary>
    /// Plays a random audio event with random volume, pitch and stereo pan.
    /// </summary>
    [CreateAssetMenu(menuName = "Avrahamy/Audio/Events/Sound Pool")]
    public class SoundPoolAudioEvent : AudioEvent {
        [SerializeField] AudioClip[] clips;
        [SerializeField] AudioMixerGroup mixerGroup;
        [MinMaxRange(0f, 2f)]
        [SerializeField] FloatRange volume = new FloatRange(1f);
        [MinMaxRange(0.1f, 2f)]
        [SerializeField] FloatRange pitch = new FloatRange(1f);
        [MinMaxRange(-1f, 1f)]
        [SerializeField] FloatRange stereoPan;

        public override float Volume {
            get {
                return RandomUtils.Range(volume);
            }
        }

        public override void Play(ExtendedAudioSource source, float timeScale = 1f, float delay = 0f) {
            if (clips.Length == 0) return;
            DebugAssert.Assert(timeScale > 0);

            source.AudioEvent = this;
            source.Source.clip = clips[Random.Range(0, clips.Length)];
            DebugAssert.WarningAssert(mixerGroup != null, $"{this} doesn't have a mixer group assigned");
            source.Source.outputAudioMixerGroup = mixerGroup;
            source.Source.volume = Volume;
            timeScale = isTimeScaled ? timeScale * Time.timeScale : 1f;
            source.TimeScale = timeScale;
            source.Pitch = RandomUtils.Range(pitch);
            source.Source.panStereo = RandomUtils.Range(stereoPan);
            delay += Delay;
            if (delay > 0f) {
                var time = AudioSettings.dspTime + delay;
                source.Source.PlayScheduled(time);
                DebugLog.Log(LogTag.Audio, $"IsPlaying: {source.IsPlaying}");
            } else {
                source.Source.Play();
            }
        }
    }
}
