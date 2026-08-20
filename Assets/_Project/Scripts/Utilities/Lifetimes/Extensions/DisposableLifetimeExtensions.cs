using System;
using UnityEngine;

namespace Utilities.Lifetimes.Extensions
{
    public static class DisposableLifetimeExtensions
    {
        public static void AddTo(this IDisposable disposable, Lifetime lifetime)
        {
            if (disposable == null)
            {
                Debug.LogError("[DisposableLifetimeExtensions] IDisposable is null, binding skipped");
                return;
            }
            
            if (!lifetime.TryOnTermination(disposable.Dispose))
                disposable.Dispose();
        }
    }
}
