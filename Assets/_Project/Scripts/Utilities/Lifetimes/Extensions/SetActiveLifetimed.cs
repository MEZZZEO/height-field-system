using System;
using UnityEngine;
using Utilities.Reactive;

namespace Utilities.Lifetimes.Extensions
{
    public static class SetActiveLifetimedExtension
    {
        public static void SetActiveLifetimed(this GameObject gameObject, Lifetime lifetime)
        {
            if (gameObject == null)
            {
                Debug.LogError("[SetActiveLifetimedExtension] GameObject is null, binding skipped");
                return;
            }
            
            gameObject.SetActive(true);
            lifetime.OnTermination(() => gameObject.SetActive(false));
        }

        public static void SetActiveWhileTrue(this GameObject gameObject, Lifetime lifetime, IReadonlyProperty<bool> property)
        {
            if (gameObject == null)
            {
                Debug.LogError("[SetActiveLifetimedExtension] GameObject is null, binding skipped");
                return;
            }
            
            var gameObjectLifetime = gameObject.GetLifetime();
            var intersectedLifetime = gameObjectLifetime.Intersect(lifetime);

            property.Advise(intersectedLifetime, value =>
            {
                try
                {
                    gameObject.SetActive(value);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
        }

        public static void SetActiveWhileFalse(this GameObject gameObject, Lifetime lifetime, IReadonlyProperty<bool> property)
        {
            if (gameObject == null)
            {
                Debug.LogError("[SetActiveLifetimedExtension] GameObject is null, binding skipped");
                return;
            }
            
            var gameObjectLifetime = gameObject.GetLifetime();
            var intersectedLifetime = gameObjectLifetime.Intersect(lifetime);

            property.Advise(intersectedLifetime, value =>
            {
                try
                {
                    gameObject.SetActive(!value);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
        }
    }
}
