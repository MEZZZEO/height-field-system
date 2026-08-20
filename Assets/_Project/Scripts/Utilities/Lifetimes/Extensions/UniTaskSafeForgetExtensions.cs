using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utilities.Lifetimes.Extensions
{
    /// <summary>
    /// Расширения для безопасного выполнения UniTask без потери исключений.
    /// Обычный .Forget() поглощает все исключения, что затрудняет отладку.
    /// </summary>
    public static class UniTaskSafeForgetExtensions
    {
        /// <summary>
        /// Выполняет UniTask без ожидания, но с выводом исключений в консоль.
        /// </summary>
        public static async void ForgetSafely(this UniTask task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмену — это нормальное поведение
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Выполняет UniTaskVoid без ожидания.
        /// Для обработки исключений используйте UniTask вместо UniTaskVoid.
        /// </summary>
        public static void ForgetSafely(this UniTaskVoid task)
        {
            // UniTaskVoid просто выполняется без ожидания
            // Исключения не могут быть обработаны для UniTaskVoid
        }

        /// <summary>
        /// Выполняет UniTask без ожидания, с выводом исключений и привязкой к Lifetime.
        /// Задача будет автоматически отменена при завершении Lifetime.
        /// </summary>
        public static async void ForgetSafely(this UniTask task, Lifetime lifetime)
        {
            if (lifetime.IsTerminated)
                return;

            var cancellationToken = lifetime.ToCancellationToken();
            try
            {
                await task.AttachExternalCancellation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмену — это нормальное поведение
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Выполняет UniTaskVoid без ожидания с привязкой к Lifetime.
        /// Для обработки исключений используйте UniTask вместо UniTaskVoid.
        /// </summary>
        public static void ForgetSafely(this UniTaskVoid task, Lifetime lifetime)
        {
            // UniTaskVoid не поддерживает AttachExternalCancellation и SuppressCancellationThrow
            // Просто игнорируем задачу
        }

        /// <summary>
        /// Выполняет UniTask{T} без ожидания, с выводом исключений.
        /// </summary>
        public static async void ForgetSafely<T>(this UniTask<T> task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмену — это нормальное поведение
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Выполняет UniTask{T} без ожидания, с выводом исключений и привязкой к Lifetime.
        /// </summary>
        public static async void ForgetSafely<T>(this UniTask<T> task, Lifetime lifetime)
        {
            if (lifetime.IsTerminated)
                return;

            var cancellationToken = lifetime.ToCancellationToken();
            try
            {
                await task.AttachExternalCancellation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмену — это нормальное поведение
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Выполняет async Action с обработкой исключений и привязкой к Lifetime.
        /// </summary>
        public static void RunSafely(this Lifetime lifetime, Func<UniTask> asyncAction)
        {
            if (lifetime.IsTerminated)
                return;

            asyncAction.Invoke().ForgetSafely(lifetime);
        }

        /// <summary>
        /// Выполняет async Action с обработкой исключений и привязкой к Lifetime.
        /// </summary>
        public static void RunSafely(this Lifetime lifetime, Func<Lifetime, UniTask> asyncAction)
        {
            if (lifetime.IsTerminated)
                return;

            asyncAction.Invoke(lifetime).ForgetSafely(lifetime);
        }
    }
}
