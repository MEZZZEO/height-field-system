using System;
using R3;
using UnityEngine;

namespace View.Core
{
    public class VisibilityController : IDisposable
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly ReactiveProperty<bool> _isVisible;
        private bool _isDisposed;
        
        public ReadOnlyReactiveProperty<bool> IsVisible => _isVisible;
        
        public VisibilityController(CanvasGroup canvasGroup, bool initiallyVisible = false)
        {
            _canvasGroup = canvasGroup ?? throw new ArgumentNullException(nameof(canvasGroup));

            _isVisible = new ReactiveProperty<bool>(initiallyVisible);
            _isVisible.Subscribe(SetCanvasGroupVisible);
            
            SetCanvasGroupVisible(initiallyVisible);
        }
        
        public void SetVisible(bool visible)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("VisibilityController has been disposed");

            _isVisible.Value = visible;
        }
        
        public void ToggleVisible()
        {
            SetVisible(!_isVisible.Value);
        }
        
        public void Show()
        {
            SetVisible(true);
        }
        
        public void Hide()
        {
            SetVisible(false);
        }
        
        public void SetInteractable(bool interactable)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.interactable = interactable;
        }
        
        public void SetBlockRaycasts(bool blockRaycasts)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.blocksRaycasts = blockRaycasts;
        }
        
        public void SetAlpha(float alpha)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = Mathf.Clamp01(alpha);
        }
        
        public float GetAlpha()
        {
            return _canvasGroup?.alpha ?? 1f;
        }

        private void SetCanvasGroupVisible(bool visible)
        {
            if (_canvasGroup == null) return;
            
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isVisible?.Dispose();
            _isDisposed = true;
        }
    }
}
