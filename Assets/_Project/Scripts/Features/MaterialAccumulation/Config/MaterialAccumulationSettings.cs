using System;
using UnityEngine;

namespace Features.MaterialAccumulation.Config
{
    [CreateAssetMenu(fileName = "MaterialAccumulationSettings", menuName = "Material Accumulation/Settings")]
    public sealed class MaterialAccumulationSettings : ScriptableObject
    {
        [Header("Surface")]
        [SerializeField, Min(2)] private int _resolutionX = 128;
        [SerializeField, Min(2)] private int _resolutionZ = 128;
        [SerializeField, Min(0.01f)] private float _sizeX = 20f;
        [SerializeField, Min(0.01f)] private float _sizeZ = 20f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _moveSpeed = 3f;

        [Header("Radius")]
        [SerializeField, Min(0f)] private float _baseRadius = 1.25f;
        [SerializeField, Min(0f)] private float _amplitude = 0.85f;
        [SerializeField, Min(0f)] private float _frequency = 1.2f;
        [SerializeField] private AnimationCurve _radiusCurve =
            new AnimationCurve(new Keyframe(0f, 0.35f), new Keyframe(0.25f, 0.85f), new Keyframe(0.65f, 0.02f), new Keyframe(1f, 0.8f));

        [Header("Accumulation")]
        [SerializeField, Min(0f)] private float _accumulationSpeed = 2f;
        [SerializeField, Min(0.01f)] private float _sweepStepFactor = 0.5f;

        public int ResolutionX => _resolutionX;
        public int ResolutionZ => _resolutionZ;
        public Vector2 SurfaceSize => new(_sizeX, _sizeZ);
        public float MoveSpeed => _moveSpeed;
        public float BaseRadius => _baseRadius;
        public float Amplitude => _amplitude;
        public float Frequency => _frequency;
        public AnimationCurve RadiusCurve => _radiusCurve;
        public float AccumulationSpeed => _accumulationSpeed;
        public float SweepStepFactor => _sweepStepFactor;

        public void Validate()
        {
            if (_resolutionX < 2 || _resolutionZ < 2)
                throw new InvalidOperationException("Material accumulation resolution must be at least 2x2.");

            if (_sizeX <= 0f || _sizeZ <= 0f)
                throw new InvalidOperationException("Material accumulation surface size must be positive.");

            if (_moveSpeed < 0f || _baseRadius < 0f || _amplitude < 0f || _frequency < 0f ||
                _accumulationSpeed < 0f || _sweepStepFactor <= 0f)
            {
                throw new InvalidOperationException("Material accumulation settings contain a negative or zero value.");
            }

            if (_radiusCurve == null)
                throw new InvalidOperationException("Material accumulation radius curve is not assigned.");

            foreach (var key in _radiusCurve.keys)
            {
                if (key.time < 0f || key.time > 1f || key.value < 0f || key.value > 1f)
                    throw new InvalidOperationException("Material accumulation radius curve keys must stay in the 0..1 range.");
            }
        }

        private void OnValidate()
        {
            _resolutionX = Mathf.Max(2, _resolutionX);
            _resolutionZ = Mathf.Max(2, _resolutionZ);
            _sizeX = Mathf.Max(0.01f, _sizeX);
            _sizeZ = Mathf.Max(0.01f, _sizeZ);
            _moveSpeed = Mathf.Max(0f, _moveSpeed);
            _baseRadius = Mathf.Max(0f, _baseRadius);
            _amplitude = Mathf.Max(0f, _amplitude);
            _frequency = Mathf.Max(0f, _frequency);
            _accumulationSpeed = Mathf.Max(0f, _accumulationSpeed);
            _sweepStepFactor = Mathf.Max(0.01f, _sweepStepFactor);

            if (_radiusCurve == null)
                return;

            var keys = _radiusCurve.keys;
            for (var i = 0; i < keys.Length; i++)
            {
                keys[i].time = Mathf.Clamp01(keys[i].time);
                keys[i].value = Mathf.Clamp01(keys[i].value);
            }

            _radiusCurve.keys = keys;
        }
    }
}