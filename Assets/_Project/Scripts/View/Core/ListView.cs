using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities.Reactive;
using Zenject;
using Object = UnityEngine.Object;
using Lifetime = Utilities.Lifetimes.Lifetime;

namespace View.Core
{
    public interface IListView<T>
    {
        void Bind(Lifetime lifetime, IViewableList<T> source, Func<Lifetime, T, UniTask<GameObject>> prefabFactory = null);
    }

    public interface IListViewItem<in T>
    {
        void Bind(Lifetime lifetime, T source);
    }

    public interface IToggleListViewItem<in T> : IListViewItem<T>
    {
        UnityEngine.UI.Toggle Toggle { get; }
    }

    public abstract class ListView<T> : ListViewBase<T>, IListView<T>
    {
        private GameObject _prefab;
        private Func<Lifetime, T, UniTask<GameObject>> _prefabFactory;

        [Inject] private DiContainer _resolver;

        public new void Bind(Lifetime lifetime, IViewableList<T> source, Func<Lifetime, T, UniTask<GameObject>> prefabFactory = null)
        {
            prefabFactory ??= (_, _) => UniTask.FromResult(_prefab);
            _prefabFactory = prefabFactory;
            if (transform.childCount > 0)
            {
                _prefab = transform.GetChild(0).gameObject;
                _prefab.SetActive(false);
            }

            base.Bind(lifetime, source, CreateInstance);
        }

        private async UniTask<IListViewItem<T>> CreateInstance(Lifetime lifetime, T source)
        {
            var prefab = await _prefabFactory.Invoke(lifetime, source);
            var item = _resolver.InstantiatePrefabForComponent<IListViewItem<T>>(prefab, transform);
            if (item is not Component component)
                throw new InvalidOperationException($"List item '{item.GetType().Name}' must be a Unity component.");

            lifetime.OnTermination(() => Object.Destroy(component.gameObject));
            component.gameObject.SetActive(true);
            return item;
        }
    }
}
