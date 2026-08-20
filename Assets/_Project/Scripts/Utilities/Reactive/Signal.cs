using System;
using R3;

namespace Utilities.Reactive
{
    public interface ISource<T>
    {
        Observable<T> Observable { get; }
    }

    public interface ISignal<T> : ISource<T>
    {
        void Fire(T value);
    }

    public sealed class Signal<T> : ISignal<T>, IDisposable
    {
        private readonly Subject<T> _subject = new();

        public Observable<T> Observable => _subject;

        public void Fire(T value)
        {
            _subject.OnNext(value);
        }

        public void Dispose()
        {
            _subject.Dispose();
        }
    }

    public static class SignalExtensions
    {
        public static void Fire(this ISignal<Unit> signal)
        {
            signal.Fire(Unit.Default);
        }
    }
}
