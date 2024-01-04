using UnityEngine;

namespace Avrahamy.Math {
    public class LerpVector2 : Lerp<Vector2> {
        public LerpVector2() {
            // Empty on purpose.
        }

        /// <param name="unscaled">If set to <c>true</c> use unscaled time.</param>
        public LerpVector2(Vector2 from, Vector2 to, float duration, bool unscaled = true)
            : base(from, to, duration, unscaled) {}

        public override Vector2 GetValue() {
            var progress = timeLerp.GetValue();
            return Vector2.LerpUnclamped(from, to, progress);
        }

        public static Vector2 SlerpUnclamped(Vector2 from, Vector2 to, float t) {
            return Vector3.SlerpUnclamped(from, to, t);
        }
    }
}
