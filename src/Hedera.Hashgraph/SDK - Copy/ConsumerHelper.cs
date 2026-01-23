// SPDX-License-Identifier: Apache-2.0
using Java.Util.Concurrent;
using Java.Util.Function;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    class ConsumerHelper
    {
        static void BiConsumer<T>(CompletableFuture<T> future, BiConsumer<T, Throwable> consumer)
        {
            future.WhenComplete(consumer);
        }

        static void TwoConsumers<T>(CompletableFuture<T> future, Consumer<T> onSuccess, Consumer<Throwable> onFailure)
        {
            future.WhenComplete((output, error) =>
            {
                if (error != null)
                {
                    onFailure.Accept(error);
                }
                else
                {
                    onSuccess.Accept(output);
                }
            });
        }
    }
}