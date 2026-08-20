using System;
using System.Threading;
using R3;

namespace Utilities.Lifetimes
{
    public sealed class Lifetime : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isTerminated;

        public bool IsAlive => !_isTerminated;
        public bool IsNotAlive => _isTerminated;
        public bool IsTerminated => _isTerminated;

        public LifetimeDefinition CreateNested()
        {
            EnsureAlive();
            var nested = new LifetimeDefinition();
            OnTermination(nested.Terminate);
            return nested;
        }

        public void AddDisposable(IDisposable disposable)
        {
            if (disposable == null)
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            if (_isTerminated)
            {
                disposable.Dispose();
                return;
            }

            _disposables.Add(disposable);
        }

        public void OnTermination(Action action)
        {
            if (!TryOnTermination(action))
            {
                action();
            }
        }

        public bool TryOnTermination(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (_isTerminated)
            {
                return false;
            }

            _disposables.Add(Disposable.Create(action));
            return true;
        }

        public T Bracket<T>(Func<T> opening, Action<T> closing)
        {
            if (opening == null)
            {
                throw new ArgumentNullException(nameof(opening));
            }

            if (closing == null)
            {
                throw new ArgumentNullException(nameof(closing));
            }

            EnsureAlive();
            var value = opening();
            OnTermination(() => closing(value));
            return value;
        }

        public void Bracket(Action opening, Action closing)
        {
            if (opening == null)
            {
                throw new ArgumentNullException(nameof(opening));
            }

            if (closing == null)
            {
                throw new ArgumentNullException(nameof(closing));
            }

            EnsureAlive();
            opening();
            OnTermination(closing);
        }

        public Lifetime Intersect(Lifetime other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            EnsureAlive();
            other.EnsureAlive();

            var intersected = new Lifetime();
            OnTermination(intersected.Terminate);
            other.OnTermination(intersected.Terminate);
            return intersected;
        }

        public CancellationToken ToCancellationToken()
        {
            return _cancellationTokenSource.Token;
        }

        public void Terminate()
        {
            if (_isTerminated)
            {
                return;
            }

            _isTerminated = true;

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            _disposables.Dispose();
            _cancellationTokenSource.Dispose();
        }

        public void ThrowIfNotAlive()
        {
            EnsureAlive();
        }

        public void Dispose()
        {
            Terminate();
        }

        private void EnsureAlive()
        {
            if (_isTerminated)
            {
                throw new ObjectDisposedException(nameof(Lifetime));
            }
        }
    }
}
