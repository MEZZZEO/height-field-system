using UnityEngine;
using Zenject;

namespace Utilities.Lifetimes.Extensions
{
    public static class LifetimedInstatiateExtensions
    {
        public static GameObject Instantiate(this GameObject gameObject, Lifetime lifetime, Transform parent = null)
        {
            if (gameObject == null)
            {
                Debug.LogError("[LifetimedInstatiateExtensions] GameObject is null, instantiation skipped");
                return null;
            }
            
            return lifetime.Bracket(
                opening: () => Object.Instantiate(gameObject, parent),
                closing: Object.Destroy
            );
        }

        public static GameObject InstantiatePrefab(this DiContainer resolver, Lifetime lifetime, Object prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[LifetimedInstatiateExtensions] Prefab is null, instantiation skipped");
                return null;
            }
            
            return lifetime.Bracket(
                opening: () => resolver.InstantiatePrefab(prefab, parent),
                closing: Object.Destroy
            );
        }

        public static GameObject InstantiatePrefab(
            this DiContainer resolver, Lifetime lifetime, Object prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[LifetimedInstatiateExtensions] Prefab is null, instantiation skipped");
                return null;
            }
            
            return lifetime.Bracket(
                opening: () => resolver.InstantiatePrefab(prefab, position, rotation, parent),
                closing: Object.Destroy
            );
        }

        public static T InstantiatePrefabForComponent<T>(this DiContainer resolver, Lifetime lifetime, T prefab, Transform parent = null)
            where T : Component
        {
            if (prefab == null)
            {
                Debug.LogError("[LifetimedInstatiateExtensions] Prefab is null, instantiation skipped");
                return null;
            }
            
            return lifetime.Bracket(
                opening: () => resolver.InstantiatePrefabForComponent<T>(prefab, parent),
                closing: Object.Destroy);
        }
    }
}
