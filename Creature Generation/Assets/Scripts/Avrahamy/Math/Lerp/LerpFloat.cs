using UnityEngine;

namespace Avrahamy.Math {
    public class LerpFloat : Lerp<float> {
        public LerpFloat() {
            // Empty on purpose.
        }

        /// <param name="unscaled">If set to <c>true</c> use unscaled time.</param>
        public LerpFloat(float from, float to, float duration, bool unscaled = true)
            : base(from, to, duration, unscaled) {}

        public override float GetValue() {
            var progress = timeLerp.GetValue();
            return Mathf.LerpUnclamped(from, to, progress);
        }
    }
}
