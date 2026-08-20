using System;
using System.Collections.Generic;
using Utilities.Reactive;

namespace Utilities.Lifetimes.Extensions
{
    public static class ViewableMapLifetimeExtension
    {
        public static void AddLifetimed<TKey, TValue>(
            this IViewableMap<TKey, TValue> map,
            Lifetime lifetime,
            TKey key,
            TValue value)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (lifetime == null)
            {
                throw new ArgumentNullException(nameof(lifetime));
            }

            map.Add(new KeyValuePair<TKey, TValue>(key, value));
            lifetime.OnTermination(() => map.Remove(key));
        }

        public static void AddLifetimed<TKey, TValue>(
            this IViewableMap<TKey, TValue> map,
            Lifetime lifetime,
            KeyValuePair<TKey, TValue> pair)
        {
            map.AddLifetimed(lifetime, pair.Key, pair.Value);
        }
    }
}
