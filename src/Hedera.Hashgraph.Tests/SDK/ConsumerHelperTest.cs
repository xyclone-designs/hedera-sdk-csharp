// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api.Assertions;
using Org.Mockito.Mockito;
using Java.Util.Concurrent;
using Java.Util.Function;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK
{
    class ConsumerHelperTest
    {
        public virtual void BiConsumer()
        {
            CompletableFuture<string> future = CompletableFuture.SupplyAsync(() => "Hello");
            BiConsumer<string, Throwable> consumer = Mock(typeof(BiConsumer));
            ConsumerHelper.BiConsumer(future, consumer);
            future.Join();
            Verify(consumer, Times(1)).Accept(Any(), Any());
        }

        public virtual void TwoConsumersWithoutError()
        {
            var value = "Hello";
            CompletableFuture<string> future = CompletableFuture.CompletedFuture(value);
            Consumer<string> onSuccess = Mock(typeof(Consumer));
            Consumer<Throwable> onFailure = Mock(typeof(Consumer));
            ConsumerHelper.TwoConsumers(future, onSuccess, onFailure);
            future.Join();
            Verify(onSuccess, Times(1)).Accept(value);
            Verify(onFailure, Times(0)).Accept(Any());
        }

        public virtual void TwoConsumersWithError()
        {
            var exception = new Exception("Exception");
            CompletableFuture<string> future = CompletableFuture.FailedFuture(exception);
            Consumer<string> onSuccess = Mock(typeof(Consumer));
            Consumer<Throwable> onFailure = Mock(typeof(Consumer));
            ConsumerHelper.TwoConsumers(future, onSuccess, onFailure);
            Assert.Throws<Exception>(future.Join());
            Verify(onSuccess, Times(0)).Accept(Any());
            Verify(onFailure, Times(1)).Accept(exception);
        }
    }
}