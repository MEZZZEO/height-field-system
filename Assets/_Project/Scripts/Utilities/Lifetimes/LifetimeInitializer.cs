using System;
using System.Collections.Generic;
using Zenject;

namespace Utilities.Lifetimes
{
    public class LifetimeInitializer
    {
        private readonly List<ILifetimeInitializable> _initializables;
        private bool _hasInitialized;

        public LifetimeInitializer([InjectLocal] List<ILifetimeInitializable> initializables)
        {
            _initializables = initializables;
        }
        
        public void Initialize(Lifetime lifetime)
        {
            if (_hasInitialized)
                throw new InvalidOperationException("LifetimeInitializer can only be initialized once");
            _hasInitialized = true;

#if UNITY_EDITOR
            var types = new HashSet<Type>();
            foreach (var initializable in _initializables)
                if (!types.Add(initializable.GetType()))
                    throw new InvalidOperationException($"Found duplicate ILifetimeInitializable with type '{initializable.GetType()}'");
#endif

            foreach (var initializable in _initializables)
            {
                try
                {
#if ZEN_INTERNAL_PROFILING
                    using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
                    {
                        initializable.Initialize(lifetime);
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        $"Error occurred while initializing ILifetimeInitializable with type '{initializable.GetType()}'", e);
                }
            }
        }
    }
}
