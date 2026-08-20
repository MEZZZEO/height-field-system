using System;
using Utilities.Reactive;

namespace Utilities.Lifetimes.Extensions
{
    public static class ViewableSetLifetimeExtension
    {
        public static void AddLifetimed<T>(this IViewableSet<T> set, Lifetime lifetime, T value)
        {
            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            if (lifetime == null)
            {
                throw new ArgumentNullException(nameof(lifetime));
            }

            set.Add(value);
            lifetime.OnTermination(() => set.Remove(value));
        }
    }
}
