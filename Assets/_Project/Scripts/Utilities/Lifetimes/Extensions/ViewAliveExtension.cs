using System;
using Utilities.Reactive;

namespace Utilities.Lifetimes.Extensions
{
    public static class ViewAliveExtension
    {
        public static void ViewAlive(this IReadonlyProperty<Lifetime> me, Lifetime lifetime, Action<Lifetime> handler)
        {
            if (!lifetime.IsAlive) return;

            Lifetime current = null;
            
            me.Advise(lifetime, v =>
            {
                current?.Terminate();
                if (v.IsAlive)
                {
                    current = lifetime.CreateNested().Lifetime.Intersect(v);
                    handler(current);
                }
            });

            lifetime.OnTermination(() => current?.Terminate());
        }
    }
}
