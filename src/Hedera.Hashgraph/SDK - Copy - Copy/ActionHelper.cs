// SPDX-License-Identifier: Apache-2.0
using Java.Util.Concurrent;
using Java.Util.Function;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
	internal class ActionHelper
    {
		internal static void Action(Task future, Action<Exception> Action)
        {
            future.WhenComplete(Action);
        }
		internal static void Action<T>(Task<T> future, Action<T, Exception> Action)
        {
            future.WhenComplete(Action);
        }

        internal static void TwoActions(Task future, Action onSuccess, Action<Exception> onFailure)
        {
            future.ContinueWith((output, error) =>
            {
                if (error != null)
                {
                    onFailure.Invoke(error);
                }
                else
                {
                    onSuccess.Invoke(output);
                }
            });
        }

        internal static void TwoActions<T>(Task<T> future, Action<T> onSuccess, Action<Exception> onFailure)
        {
            future.ContinueWith((output, error) =>
            {
                if (error != null)
                {
                    onFailure.Invoke(error);
                }
                else
                {
                    onSuccess.Invoke(output);
                }
            });
        }
    }
}