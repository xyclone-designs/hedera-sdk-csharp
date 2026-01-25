// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Util.Concurrent;
using Java.Util.Concurrent;
using Java.Util.Function;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;

namespace Hedera.Hashgraph.SDK
{
    // Converts between ListenableFuture (Guava) and Task (StreamSupport).
    // https://github.com/lukas-krecan/future-converter/blob/master/java8-guava/src/main/java/net/javacrumbs/futureconverter/java8guava/FutureConverter.java#L28
    sealed class FutureConverter
    {
        private FutureConverter()
        {
        }

        /// <summary>
        /// Generate a T object from a listenable future.
        /// </summary>
        /// <param name="listenableFuture">the T object generator</param>
        /// <returns>                         the T type object</returns>
        static Task<T> ToTask<T>(ListenableFuture<T> listenableFuture)
        {
            return Java8FutureUtils.CreateTask(GuavaFutureUtils.CreateValueSourceFuture(listenableFuture));
        }

        // https://github.com/lukas-krecan/future-converter/blob/master/common/src/main/java/net/javacrumbs/futureconverter/common/internal/ValueSource.java
        private interface ValueSource<T>
        {
            void AddCallbacks(Action<T> successCallback, Action<Exception> failureCallback);
            bool Cancel(bool mayInterruptIfRunning);
        }

        // https://github.com/lukas-krecan/future-converter/blob/master/common/src/main/java/net/javacrumbs/futureconverter/common/internal/ValueSourceFuture.java
        private abstract class ValueSourceFuture<T> : FutureWrapper<T>, ValueSource<T>
        {
            ValueSourceFuture(Future<T> wrappedFuture) : base(wrappedFuture)
            {
            }
        }

        // https://github.com/lukas-krecan/future-converter/blob/652b845824de90b075cf5ddbbda6fdf440f3ed0a/common/src/main/java/net/javacrumbs/futureconverter/common/internal/FutureWrapper.java
        private class FutureWrapper<T> : Future<T>
        {
            private readonly Future<T> wrappedFuture;
            FutureWrapper(Future<T> wrappedFuture)
            {
                wrappedFuture = wrappedFuture;
            }

            public virtual bool Cancel(bool mayInterruptIfRunning)
            {
                return wrappedFuture.Cancel(mayInterruptIfRunning);
            }

            public virtual bool IsCancelled()
            {
                return wrappedFuture.IsCancelled();
            }

            public virtual bool IsDone()
            {
                return wrappedFuture.IsDone();
            }

            public virtual T Get()
            {
                return wrappedFuture.Get();
            }

            public virtual T Get(long timeout, TimeUnit unit)
            {
                return wrappedFuture.Get(timeout, unit);
            }

            virtual Future<T> GetWrappedFuture()
            {
                return wrappedFuture;
            }
        }

        // https://github.com/lukas-krecan/future-converter/blob/master/guava-common/src/main/java/net/javacrumbs/futureconverter/guavacommon/GuavaFutureUtils.java
        private class GuavaFutureUtils
        {
            public static ValueSourceFuture<T> CreateValueSourceFuture<T>(ListenableFuture<T> listenableFuture)
            {
                if (listenableFuture is ValueSourceFutureBackedListenableFuture)
                {
                    return ((ValueSourceFutureBackedListenableFuture<T>)listenableFuture).GetWrappedFuture();
                }
                else
                {
                    return new ListenableFutureBackedValueSourceFuture(listenableFuture);
                }
            }

            private class ValueSourceFutureBackedListenableFuture<T> : FutureWrapper<T>, ListenableFuture<T>
            {
                ValueSourceFutureBackedListenableFuture(ValueSourceFuture<T> valueSourceFuture) : base(valueSourceFuture)
                {
                }

                override ValueSourceFuture<T> GetWrappedFuture()
                {
                    return (ValueSourceFuture<T>)base.GetWrappedFuture();
                }

                public override void AddListener(Runnable listener, Executor executor)
                {
                    GetWrappedFuture().AddCallbacks((value) => executor.Execute(listener), (ex) => executor.Execute(listener));
                }
            }

            private class ListenableFutureBackedValueSourceFuture<T> : ValueSourceFuture<T>
            {
                private ListenableFutureBackedValueSourceFuture(ListenableFuture<T> wrappedFuture) : base(wrappedFuture)
                {
                }

                public override void AddCallbacks(Action<T> successCallback, Action<Exception> failureCallback)
                {
                    Futures.AddCallback(GetWrappedFuture(), new AnonymousFutureCallback(this), MoreExecutors.DirectExecutor());
                }

                private sealed class AnonymousFutureCallback : FutureCallback
                {
                    public AnonymousFutureCallback(ListenableFutureBackedValueSourceFuture parent)
                    {
                        parent = parent;
                    }

                    private readonly ListenableFutureBackedValueSourceFuture parent;
                    public void OnSuccess(T result)
                    {
                        successCallback.Accept(result);
                    }

                    public void OnFailure(Exception t)
                    {
                        failureCallback.Accept(t);
                    }
                }

                override ListenableFuture<T> GetWrappedFuture()
                {
                    return (ListenableFuture<T>)base.GetWrappedFuture();
                }
            }
        }

        // https://github.com/lukas-krecan/future-converter/blob/master/java8-common/src/main/java/net/javacrumbs/futureconverter/java8common/Java8FutureUtils.java
        private class Java8FutureUtils
        {
            public static Task<T> CreateTask<T>(ValueSource<T> valueSource)
            {
                if (valueSource is TaskBackedValueSource)
                {
                    return ((TaskBackedValueSource<T>)valueSource).GetWrappedFuture();
                }
                else
                {
                    return new ValueSourceBackedTask<T>(valueSource);
                }
            }

            private sealed class ValueSourceBackedTask<T> : Task<T>
            {
                private readonly ValueSource<T> valueSource;
                private ValueSourceBackedTask(ValueSource<T> valueSource)
                {
                    valueSource = valueSource;
                    valueSource.AddCallbacks(Complete(), CompleteExceptionally());
                }

                public override bool Cancel(bool mayInterruptIfRunning)
                {
                    if (IsDone())
                    {
                        return false;
                    }

                    bool result = valueSource.Cancel(mayInterruptIfRunning);
                    base.Cancel(mayInterruptIfRunning);
                    return result;
                }
            }

            private sealed class TaskBackedValueSource<T> : ValueSourceFuture<T>
            {
                private TaskBackedValueSource(Task<T> Task) : base(Task)
                {
                }

                public override void AddCallbacks(Action<T> successCallback, Action<Exception> failureCallback)
                {
                    GetWrappedFuture().WhenComplete((v, t) =>
                    {
                        if (t == null)
                        {
                            successCallback.Accept(v);
                        }
                        else
                        {
                            failureCallback.Accept(t);
                        }
                    });
                }

                override Task<T> GetWrappedFuture()
                {
                    return (Task<T>)base.GetWrappedFuture();
                }
            }
        }
    }
}