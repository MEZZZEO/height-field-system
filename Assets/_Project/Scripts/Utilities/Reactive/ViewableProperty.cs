using System;
using System.Collections.Generic;
using R3;
using Utilities.Lifetimes;

namespace Utilities.Reactive
{
    public readonly struct Maybe<T>
    {
        private readonly T _value;

        public Maybe(T value)
        {
            _value = value;
        }

        public T ValueOrDefault => _value;
    }

    public interface IReadonlyProperty<T>
    {
        T Value { get; }
        Maybe<T> Maybe { get; }
    }

    public interface IViewableProperty<T> : IReadonlyProperty<T>, IDisposable
    {
        ISource<T> Change { get; }
        new T Value { get; set; }
    }

    public sealed class ViewableProperty<T> : IViewableProperty<T>
    {
        private readonly ReactiveProperty<T> _property;
        private readonly Signal<T> _change = new();

        public ViewableProperty()
            : this(default)
        {
        }

        public ViewableProperty(T initialValue)
        {
            _property = new ReactiveProperty<T>(initialValue);
        }

        public T Value
        {
            get => _property.Value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_property.Value, value))
                {
                    return;
                }

                _property.Value = value;
                _change.Fire(value);
            }
        }

        public ISource<T> Change => _change;
        public Maybe<T> Maybe => new(Value);

        public IDisposable Subscribe(Action<T> onNext)
        {
            return _property.Subscribe(onNext);
        }

        public void Dispose()
        {
            _property.Dispose();
            _change.Dispose();
        }
    }

    public static class ViewablePropertyExtensions
    {
        public static void Advise<T>(this IReadonlyProperty<T> property, Lifetime lifetime, Action<T> handler)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (lifetime == null)
            {
                throw new ArgumentNullException(nameof(lifetime));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (property is ViewableProperty<T> viewableProperty)
            {
                lifetime.AddDisposable(viewableProperty.Subscribe(handler));
                return;
            }

            handler(property.Value);
        }

        public static void Advise<T>(this ISource<T> source, Lifetime lifetime, Action<T> handler)
        {
            lifetime.AddDisposable(source.Observable.Subscribe(handler));
        }

        public static bool HasValue<T>(this IReadonlyProperty<T> property)
            where T : class
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return property.Value != null;
        }

        public static void Advise(this ISource<Unit> source, Lifetime lifetime, Action handler)
        {
            lifetime.AddDisposable(source.Observable.Subscribe(_ => handler()));
        }

        public static void View<T>(this IReadonlyProperty<T> property, Lifetime lifetime, Action<Lifetime, T> handler)
        {
            Lifetime nested = null;
            property.Advise(lifetime, value =>
            {
                nested?.Terminate();
                if (!lifetime.IsAlive)
                {
                    return;
                }

                nested = lifetime.CreateNested().Lifetime;
                handler(nested, value);
            });

            lifetime.OnTermination(() => nested?.Terminate());
        }

        public static void ViewNotNull<T>(this IReadonlyProperty<T> property, Lifetime lifetime, Action<Lifetime, T> handler)
            where T : class
        {
            property.View(lifetime, (nested, value) =>
            {
                if (value != null)
                {
                    handler(nested, value);
                }
            });
        }

        public static void WhenTrue(this IReadonlyProperty<bool> property, Lifetime lifetime, Action<Lifetime> handler)
        {
            property.View(lifetime, (nested, value) =>
            {
                if (value)
                {
                    handler(nested);
                }
            });
        }

        public static void WhenFalse(this IReadonlyProperty<bool> property, Lifetime lifetime, Action<Lifetime> handler)
        {
            property.View(lifetime, (nested, value) =>
            {
                if (!value)
                {
                    handler(nested);
                }
            });
        }

        public static void Compose<T>(this IReadonlyProperty<T> first, Lifetime lifetime, IReadonlyProperty<T> second, Action<T, T> handler)
        {
            void Invoke() => handler(first.Value, second.Value);

            first.Advise(lifetime, _ => Invoke());
            second.Advise(lifetime, _ => Invoke());
        }
    }
}
