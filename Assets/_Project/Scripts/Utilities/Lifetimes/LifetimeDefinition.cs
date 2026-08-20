using System;

namespace Utilities.Lifetimes
{
    public sealed class LifetimeDefinition : IDisposable
    {
        public LifetimeDefinition()
            : this(new Lifetime())
        {
        }

        public LifetimeDefinition(Lifetime lifetime)
        {
            Lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        }

        public Lifetime Lifetime { get; }

        public void Terminate()
        {
            Lifetime.Terminate();
        }

        public void Dispose()
        {
            Terminate();
        }
    }
}
