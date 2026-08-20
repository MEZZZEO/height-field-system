using UnityEngine;

namespace Features.MaterialAccumulation.Runtime
{
    public sealed class RadiusProvider
    {
        private const float RadiusEpsilon = 0.0001f;
        private readonly AccumulationRuntimeSettings _settings;

        public RadiusProvider(AccumulationRuntimeSettings settings)
        {
            _settings = settings;
        }

        public float Evaluate(float time)
        {
            var phase = Mathf.Repeat(time * _settings.Frequency.Value, 1f);
            var curveValue = _settings.RadiusCurve.Evaluate(phase);
            var radius = _settings.BaseRadius.Value + _settings.Amplitude.Value * curveValue;
            return Mathf.Max(RadiusEpsilon, radius);
        }
    }
}
