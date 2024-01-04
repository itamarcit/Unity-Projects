using UnityEngine;
using System;

namespace Avrahamy.Math {
    [Serializable]
    public struct FloatRange {
        public float min;
        public float max;

        public float Average {
            get {
                return (min + max) / 2f;
            }
        }

        public float Size {
            get {
                return max - min;
            }
        }

        public FloatRange(float defaultValue) : this(defaultValue, defaultValue) {}

        public FloatRange(float min, float max) {
            this.min = min;
            this.max = max;
        }

        public static FloatRange operator+(FloatRange lhs, float value) {
            return new FloatRange(lhs.min + value, lhs.max + value);
        }

        public static FloatRange operator-(FloatRange lhs, float value) {
            return new FloatRange(lhs.min - value, lhs.max - value);
        }

        public float Clamp(float value) {
            DebugAssert.Assert(min <= max, $"FloatRange has min({min}) > max({max})");
            return Mathf.Clamp(value, min, max);
        }

        public float Lerp(float t) {
            return Mathf.Lerp(min, max, t);
        }

        public float InverseLerp(float value) {
            return Mathf.InverseLerp(min, max, value);
        }

        public float LerpUnclamped(float t) {
            return Mathf.LerpUnclamped(min, max, t);
        }

        public float InverseLerpUnclamped(float value) {
            return MathsUtils.InverseLerpUnclamped(min, max, value);
        }

        public float Remap(float value, float toMin, float toMax) {
            return MathsUtils.Remap(value, this, toMin, toMax);
        }

        public float Remap(float value, FloatRange to) {
            return MathsUtils.Remap(value, this, to);
        }

        /// <summary>
        /// Inclusive.
        /// </summary>
        public bool IsInRange(float value) {
            DebugAssert.Assert(min <= max, $"FloatRange has min({min}) > max({max})");
            return min <= value && value <= max;
        }

        public static FloatRange Lerp(FloatRange a, FloatRange b, float t) {
            return new FloatRange {
                min = Mathf.Lerp(a.min, b.min, t),
                max = Mathf.Lerp(a.max, b.max, t),
            };
        }

        public static FloatRange LerpUnclamped(FloatRange a, FloatRange b, float t) {
            return new FloatRange {
                min = Mathf.LerpUnclamped(a.min, b.min, t),
                max = Mathf.LerpUnclamped(a.max, b.max, t),
            };
        }
    }
}
