using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities.Lifetimes;
using Utilities.Lifetimes.Extensions;
using Utilities.Reactive;

namespace View.Core
{
    public abstract class ListViewBase<T> : MonoBehaviour
    {
        private Func<Lifetime, T, UniTask<IListViewItem<T>>> _instanceFactory;
        
        public void Bind(Lifetime lifetime, IViewableList<T> source, Func<Lifetime, T, UniTask<IListViewItem<T>>> instanceFactory)
        {
            _instanceFactory = instanceFactory;
            gameObject.GetActiveLifetimeComponent().IsActive
                .WhenTrue(lifetime, activeLifetime => { source.View(activeLifetime, OnNewItem); });
        }

        private void OnNewItem(Lifetime lifetime, int index, T source)
        {
            if (lifetime.IsNotAlive)
                return;
            InstantiatePrefab(lifetime, index, source).ForgetSafely(lifetime);
        }

        private async UniTask InstantiatePrefab(Lifetime lifetime, int index, T source)
        {
            var item = await _instanceFactory.Invoke(lifetime, source);
            if (lifetime.IsNotAlive)
                return;

            if (item is not Component component)
                throw new InvalidOperationException($"List item '{item.GetType().Name}' must be a Unity component.");

            component.transform.SetSiblingIndex(index);
            item.Bind(lifetime, source);
        }
    }
}
