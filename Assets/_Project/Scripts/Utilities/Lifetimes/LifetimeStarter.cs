using UnityEngine;
using Utilities.Lifetimes.Extensions;
using Zenject;

namespace Utilities.Lifetimes
{
    public sealed class LifetimeStarter : MonoBehaviour, IInitializable
    {
        private LifetimeInitializer _lifetimeInitializer;

        [Inject]
        public void SetDependencies(LifetimeInitializer lifetimeInitializer)
        {
            _lifetimeInitializer = lifetimeInitializer;
        }

        public void Initialize() => _lifetimeInitializer.Initialize(gameObject.GetLifetime());
    }
}
