using UnityEngine;
using System;

namespace Avrahamy.Math {
    public class TimeLerp {
        private float duration;
        private float startTime;
        private bool unscaled = true;

        public Func<float> GetValue;
        public bool HasReachedTarget {get; private set;}

        public TimeLerp() {
            GetValue = GatValueUnscaled;
            HasReachedTarget = true;
        }

        /// <param name="unscaled">If set to <c>true</c> use unscaled time.</param>
        public TimeLerp(float duration, bool unscaled = true) {
            Reset(duration, unscaled);
        }

        public void Stop() {
            HasReachedTarget = true;
        }

        public void Reset(float duration, bool unscaled = true) {
            this.duration = duration;
            this.unscaled = unscaled;

            GetValue = this.unscaled ? GatValueUnscaled : (Func<float>)GetValueScaled;
            startTime = this.unscaled ? Time.realtimeSinceStartup : Time.timeSinceLevelLoad;
            HasReachedTarget = Mathf.Approximately(duration, 0f);
        }

        public void SetProgress(float progress) {
            var currentTime = unscaled ? Time.realtimeSinceStartup : Time.timeSinceLevelLoad;
            startTime = currentTime - progress * duration;
        }

        private float GatValueUnscaled() {
            if (HasReachedTarget) return 1f;
            var currentTime = Time.realtimeSinceStartup;
            return GetValueAtTime(currentTime);
        }

        private float GetValueScaled() {
            if (HasReachedTarget) return 1f;
            var currentTime = Time.timeSinceLevelLoad;
            return GetValueAtTime(currentTime);
        }

        private float GetValueAtTime(float currentTime) {
            var progress = (currentTime - startTime) / duration;
            if (progress >= 1f) {
                HasReachedTarget = true;
                return 1f;
            }
            HasReachedTarget = Mathf.Approximately(progress, 1f);
            return progress;
        }
    }
}
