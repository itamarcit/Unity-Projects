using UnityEngine;

namespace Avrahamy.Math {
    public class LerpVector3 : Lerp<Vector3> {
        public LerpVector3() {
            // Empty on purpose.
        }

        /// <param name="unscaled">If set to <c>true</c> use unscaled time.</param>
        public LerpVector3(Vector3 from, Vector3 to, float duration, bool unscaled = true)
            : base(from, to, duration, unscaled) {}

        public override Vector3 GetValue() {
            var progress = timeLerp.GetValue();
            return Vector3.LerpUnclamped(from, to, progress);
        }
    }
}
