using System;
using System.Collections;
using System.Collections.Generic;
using ObservableCollections;
using R3;
using Utilities.Lifetimes;

namespace Utilities.Reactive
{
    public enum AddRemove
    {
        Add,
        Remove
    }

    public readonly struct ViewableCollectionEvent<T>
    {
        public ViewableCollectionEvent(int index, T item)
        {
            Index = index;
            Item = item;
        }

        public int Index { get; }
        public T Item { get; }
    }

    public readonly struct ViewableMapEvent<TKey, TValue>
    {
        public ViewableMapEvent(AddRemove change, TKey key, TValue value)
        {
            Change = change;
            Key = key;
            Value = value;
        }

        public AddRemove Change { get; }
        public TKey Key { get; }
        public TValue Value { get; }
        public bool IsAdd => Change == AddRemove.Add;
        public bool IsRemove => Change == AddRemove.Remove;
    }

    public interface IViewableList<T> : IList<T>
    {
        ISource<Unit> Change { get; }
        ISource<(AddRemove Change, T Item)> AddRemoveChanges { get; }
    }

    public sealed class ViewableList<T> : IViewableList<T>, IDisposable
    {
        private readonly ObservableList<T> _items = new();
        private readonly ISource<Unit> _change;
        private readonly ISource<(AddRemove Change, T Item)> _addRemoveChanges;

        public ViewableList()
        {
            var changes = _items.ObserveChanged();
            _change = new ObservableSource<Unit>(changes.Select(_ => Unit.Default));
            _addRemoveChanges = new ObservableSource<(AddRemove Change, T Item)>(
                new ProjectObservable<CollectionChangedEvent<T>, (AddRemove Change, T Item)>(changes, EmitAddRemove));
        }

        public ISource<Unit> Change => _change;
        public ISource<(AddRemove Change, T Item)> AddRemoveChanges => _addRemoveChanges;
        public int Count => _items.Count;
        public bool IsReadOnly => false;
        public T this[int index] { get => _items[index]; set => _items[index] = value; }

        public void Add(T item) => _items.Add(item);

        public void Clear()
        {
            var removedItems = new List<T>(_items);
            foreach (var item in removedItems)
            {
                _items.Remove(item);
            }
        }

        public bool Contains(T item) => _items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        public int IndexOf(T item) => _items.IndexOf(item);
        public void Insert(int index, T item) => _items.Insert(index, item);
        public bool Remove(T item) => _items.Remove(item);
        public void RemoveAt(int index) => _items.RemoveAt(index);
        public void Dispose() { }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static void EmitAddRemove(CollectionChangedEvent<T> change, Action<(AddRemove Change, T Item)> emit)
        {
            switch (change.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    emit((AddRemove.Add, change.NewItem));
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    emit((AddRemove.Remove, change.OldItem));
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    emit((AddRemove.Remove, change.OldItem));
                    emit((AddRemove.Add, change.NewItem));
                    break;
            }
        }
    }

    public interface IViewableSet<T> : ICollection<T>
    {
        ISource<Unit> Change { get; }
        ISource<(AddRemove Change, T Item)> AddRemoveChanges { get; }
        new bool Add(T item);
    }

    public sealed class ViewableSet<T> : IViewableSet<T>, IDisposable
    {
        private readonly ObservableHashSet<T> _items = new();
        private readonly ISource<Unit> _change;
        private readonly ISource<(AddRemove Change, T Item)> _addRemoveChanges;

        public ViewableSet()
        {
            var changes = _items.ObserveChanged();
            _change = new ObservableSource<Unit>(changes.Select(_ => Unit.Default));
            _addRemoveChanges = new ObservableSource<(AddRemove Change, T Item)>(
                new ProjectObservable<CollectionChangedEvent<T>, (AddRemove Change, T Item)>(changes, EmitAddRemove));
        }

        public ISource<Unit> Change => _change;
        public ISource<(AddRemove Change, T Item)> AddRemoveChanges => _addRemoveChanges;
        public int Count => _items.Count;
        public bool IsReadOnly => false;
        public bool Add(T item) => _items.Add(item);
        void ICollection<T>.Add(T item) => Add(item);

        public void Clear()
        {
            var removedItems = new List<T>(_items);
            foreach (var item in removedItems)
            {
                _items.Remove(item);
            }
        }

        public bool Contains(T item) => _items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in _items)
            {
                array[arrayIndex++] = item;
            }
        }
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        public bool Remove(T item) => _items.Remove(item);
        public void Dispose() { }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static void EmitAddRemove(CollectionChangedEvent<T> change, Action<(AddRemove Change, T Item)> emit)
        {
            if (change.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                emit((AddRemove.Add, change.NewItem));
            }
            else if (change.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                emit((AddRemove.Remove, change.OldItem));
            }
        }
    }

    public interface IViewableMap<TKey, TValue> : IDictionary<TKey, TValue>
    {
        ISource<Unit> Change { get; }
        ISource<(AddRemove Change, TKey Key, TValue Value)> AddRemoveChanges { get; }
    }

    public sealed class ViewableMap<TKey, TValue> : IViewableMap<TKey, TValue>, IDisposable
    {
        private readonly ObservableDictionary<TKey, TValue> _items = new();
        private readonly ISource<Unit> _change;
        private readonly ISource<(AddRemove Change, TKey Key, TValue Value)> _addRemoveChanges;

        public ViewableMap()
        {
            var changes = _items.ObserveChanged();
            _change = new ObservableSource<Unit>(changes.Select(_ => Unit.Default));
            _addRemoveChanges = new ObservableSource<(AddRemove Change, TKey Key, TValue Value)>(
                new ProjectObservable<CollectionChangedEvent<KeyValuePair<TKey, TValue>>, (AddRemove Change, TKey Key, TValue Value)>(changes, EmitAddRemove));
        }

        public ISource<Unit> Change => _change;
        public ISource<(AddRemove Change, TKey Key, TValue Value)> AddRemoveChanges => _addRemoveChanges;
        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)_items).Keys;
        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)_items).Values;
        public int Count => _items.Count;
        public bool IsReadOnly => false;
        public TValue this[TKey key] { get => _items[key]; set => _items[key] = value; }
        public void Add(TKey key, TValue value) => _items.Add(key, value);
        public void Add(KeyValuePair<TKey, TValue> item) => _items.Add(item);

        public void Clear()
        {
            var keys = new List<TKey>(((IDictionary<TKey, TValue>)_items).Keys);
            foreach (var key in keys)
            {
                _items.Remove(key);
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => _items.Contains(item);
        public bool ContainsKey(TKey key) => _items.ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _items.GetEnumerator();
        public bool Remove(TKey key) => _items.Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => _items.Remove(item);
        public bool TryGetValue(TKey key, out TValue value) => _items.TryGetValue(key, out value);
        public void Dispose() { }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static void EmitAddRemove(CollectionChangedEvent<KeyValuePair<TKey, TValue>> change, Action<(AddRemove Change, TKey Key, TValue Value)> emit)
        {
            switch (change.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    emit((AddRemove.Add, change.NewItem.Key, change.NewItem.Value));
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    emit((AddRemove.Remove, change.OldItem.Key, change.OldItem.Value));
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    emit((AddRemove.Remove, change.OldItem.Key, change.OldItem.Value));
                    emit((AddRemove.Add, change.NewItem.Key, change.NewItem.Value));
                    break;
            }
        }
    }

    internal sealed class ObservableSource<T> : ISource<T>
    {
        public ObservableSource(Observable<T> observable)
        {
            Observable = observable;
        }

        public Observable<T> Observable { get; }
    }

    internal sealed class ProjectObservable<TSource, TResult> : Observable<TResult>
    {
        private readonly Observable<TSource> _source;
        private readonly Action<TSource, Action<TResult>> _project;

        public ProjectObservable(Observable<TSource> source, Action<TSource, Action<TResult>> project)
        {
            _source = source;
            _project = project;
        }

        protected override IDisposable SubscribeCore(Observer<TResult> observer)
        {
            return _source.Subscribe(value => _project(value, observer.OnNext));
        }
    }

    public static class ViewableCollectionExtensions
    {
        public static void Advise<T>(this IViewableList<T> list, Lifetimes.Lifetime lifetime, Action<Unit> handler) => list.Change.Advise(lifetime, handler);

        public static void Advise<TKey, TValue>(this IViewableMap<TKey, TValue> map, Lifetimes.Lifetime lifetime, Action<ViewableMapEvent<TKey, TValue>> handler)
        {
            lifetime.AddDisposable(map.AddRemoveChanges.Observable.Subscribe(change => handler(new ViewableMapEvent<TKey, TValue>(change.Change, change.Key, change.Value))));
        }

        public static void Advise<T>(this IViewableSet<T> set, Lifetimes.Lifetime lifetime, Action<AddRemove, T> handler)
        {
            lifetime.AddDisposable(set.AddRemoveChanges.Observable.Subscribe(change => handler(change.Change, change.Item)));
        }

        public static void Advise<T>(this IViewableSet<T> set, Lifetimes.Lifetime lifetime, Action<T> handler)
        {
            lifetime.AddDisposable(set.AddRemoveChanges.Observable.Subscribe(change => handler(change.Item)));
        }

        public static void Advise<T>(this IViewableList<T> list, Lifetimes.Lifetime lifetime, Action<AddRemove, T> handler)
        {
            lifetime.AddDisposable(list.AddRemoveChanges.Observable.Subscribe(change => handler(change.Change, change.Item)));
        }

        public static void AdviseAddRemove<TKey, TValue>(this IViewableMap<TKey, TValue> map, Lifetimes.Lifetime lifetime, Action<AddRemove, TKey, TValue> handler)
        {
            lifetime.AddDisposable(map.AddRemoveChanges.Observable.Subscribe(change => handler(change.Change, change.Key, change.Value)));
        }

        public static void AddLifetimed<T>(this IViewableList<T> list, Lifetimes.Lifetime lifetime, T item)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (lifetime == null) throw new ArgumentNullException(nameof(lifetime));
            list.Add(item);
            lifetime.OnTermination(() => list.Remove(item));
        }

        public static void View<T>(this IViewableList<T> list, Lifetimes.Lifetime lifetime, Action<Lifetimes.Lifetime, int, T> handler)
        {
            var trackedItems = new List<(T Item, Lifetimes.Lifetime Lifetime)>();

            void AddItem(T item)
            {
                var itemLifetime = lifetime.CreateNested().Lifetime;
                trackedItems.Add((item, itemLifetime));
                handler(itemLifetime, trackedItems.Count - 1, item);
            }

            void RemoveItem(T item)
            {
                for (var i = 0; i < trackedItems.Count; i++)
                {
                    if (!EqualityComparer<T>.Default.Equals(trackedItems[i].Item, item)) continue;
                    var trackedItem = trackedItems[i];
                    trackedItems.RemoveAt(i);
                    trackedItem.Lifetime.Terminate();
                    return;
                }
            }

            foreach (var item in list) AddItem(item);
            list.Advise(lifetime, (change, item) =>
            {
                if (change == AddRemove.Add) AddItem(item);
                else RemoveItem(item);
            });
            lifetime.OnTermination(() =>
            {
                foreach (var trackedItem in trackedItems) trackedItem.Lifetime.Terminate();
                trackedItems.Clear();
            });
        }

        public static void View<TKey, TValue>(this IViewableMap<TKey, TValue> map, Lifetime lifetime, Action<Lifetime, TKey, TValue> handler)
        {
            var itemLifetimes = new Dictionary<TKey, Lifetime>();

            void AddItem(TKey key, TValue value)
            {
                var itemLifetime = lifetime.CreateNested().Lifetime;
                itemLifetimes[key] = itemLifetime;
                handler(itemLifetime, key, value);
            }

            void RemoveItem(TKey key)
            {
                if (itemLifetimes.Remove(key, out var itemLifetime)) itemLifetime.Terminate();
            }

            foreach (var pair in map) AddItem(pair.Key, pair.Value);
            map.AdviseAddRemove(lifetime, (change, key, value) =>
            {
                if (change == AddRemove.Add) AddItem(key, value);
                else RemoveItem(key);
            });
            lifetime.OnTermination(() =>
            {
                foreach (var itemLifetime in itemLifetimes.Values) itemLifetime.Terminate();
                itemLifetimes.Clear();
            });
        }

        public static void View<T>(this IViewableSet<T> set, Lifetime lifetime, Action<Lifetime, T> handler)
        {
            var itemLifetimes = new Dictionary<T, Lifetime>();

            void Remove(T item)
            {
                if (!itemLifetimes.Remove(item, out var itemLifetime)) return;
                itemLifetime.Terminate();
            }

            void Add(T item)
            {
                var itemLifetime = lifetime.CreateNested().Lifetime;
                itemLifetimes[item] = itemLifetime;
                handler(itemLifetime, item);
            }

            foreach (var item in set) Add(item);
            set.Advise(lifetime, (change, item) =>
            {
                if (change == AddRemove.Remove) Remove(item);
                else Add(item);
            });
            lifetime.OnTermination(() =>
            {
                foreach (var itemLifetime in itemLifetimes.Values) itemLifetime.Terminate();
                itemLifetimes.Clear();
            });
        }
    }
}
