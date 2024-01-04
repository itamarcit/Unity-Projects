using System;
using System.Collections.Generic;
using Avrahamy.Utils;

namespace Avrahamy.Audio {
    /// <summary>
    /// A handle to a managed audio track.
    /// It provides limited functionality to the track, and when the track is
    /// reused, this instance becomes invalid to it can not affect the new instance
    /// that is now using the track.
    /// </summary>
    public struct AudioInstance : ITimeScaled {
        public float Volume {
            get {
                return IsValid ? track.Volume : 0f;
            }
            set {
                if (!IsValid) return;
                track.Volume = value;
                if (additionalAudioInstances == null) return;
                for (var i = 0; i < additionalAudioInstances.Count; i++) {
                    var audioInstance = additionalAudioInstances[i];
                    audioInstance.Volume = value;
                }
            }
        }

        public bool IsPlaying {
            get {
                if (additionalAudioInstances != null) {
                    foreach (var audioInstance in additionalAudioInstances) {
                        if (audioInstance.IsPlaying) return true;
                    }
                }
                return IsValid && track.IsPlaying;
            }
        }

        public float Time {
            get {
                return IsValid ? track.Time : 0f;
            }
            set {
                if (!IsValid) return;
                track.Time = value;
                DebugAssert.Assert(compositeAudioEvent == null, $"AudioInstance.Time is not supported for {compositeAudioEvent}");
            }
        }

        public float TimeScale {
            get {
                return track.TimeScale;
            }
            set {
                track.TimeScale = value;
            }
        }

        public AudioEvent AudioEvent {
            get {
                if (!IsValid) return null;
                return track.AudioEvent;
            }
        }

        public bool IsValid {
            get {
                return track != null && track.Handle == handle;
            }
        }

        public event Action AudioEnded {
            add {
                if (!IsValid) return;
                track.AudioEnded += value;
                DebugAssert.WarningAssert(compositeAudioEvent == null, $"AudioInstance.AudioEnded will be called only for the first audio for {compositeAudioEvent}");
            }
            remove {
                // Allow to unregister even if the handle is wrong.
                if (track == null) return;
                track.AudioEnded -= value;
            }
        }

        private AudioTrack track;
        private readonly int handle;
        private readonly CompositeAudioEvent compositeAudioEvent;
        private readonly List<AudioInstance> additionalAudioInstances;

        public AudioInstance(AudioTrack track) {
            this.track = track;
            handle = track.Handle;
            compositeAudioEvent = null;
            additionalAudioInstances = null;
        }

        public AudioInstance(AudioTrack track, CompositeAudioEvent compositeAudioEvent)
            : this(track) {
            this.compositeAudioEvent = compositeAudioEvent;
            additionalAudioInstances = compositeAudioEvent.GetLastAudioInstances();
        }

        public AudioInstance(AudioTrack track, AudioEventWithIntro audioEventWithIntro)
            : this(track) {
            compositeAudioEvent = null;
            additionalAudioInstances = new List<AudioInstance> {
                audioEventWithIntro.TakeAudioInstance()
            };
        }

        public void PlayFromStart() {
            if (!IsValid) return;
            track.PlayFromStart();
            DebugAssert.WarningAssert(compositeAudioEvent == null, $"AudioInstance.PlayFromStart will only work for the first audio for {compositeAudioEvent}");
        }

        public void Stop(float fadeOutTime = 0f) {
            if (IsValid) {
                track.Stop(fadeOutTime);
                track = null;
            }
            if (additionalAudioInstances == null) return;
            foreach (var audioInstance in additionalAudioInstances) {
                audioInstance.Stop(fadeOutTime);
            }
        }

        public void ChangeVolume(float fromVolume, float targetVolume, float duration) {
            if (!IsValid) return;
            track.ChangeVolume(fromVolume, targetVolume, duration);
            if (additionalAudioInstances == null) return;
            foreach (var audioInstance in additionalAudioInstances) {
                audioInstance.ChangeVolume(fromVolume, targetVolume, duration);
            }
        }

        public void ChangeVolume(float targetVolume, float duration) {
            if (!IsValid) return;
            track.ChangeVolume(Volume, targetVolume, duration);
            if (additionalAudioInstances == null) return;
            foreach (var audioInstance in additionalAudioInstances) {
                audioInstance.ChangeVolume(targetVolume, duration);
            }
        }

        public void DeductFromDelay(float time) {
            if (!IsValid) return;
            track.DeductFromDelay(time);
        }

        public void SyncWith(ITimeScaled timeScaled) {
            track.SyncWith(timeScaled);
            if (additionalAudioInstances == null) return;
            foreach (var audioInstance in additionalAudioInstances) {
                audioInstance.SyncWith(timeScaled);
            }
        }

        public override string ToString() {
            if (track == null) return "Track=null";
            return track.ToString();
        }
    }
}
