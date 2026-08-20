using UnityEngine;
using Utilities.Reactive;

namespace Utilities.Lifetimes.Extensions
{
    public static class GameObjectActiveLifetimeExtensions
    {
        public static Lifetime GetActiveLifetime(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new System.ArgumentNullException(nameof(gameObject));
            }
            
            var component = gameObject.AddOrGetComponent<ActiveLifetimeComponent>();
            return component.Lifetime;
        }

        public static ActiveLifetimeComponent GetActiveLifetimeComponent(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogError("[GameObjectActiveLifetimeExtensions] GameObject is null");
                return null;
            }
            
            return gameObject.AddOrGetComponent<ActiveLifetimeComponent>();
        }

        [DefaultExecutionOrder(-100)]
        public class ActiveLifetimeComponent : MonoBehaviour
        {
            private readonly ViewableProperty<bool> _isActive = new(false);
            private Lifetime _lifetime;

            public IReadonlyProperty<bool> IsActive => _isActive;
            public Lifetime Lifetime => _lifetime;

            private void OnEnable()
            {
                _lifetime = gameObject.GetLifetime().CreateNested().Lifetime;
                _isActive.Value = true;
            }

            private void OnDisable()
            {
                _lifetime.Terminate();
                _isActive.Value = false;
            }
        }
    }
}
