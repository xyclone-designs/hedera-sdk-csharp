// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="SubscriptionHandle.cs.xml" path='docs/member[@name="T:SubscriptionHandle"]/*' />
    public sealed class SubscriptionHandle
    {
        public Action? OnUnsubscribe;
        /// <include file="SubscriptionHandle.cs.xml" path='docs/member[@name="M:SubscriptionHandle.#ctor"]/*' />
        public SubscriptionHandle() { }

        /// <include file="SubscriptionHandle.cs.xml" path='docs/member[@name="M:SubscriptionHandle.SetOnUnsubscribe(System.Action)"]/*' />
        public void SetOnUnsubscribe(Action onUnsubscribe)
        {
            OnUnsubscribe = onUnsubscribe;
        }

        /// <include file="SubscriptionHandle.cs.xml" path='docs/member[@name="M:SubscriptionHandle.Unsubscribe"]/*' />
        public void Unsubscribe()
        {
            var unsubscribe = OnUnsubscribe;

            // Set onUnsubscribe back to null to make sure it is run just once.
            OnUnsubscribe = null;

			unsubscribe?.Invoke();
		}
    }
}