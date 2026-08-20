using System;
using System.Collections.Generic;

namespace Utilities.Lifetimes.Extensions
{
    public static class CollectionLifetimeExtension
    {
        public static void AddLifetimed<T>(this ICollection<T> collection, Lifetime lifetime, T item)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (lifetime == null)
            {
                throw new ArgumentNullException(nameof(lifetime));
            }

            collection.Add(item);
            lifetime.OnTermination(() => collection.Remove(item));
        }
    }
}
