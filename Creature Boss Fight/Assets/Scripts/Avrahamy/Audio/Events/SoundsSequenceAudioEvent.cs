using UnityEngine;
using UnityEngine.Audio;

namespace Avrahamy.Audio {
    /// <summary>
    /// Plays audio clips one after the other each time.
    /// </summary>
    [CreateAssetMenu(menuName = "Avrahamy/Audio/Events/Sounds Sequence")]
    public class SoundsSequenceAudioEvent : AudioEvent {
        [SerializeField] int nextClipIndex;
        [SerializeField] AudioClip[] clips;
        [SerializeField] AudioMixerGroup mixerGroup;
        [Range(0f, 10f)]
        [SerializeField] float volume = 1f;
        [Range(0.1f, 2f)]
        [SerializeField] float pitch = 1f;
        [Range(-1f, 1f)]
        [SerializeField] float stereoPan;

        public override float Volume {
            get {
                return volume;
            }
        }

        public override void Play(ExtendedAudioSource source, float timeScale = 1f, float delay = 0f) {
            DebugAssert.Assert(timeScale > 0);
            source.AudioEvent = this;
            source.Source.clip = clips[nextClipIndex++];
            if (nextClipIndex == clips.Length) {
                nextClipIndex = 0;
            }
            DebugAssert.WarningAssert(mixerGroup != null, $"{this} doesn't have a mixer group assigned");
            source.Source.outputAudioMixerGroup = mixerGroup;
            source.Source.volume = volume;
            timeScale = isTimeScaled ? timeScale * Time.timeScale : 1f;
            source.TimeScale = timeScale;
            source.Pitch = pitch;
            source.Source.panStereo = stereoPan;
            delay += Delay;
            if (delay > 0f) {
                var time = AudioSettings.dspTime + delay;
                source.Source.PlayScheduled(time);
            } else {
                source.Source.Play();
            }
        }
    }
}
