using UnityEngine;
using Avrahamy.Utils;

namespace Avrahamy.Audio {
    /// <summary>
    /// Wraps an AudioSource and allows manipulating it.
    /// It preserves the AudioEvent base volume when changing the volume level
    /// and allows changing its timeScale (through pitch).
    /// </summary>
    public class ExtendedAudioSource {
        protected readonly AudioSource source;
        protected AudioEvent audioEvent;
        private float timeScale;

        public AudioSource Source {
            get {
                return source;
            }
        }

        public AudioEvent AudioEvent {
            get {
                return audioEvent;
            }
            set {
                audioEvent = value;
            }
        }

        public float Volume {
            get {
                return source.volume / Mathf.Max(audioEvent.Volume, 0.01f);
            }
            set {
                DebugLog.Log(LogTag.Audio, $"{source} {audioEvent} Volume = {value}");
                source.volume = value * audioEvent.Volume;
            }
        }

        public bool IsPlaying {
            get {
                //DebugLog.LogError(LogTag.Audio, $"{source} {audioEvent} source.isPlaying = {source.isPlaying} startTime({startTime}) >= dspTime({AudioSettings.dspTime}) {startTime >= AudioSettings.dspTime} {startTime - AudioSettings.dspTime}");
                return source.isPlaying || startTime >= AudioSettings.dspTime;
            }
        }

        public bool IsLooping {
            get {
                return source.loop;
            }
            set {
                source.loop = value;
            }
        }

        public float Time {
            get {
                return source.time;
            }
            set {
                source.time = value;
            }
        }

        public float TimeScale {
            get {
                return timeScale;
            }
            set {
                //DebugLog.Log(LogTag.Audio, $"{source} {audioEvent} TimeScale {timeScale} => {value} dspTime = {AudioSettings.dspTime} startTime = {startTime} stopTime = {stopTime}");
                var newValue = Mathf.Max(value, 0.0001f);
                if (audioEvent != null && audioEvent.IsTimeScaled) {
                    source.pitch = pitch * newValue;
                }
                if (!Mathf.Approximately(timeScale, newValue)) {
                    var remainingTime = startTime - AudioSettings.dspTime;
                    if (remainingTime > 0) {
                        remainingTime *= timeScale / newValue;
                        startTime = remainingTime + AudioSettings.dspTime;
                        //DebugLog.Log(LogTag.Audio, $"{source} {audioEvent} Remaining time = {remainingTime} startTime = {startTime}");
                        source.SetScheduledStartTime(startTime);
                    }
                    remainingTime = stopTime - AudioSettings.dspTime;
                    if (remainingTime > 0) {
                        remainingTime *= timeScale / newValue;
                        stopTime = remainingTime + AudioSettings.dspTime;
                        //DebugLog.Log(LogTag.Audio, $"{source} {audioEvent} Remaining time = {remainingTime} stopTime = {stopTime}");
                        source.SetScheduledEndTime(stopTime);
                    }
                }
                timeScale = newValue;
            }
        }

        public float Pitch {
            get {
                return pitch;
            }
            set {
                pitch = value;
                if (audioEvent != null && audioEvent.IsTimeScaled) {
                    source.pitch = pitch * timeScale;
                } else {
                    source.pitch = pitch;
                }
            }
        }

        public double StartTime {
            get {
                return startTime;
            }
        }

        public double StopTime {
            get {
                return stopTime;
            }
        }

        public double TimeRemaining {
            get {
                if (stopTime >= AudioSettings.dspTime) {
                    return stopTime - AudioSettings.dspTime;
                }
                if (source.isPlaying) {
                    return source.clip.length - source.time;
                }
                return startTime >= AudioSettings.dspTime
                    ? startTime - AudioSettings.dspTime + source.clip.length
                    : 0;
            }
        }

        private float pitch;
        // This is the time to start playing in the AudioSettings.dspTime timeline.
        private double startTime;
        private double stopTime;

        public ExtendedAudioSource(AudioSource source) {
            this.source = source;
            timeScale = 1f;
        }

        public void Schedule(float delay) {
            DebugLog.Log(LogTag.Audio, $"{source} {audioEvent} Schedule {delay} * {timeScale}");
            startTime = AudioSettings.dspTime + delay * timeScale;
            source.PlayScheduled(startTime);
        }

        public void ScheduleStop(double delay) {
            DebugLog.Log(LogTag.Audio, $"{source} {audioEvent} Schedule Stop {delay} * {timeScale}");
            stopTime = AudioSettings.dspTime + delay * timeScale;
            if (source.IsNullOrDestroyed()) return;
            source.SetScheduledEndTime(stopTime);
        }

        public void UnSchedule() {
            DebugLog.Log(LogTag.Audio, $"{source} {audioEvent} UnSchedule");
            if (startTime >= AudioSettings.dspTime && !source.IsNullOrDestroyed()) {
                // Didn't play yet. Stop the source to un-schedule it.
                source.Stop();
            }
            startTime = -1;
        }

        public void UnScheduleStop() {
            DebugLog.Log(LogTag.Audio, $"{source} {audioEvent} UnScheduleStop");
            if (source.isPlaying && stopTime >= AudioSettings.dspTime) {
                source.SetScheduledEndTime(AudioSettings.dspTime + 99999);
            }
            stopTime = -1;
        }

        public void DeductFromDelay(float time) {
            DebugLog.Log(LogTag.Audio, $"{source} {audioEvent} DeductFromDelay {time} * {timeScale}");
            time *= timeScale;
            if (startTime >= AudioSettings.dspTime) {
                startTime -= time;
                source.SetScheduledStartTime(startTime);
            }
            if (stopTime >= AudioSettings.dspTime) {
                stopTime -= time;
                source.SetScheduledEndTime(stopTime);
            }
        }
    }
}
