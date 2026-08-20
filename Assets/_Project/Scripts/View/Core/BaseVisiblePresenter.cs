using R3;
using UnityEngine;

namespace View.Core
{
    public abstract class BaseVisiblePresenter : MonoPresenter
    {
        [SerializeField] private bool _initiallyVisible;
        [SerializeField] private CanvasGroup _canvasGroup;

        protected VisibilityController VisibilityController { get; private set; }
        public ReadOnlyReactiveProperty<bool> IsVisible => VisibilityController.IsVisible;
        public bool Visible => VisibilityController.IsVisible.CurrentValue;
        
        protected override void OnEnable()
        {
            if (VisibilityController == null)
            {
                VisibilityController = new VisibilityController(_canvasGroup, _initiallyVisible);
            }

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            VisibilityController?.Dispose();
            VisibilityController = null;
        }
        
        public virtual void Show()
        {
            VisibilityController?.Show();
        }
        
        public virtual void Hide()
        {
            VisibilityController?.Hide();
        }
        
        public virtual void ToggleVisible()
        {
            VisibilityController?.ToggleVisible();
        }
        
        public virtual void SetVisible(bool visible)
        {
            VisibilityController?.SetVisible(visible);
        }
        
        public virtual void SetInteractable(bool interactable)
        {
            VisibilityController?.SetInteractable(interactable);
        }
        
        public virtual void SetBlockRaycasts(bool blockRaycasts)
        {
            VisibilityController?.SetBlockRaycasts(blockRaycasts);
        }
        
        public virtual void SetAlpha(float alpha)
        {
            VisibilityController?.SetAlpha(alpha);
        }
        
        public virtual float GetAlpha()
        {
            return VisibilityController?.GetAlpha() ?? 1f;
        }
        
        protected virtual void OnDestroy()
        {
            VisibilityController?.Dispose();
        }
    }
}
