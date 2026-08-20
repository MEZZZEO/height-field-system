using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Lifetimes;
using Utilities.Lifetimes.Extensions;
using Zenject;
using Lifetime = Utilities.Lifetimes.Lifetime;

namespace View.Core
{
    public interface IProtocolDispatcher
    {
        TProtocol Get<TProtocol>(Lifetime lifetime) where TProtocol : IProtocol;
    }

    public class ProtocolDispatcher : IProtocolDispatcher
    {
        private static readonly ConcurrentDictionary<Type, Type> ProtocolTypesByInteractorType = new();

        private readonly Dictionary<Type, IInteractor> _interactors = new();
        private readonly Dictionary<Type, ProtocolInfo> _activeProtocols = new();

        private readonly IProtocolDispatcher _parentDispatcher;

        public ProtocolDispatcher([InjectLocal] List<IInteractor> interactors, DiContainer container)
        {
            _parentDispatcher = container.ParentContainers.Length == 0
                ? null
                : container.ParentContainers[0].TryResolve<IProtocolDispatcher>();
            FillInteractorsDictionary(interactors);
        }

        private void FillInteractorsDictionary(IReadOnlyList<IInteractor> interactors)
        {
            foreach (var interactor in interactors)
            {
                try
                {
                    var protocol = ProtocolTypesByInteractorType.GetOrAdd(
                        interactor.GetType(),
                        ResolveProtocolType);

                    _interactors[protocol] = interactor;
                }
                catch (Exception)
                {
                    Debug.Log(
                        $"Interactor {interactor.GetType()} must implement one and only one {typeof(IInteractor<>)} interface");
                }
            }
        }

        private static Type ResolveProtocolType(Type interactorType)
        {
            Type protocolType = null;
            var interfaces = interactorType.GetInterfaces();
            for (var i = 0; i < interfaces.Length; i++)
            {
                var interfaceType = interfaces[i];
                if (!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != typeof(IInteractor<>))
                    continue;

                var genericArguments = interfaceType.GetGenericArguments();
                if (genericArguments.Length != 1)
                    continue;

                if (protocolType != null)
                    throw new InvalidOperationException(
                        $"Interactor {interactorType} must implement one and only one {typeof(IInteractor<>)} interface");

                protocolType = genericArguments[0];
            }

            if (protocolType == null)
                throw new InvalidOperationException(
                    $"Interactor {interactorType} must implement one and only one {typeof(IInteractor<>)} interface");

            return protocolType;
        }

        public TProtocol Get<TProtocol>(Lifetime lifetime) where TProtocol : IProtocol
        {
            if (TryGetOrActivateProtocol<TProtocol>(out var protocolInfo, lifetime))
            {
                return (TProtocol)protocolInfo.Protocol;
            }

            if (_parentDispatcher != null)
            {
                return _parentDispatcher.Get<TProtocol>(lifetime);
            }

            throw new InvalidOperationException(
                $"Protocol {typeof(TProtocol)} cannot be resolved by {nameof(ProtocolDispatcher)}");
        }

        private bool TryGetOrActivateProtocol<TProtocol>(out ProtocolInfo protocolInfo, Lifetime lifetime) where TProtocol : IProtocol
        {
            var protocolType = typeof(TProtocol);
            if (_activeProtocols.TryGetValue(protocolType, out protocolInfo))
            {
                protocolInfo.IncreaseReferenceCount(lifetime);
                return true;
            }

            if (!_interactors.TryGetValue(protocolType, out var interactor))
            {
                return false;
            }

            var lifetimeDefinition = new LifetimeDefinition();
            protocolInfo = new ProtocolInfo
            {
                LifetimeDefinition = lifetimeDefinition,
                Protocol = ((IInteractor<TProtocol>)interactor).Get(lifetimeDefinition.Lifetime)
            };
            _activeProtocols.AddLifetimed(lifetimeDefinition.Lifetime, protocolType, protocolInfo);
            
            protocolInfo.IncreaseReferenceCount(lifetime);
            return true;
        }
        
        private class ProtocolInfo
        {
            public IProtocol Protocol;
            public LifetimeDefinition LifetimeDefinition;

            private int _referenceCount;

            public void IncreaseReferenceCount(Lifetime lifetime)
            {
                _referenceCount++;
                lifetime.OnTermination(DecreaseReferenceCount);
            }

            private void DecreaseReferenceCount()
            {
                _referenceCount--;
                if (_referenceCount == 0)
                {
                    LifetimeDefinition.Terminate();
                }
            }
        }
    }
}
