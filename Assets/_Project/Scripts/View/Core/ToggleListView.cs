using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Reactive;
using Zenject;
using Object = UnityEngine.Object;
using Lifetime = Utilities.Lifetimes.Lifetime;

namespace View.Core
{
    /// <summary>
    /// ListView который автоматически добавляет Toggle элементы в ToggleGroup
    /// </summary>
    public abstract class ToggleListView<T> : ListViewBase<T>, IListView<T>
    {
        [SerializeField] private ToggleGroup _toggleGroup;
        
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
            
            // Автоматически добавляем Toggle в ToggleGroup
            if (_toggleGroup != null)
            {
                if (item is not IToggleListViewItem<T> toggleItem)
                    throw new InvalidOperationException($"List item '{item.GetType().Name}' must implement {nameof(IToggleListViewItem<T>)}.");

                toggleItem.Toggle.group = _toggleGroup;
            }
            
            return item;
        }
    }
}
