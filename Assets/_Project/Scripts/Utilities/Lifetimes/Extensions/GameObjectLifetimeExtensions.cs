using UnityEngine;

namespace Utilities.Lifetimes.Extensions
{
    public static class GameObjectLifetimeExtensions
    {
        public static Lifetime GetLifetime(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new System.ArgumentNullException(nameof(gameObject));
            }
            
            return gameObject.AddOrGetComponent<LifetimeComponent>().Lifetime;
        }

        private sealed class LifetimeComponent : MonoBehaviour
        {
            private readonly LifetimeDefinition _definition = new();
            public Lifetime Lifetime => _definition.Lifetime;

            private void OnDestroy()
            {
                _definition.Terminate();
            }
        }
    }
}
