using UnityEngine;
using System;

namespace Avrahamy {
    [Serializable]
    public class PassiveRealTimeTimer : ITimer {
        [SerializeField] float duration;

        public float StartTime {
            get {
                return endTime - Duration;
            }
            set {
                endTime = value + Duration;
                if (!IsSet) {
                    // Just in case endTime was set to 0. This could happen when
                    // initializing timers from the state such as run timer.
                    endTime = float.Epsilon;
                }
            }
        }

        public float EndTime {
            get {
                return endTime;
            }
            set {
                endTime = value;
            }
        }

        public float Duration {
            get {
                return duration / timeScale;
            }
            set {
                duration = value;
            }
        }

        public float UnscaledDuration {
            get {
                return duration;
            }
        }

        public float ElapsedTime {
            get {
                return IsSet ? Time.realtimeSinceStartup - StartTime : 0f;
            }
            set {
                StartTime = Time.realtimeSinceStartup - value;
            }
        }

        public float RemainingTime {
            get {
                return IsSet
                    ? IsActive
                        ? endTime - Time.realtimeSinceStartup
                        : 0f
                    : Duration;
            }
            set {
                endTime = value + Time.realtimeSinceStartup;
            }
        }

        public bool IsActive {
            get {
                return Time.realtimeSinceStartup < endTime;
            }
        }

        public bool IsSet {
            get {
                return endTime > 0f || endTime < -1f;
            }
        }

        public float Progress {
            get {
                return IsSet && Duration > 0
                    ? ElapsedTime / Duration
                    : 0f;
            }
            set {
                ElapsedTime = value * Duration;
            }
        }

        public float ClampedProgress {
            get {
                return Mathf.Clamp01(Progress);
            }
        }

        public float TimeScale {
            get {
                return timeScale;
            }
            set {
                var newValue = Mathf.Max(value, 0.0001f);
                if (IsActive && !Mathf.Approximately(timeScale, newValue)) {
                    RemainingTime *= timeScale / newValue;
                }
                timeScale = newValue;
            }
        }

        private float endTime;
        private float timeScale = 1f;

        public PassiveRealTimeTimer() {}

        public PassiveRealTimeTimer(float duration) {
            this.duration = duration;
        }

        public void Start() {
            this.endTime = Time.realtimeSinceStartup + Duration;
        }

        public void Start(float duration) {
            this.duration = duration;
            this.endTime = Time.realtimeSinceStartup + Duration;
        }

        public void Clear() {
            this.endTime = 0f;
        }
    }
}