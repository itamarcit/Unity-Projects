using UnityEngine;
using System;

namespace Avrahamy.Math {
    [Serializable]
    public class MathematicalCurve {
        public AnimationCurve curve;

        private float[] samples;
        private float[] areaSamples;
        private float sampleEveryX;

        /// <summary>
        /// NOTE: 21 samples means sample every 0.05. The first one is at x=0
        /// and the last is at x=1.
        /// </summary>
        public float[] GetSamples(int samplesCount = 21) {
            if (samples != null && samples.Length == samplesCount) return samples;

            DebugAssert.Assert(samplesCount > 1, "At least 2 samples are required. Got " + samplesCount);
            samples = new float[samplesCount];
            for (var i = 0; i < samplesCount; i++) {
                samples[i] = curve.Evaluate(i / (samplesCount - 1f));
            }

            return samples;
        }

        public float[] GetAreaSamples(int samplesCount = 21) {
            if (areaSamples != null && areaSamples.Length == samplesCount) return areaSamples;

            GetSamples(samplesCount);
            areaSamples = new float[samplesCount];
            areaSamples[0] = 0f;
            sampleEveryX = 1f / (samplesCount - 1f);
            for (var i = 1; i < samplesCount; i++) {
                // Estimation of the area beneath the curve at the current interval.
                var area = (samples[i - 1] + samples[i]) / 2 * sampleEveryX;
                areaSamples[i] = areaSamples[i - 1] + area;
            }

            return areaSamples;
        }

        public float Evaluate(float t) {
            return curve.Evaluate(t);
        }

        public float EvaluateAreaUpTo(float t) {
            if (areaSamples == null) {
                GetAreaSamples();
            }

            t = Mathf.Min(t, 1f);
            var previousIntervalIndex = Mathf.FloorToInt(t / sampleEveryX);
            var previousInterval = previousIntervalIndex * sampleEveryX;
            var lerpDistance = Mathf.InverseLerp(previousInterval, previousInterval + sampleEveryX, t);

            return Mathf.LerpUnclamped(
                areaSamples[previousIntervalIndex],
                areaSamples[previousIntervalIndex + 1],
                lerpDistance);
        }
    }
}
