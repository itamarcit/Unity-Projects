using UnityEngine;
using System;

namespace Avrahamy.Math {
    /// <summary>
    /// Some ideas for more ease functions: http://easings.net/
    /// Doesn't contain any formula to make those curves, but it is a nice
    /// reference.
    /// </summary>
    public enum EaseType {
        Linear,
        Reverse,
        SmoothStart,
        SimpleSmoothStart,
        SmoothStop,
        Smooth,
        SoftSmooth,
        StrongSmoothStop,
        SmoothStopOvershoot,
    }

    public static class EaseUtils {
        public static Func<float, float> GetEaseFunction(EaseType ease) {
            switch (ease) {
            case EaseType.Reverse:
                return Reverse;
            case EaseType.SmoothStart:
                return SmoothStart;
            case EaseType.SimpleSmoothStart:
                return SimpleSmoothStart;
            case EaseType.SmoothStop:
                return SmoothStop;
            case EaseType.Smooth:
                return Smooth;
            case EaseType.SoftSmooth:
                return SoftSmooth;
            case EaseType.StrongSmoothStop:
                return SimpleSmoothStop;
            case EaseType.SmoothStopOvershoot:
                return SmoothStopOvershoot1;
            default:
                return Linear;
            }
        }

        public static Func<float, float> GetInverseEaseFunction(EaseType ease) {
            switch (ease) {
            case EaseType.Reverse:
                return Reverse;
            case EaseType.SmoothStart:
                return InverseSmoothStart;
            case EaseType.SimpleSmoothStart:
                return InverseSimpleSmoothStart;
            case EaseType.SmoothStop:
            case EaseType.StrongSmoothStop:
            case EaseType.SmoothStopOvershoot:
                return InverseSmoothStop;
            case EaseType.Smooth:
                return InverseSmooth;
            case EaseType.SoftSmooth:
                return InverseSoftSmooth;
            default:
                return Linear;
            }
        }

        public static float Linear(float percent) {
            return percent;
        }

        public static float Reverse(float percent) {
            return 1f - percent;
        }

        /// <summary>
        /// Starts slow and speeds up.
        /// </summary>
        public static float SmoothStart(float percent) {
            return 1f - Mathf.Cos(percent * Mathf.PI * 0.5f);
        }

        /// <summary>
        /// Starts slow and speeds up. A bit "stronger" than SmoothStart.
        /// </summary>
        public static float SimpleSmoothStart(float percent) {
            return percent * percent;
        }

        /// <summary>
        /// Use with caution!
        /// </summary>
        public static readonly Func<float, float> InverseSmoothStart = BinarySearch(SmoothStart);

        /// <summary>
        /// Starts slow and speeds up.
        /// </summary>
        /// <param name="strength">1 - linear, 2 - SimpleSmoothStart, The bigger the value
        /// the slower it starts</param>
        public static float SmoothStart(float percent, float strength) {
            return Mathf.Pow(percent, strength);
        }

        public static float InverseSimpleSmoothStart(float value) {
            return Mathf.Sqrt(value);
        }

        /// <summary>
        /// Starts fast and slows down.
        /// </summary>
        public static float SmoothStop(float percent) {
            return Mathf.Sin(percent * Mathf.PI * 0.5f);
        }

        /// <summary>
        /// Starts fast and slows down. "Stronger" than SmoothStop.
        /// </summary>
        public static float SimpleSmoothStop(float percent) {
            percent -= 1f;
            return percent * percent * percent + 1;
        }

        /// <summary>
        /// Use with caution!
        /// </summary>
        public static readonly Func<float, float> InverseSmoothStop = BinarySearch(SmoothStop);

        /// <summary>
        /// Starts slow, speeds up and slows down.
        /// </summary>
        public static float Smooth(float percent) {
            return percent * percent * (3f - 2f * percent);
        }

        /// <summary>
        /// Use with caution!
        /// </summary>
        public static readonly Func<float, float> InverseSmooth = BinarySearch(Smooth);

        public static float SoftSmooth(float percent) {
            return percent * percent * percent * (percent * (6f * percent - 15f) + 10f);
        }

        /// <summary>
        /// Starts fast, overshoots and slows down.
        /// </summary>
        public static float SmoothStopOvershoot(float percent, float overshootAmount) {
            percent -= 1f;
            return percent * percent * ((overshootAmount + 1f) * percent + overshootAmount) + 1f;
        }

        /// <summary>
        /// Starts fast, overshoots and slows down.
        /// Same as SmoothStopOvershoot with overshootAmount = 1.
        /// </summary>
        public static float SmoothStopOvershoot1(float percent) {
            percent -= 1f;
            return percent * percent * (2f * percent + 1f) + 1f;
        }

        /// <summary>
        /// Use with caution!
        /// </summary>
        public static readonly Func<float, float> InverseSoftSmooth = BinarySearch(SoftSmooth);

        /// <summary>
        /// Returns a function that allows to binary search a value over an ease
        /// function.
        /// </summary>
        private static Func<float, float> BinarySearch(Func<float, float> EaseFunc) {
            return delegate(float value) {
                var start = 0f;
                var end = 1f;
                float percent = 0f;
                for (int i = 0; i < 1000; i++) {
                    percent = (start + end) * 0.5f;
                    var currentValue = EaseFunc(percent);
                    if (Mathf.Approximately(currentValue, value)) {
                        return percent;
                    }
                    if (value < currentValue) {
                        end = percent;
                    } else {
                        start = percent;
                    }
                }
                return percent;
            };
        }
    }
}
