using UnityEngine;
using Utilities.Lifetimes;
using Zenject;
using Lifetime = Utilities.Lifetimes.Lifetime;

namespace View.Core
{
    public abstract class MonoPresenter : MonoBehaviour
    {
        protected IProtocolDispatcher ProtocolDispatcher;
        
        private LifetimeDefinition _definition;

        [Inject]
        private void SetDependencies(IProtocolDispatcher protocolDispatcher)
        {
            ProtocolDispatcher = protocolDispatcher;
        }

        protected virtual void OnEnable()
        {
            _definition = new();
            Setup(_definition.Lifetime);
        }

        protected virtual void OnDisable()
        {
            _definition.Terminate();
        }

        protected abstract void Setup(Lifetime lifetime);
    }
}
