using Features.MaterialAccumulation.Core;
using Features.MaterialAccumulation.Input;
using Features.MaterialAccumulation.View;
using R3;
using UnityEngine;
using Utilities.Lifetimes;
using Utilities.Lifetimes.Extensions;
using Utilities.Reactive;

namespace Features.MaterialAccumulation.Runtime
{
    public sealed class MaterialAccumulationController : ILifetimeInitializable
    {
        private AccumulationRuntimeSettings _settings;
        private IMaterialAccumulationInput _input;
        private HeightFieldMeshView _meshView;
        private MaterialAccumulationZoneView _zoneView;
        private HeightField _heightField;
        private RadiusProvider _radiusProvider;
        private Vector2 _movement;
        private bool _isAccumulating;
        private Vector2 _previousPosition;
        private float _previousRadius;
        private float _elapsedTime;
        private bool _initialized;

        public MaterialAccumulationController(
            AccumulationRuntimeSettings settings,
            IMaterialAccumulationInput input,
            HeightFieldMeshView meshView,
            MaterialAccumulationZoneView zoneView)
        {
            _settings = settings;
            _input = input;
            _meshView = meshView;
            _zoneView = zoneView;
            _heightField = meshView.HeightField;
            _radiusProvider = new RadiusProvider(settings);
        }

        public void Initialize(Lifetime lifetime)
        {
            _input.Movement.Advise(lifetime, value => _movement = value);
            _input.IsAccumulating.Advise(lifetime, value => _isAccumulating = value);

            var position = GetZonePosition();
            _previousPosition = ClampToSurface(position);
            SetZonePosition(_previousPosition);
            _previousRadius = _radiusProvider.Evaluate(_elapsedTime);
            _zoneView.SetRadius(_previousRadius);
            
            _initialized = true;

            Observable.EveryUpdate()
                .Subscribe(_ => Tick(Time.deltaTime))
                .AddTo(lifetime);

            lifetime.OnTermination(() => _initialized = false);
        }

        public void ResetSurface()
        {
            if (!_initialized)
                throw new System.InvalidOperationException("MaterialAccumulationController is not initialized.");

            _heightField.Reset();
            _meshView.Apply();
            _previousPosition = GetZonePosition();
            _previousRadius = _radiusProvider.Evaluate(_elapsedTime);
        }

        private void Tick(float deltaTime)
        {
            if (!_initialized || deltaTime <= 0f)
                return;

            var direction = _movement;
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            var currentPosition = ClampToSurface(
                _previousPosition + direction * (_settings.MoveSpeed.Value * deltaTime));
            SetZonePosition(currentPosition);

            _elapsedTime += deltaTime;
            var currentRadius = _radiusProvider.Evaluate(_elapsedTime);
            _zoneView.SetRadius(currentRadius);

            if (_isAccumulating)
            {
                _heightField.AddSweep(
                    _previousPosition,
                    currentPosition,
                    _previousRadius,
                    currentRadius,
                    _settings.AccumulationSpeed.Value,
                    deltaTime,
                    _settings.SweepStepFactor);
                _meshView.Apply();
            }

            _previousPosition = currentPosition;
            _previousRadius = currentRadius;
        }

        private Vector2 GetZonePosition()
        {
            var worldPosition = _zoneView.transform.position;
            return new Vector2(worldPosition.x, worldPosition.z);
        }

        private void SetZonePosition(Vector2 position)
        {
            var worldPosition = _zoneView.transform.position;
            _zoneView.transform.position = new Vector3(position.x, worldPosition.y, position.y);
        }

        private Vector2 ClampToSurface(Vector2 position)
        {
            var origin = _heightField.Origin;
            var size = _heightField.SurfaceSize;
            return new Vector2(
                Mathf.Clamp(position.x, origin.x, origin.x + size.x),
                Mathf.Clamp(position.y, origin.y, origin.y + size.y));
        }
    }
}
