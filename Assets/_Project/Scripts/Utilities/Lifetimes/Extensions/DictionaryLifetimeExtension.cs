using System.Collections.Generic;

namespace Utilities.Lifetimes.Extensions
{
    public static class DictionaryLifetimeExtension
    {
        public static void AddLifetimed<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            Lifetime lifetime,
            TKey key,
            TValue value)
        {
            if (dictionary == null)
            {
                throw new System.ArgumentNullException(nameof(dictionary));
            }

            if (lifetime == null)
            {
                throw new System.ArgumentNullException(nameof(lifetime));
            }

            dictionary.Add(key, value);
            lifetime.OnTermination(() => dictionary.Remove(key));
        }
    }
}
