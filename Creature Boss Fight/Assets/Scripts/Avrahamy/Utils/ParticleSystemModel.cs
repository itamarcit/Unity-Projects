using Avrahamy.Math;
using System;
using UnityEngine;

namespace Avrahamy.Utils {
    [Serializable]
    public class ParticleSystemModel {
        [SerializeField] FloatRange size;
        [SerializeField] FloatRange speed;
        [SerializeField] FloatRange lifetime;
        [SerializeField] FloatRange rateOverTime;
        [SerializeField] FloatRange rateOverDistance;

        public void ToParticleSystem(ParticleSystem ps) {
            var main = ps.main;
            main.startSize = ToMinMaxCurve(main.startSize, size);
            main.startSpeed = ToMinMaxCurve(main.startSpeed, speed);
            main.startLifetime = ToMinMaxCurve(main.startLifetime, lifetime);
            var emission = ps.emission;
            emission.rateOverTime = ToMinMaxCurve(emission.rateOverTime, rateOverTime);
            emission.rateOverDistance = ToMinMaxCurve(emission.rateOverDistance, rateOverDistance);
        }

        public void FromParticleSystem(ParticleSystem ps) {
            var main = ps.main;
            size = FromMinMaxCurve(main.startSize, size, "start size");
            speed = FromMinMaxCurve(main.startSpeed, speed, "start speed");
            lifetime = FromMinMaxCurve(main.startLifetime, lifetime, "start lifetime");

            var emission = ps.emission;
            rateOverTime = FromMinMaxCurve(emission.rateOverTime, rateOverTime, "rate over time");
            rateOverDistance = FromMinMaxCurve(emission.rateOverDistance, rateOverDistance, "rate over distance");
        }

        public static void Lerp(ParticleSystem ps, ParticleSystemModel a, ParticleSystemModel b, float t) {
            t = Mathf.Clamp01(t);
            var main = ps.main;
            main.startSize = ToMinMaxCurve(main.startSize, FloatRange.LerpUnclamped(a.size, b.size, t));
            main.startSpeed = ToMinMaxCurve(main.startSpeed, FloatRange.LerpUnclamped(a.speed, b.speed, t));
            main.startLifetime = ToMinMaxCurve(main.startLifetime, FloatRange.LerpUnclamped(a.lifetime, b.lifetime, t));
            var emission = ps.emission;
            emission.rateOverTime = ToMinMaxCurve(emission.rateOverTime, FloatRange.LerpUnclamped(a.rateOverTime, b.rateOverTime, t));
            emission.rateOverDistance = ToMinMaxCurve(emission.rateOverDistance, FloatRange.LerpUnclamped(a.rateOverDistance, b.rateOverDistance, t));
        }

        private static ParticleSystem.MinMaxCurve ToMinMaxCurve(ParticleSystem.MinMaxCurve minMax, FloatRange range) {
            if (Mathf.Approximately(range.min, range.max)) {
                minMax.mode = ParticleSystemCurveMode.Constant;
                minMax.constant = range.min;
            } else {
                minMax.mode = ParticleSystemCurveMode.TwoConstants;
                minMax.constantMin = range.min;
                minMax.constantMax = range.max;
            }
            return minMax;
        }

        private static FloatRange FromMinMaxCurve(ParticleSystem.MinMaxCurve minMax, FloatRange range, string name) {
            switch (minMax.mode) {
                case ParticleSystemCurveMode.Constant:
                    range.min = minMax.constant;
                    range.max = minMax.constant;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    range.min = minMax.constantMin;
                    range.max = minMax.constantMax;
                    break;
                default:
                    DebugLog.LogError($"Unsupported {name} mode {minMax.mode}");
                    break;
            }
            return range;
        }
    }
}
