using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Lifetimes;
using Utilities.Reactive;

namespace Features.MaterialAccumulation.Input
{
    public interface IMaterialAccumulationInput
    {
        IReadonlyProperty<Vector2> Movement { get; }
        IReadonlyProperty<bool> IsAccumulating { get; }
    }

    public sealed class MaterialAccumulationInput : MonoBehaviour, IMaterialAccumulationInput, ILifetimeInitializable
    {
        [SerializeField] private InputActionReference _moveActionReference;
        [SerializeField] private InputActionReference _accumulateActionReference;

        private readonly ViewableProperty<Vector2> _movement = new();
        private readonly ViewableProperty<bool> _isAccumulating = new();
        
        private InputAction _moveAction;
        private InputAction _accumulateAction;

        public IReadonlyProperty<Vector2> Movement => _movement;
        public IReadonlyProperty<bool> IsAccumulating => _isAccumulating;

        public void Initialize(Lifetime lifetime)
        {
            if (_moveActionReference == null || _accumulateActionReference == null)
                throw new InvalidOperationException("MaterialAccumulationInput requires Move and Accumulate InputActionReferences.");

            _moveAction = _moveActionReference.action;
            _accumulateAction = _accumulateActionReference.action;
            _moveAction.performed += OnMovementChanged;
            _moveAction.canceled += OnMovementChanged;
            _accumulateAction.performed += OnAccumulationStarted;
            _accumulateAction.canceled += OnAccumulationCanceled;
            _moveAction.Enable();
            _accumulateAction.Enable();

            lifetime.OnTermination(() =>
            {
                _moveAction.performed -= OnMovementChanged;
                _moveAction.canceled -= OnMovementChanged;
                _accumulateAction.performed -= OnAccumulationStarted;
                _accumulateAction.canceled -= OnAccumulationCanceled;
                _moveAction.Disable();
                _accumulateAction.Disable();
                _movement.Dispose();
                _isAccumulating.Dispose();
            });
        }

        private void OnMovementChanged(InputAction.CallbackContext context)
        {
            _movement.Value = context.ReadValue<Vector2>();
        }

        private void OnAccumulationStarted(InputAction.CallbackContext context)
        {
            _isAccumulating.Value = true;
        }

        private void OnAccumulationCanceled(InputAction.CallbackContext context)
        {
            _isAccumulating.Value = false;
        }
    }
}