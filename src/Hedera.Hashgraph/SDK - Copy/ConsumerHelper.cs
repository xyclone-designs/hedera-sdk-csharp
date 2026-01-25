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
    class ActionHelper
    {
        static void Action<T>(Task<T> future, Action<T, Exception> Action)
        {
            future.WhenComplete(Action);
        }

        static void TwoActions<T>(Task<T> future, Action<T> onSuccess, Action<Exception> onFailure)
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