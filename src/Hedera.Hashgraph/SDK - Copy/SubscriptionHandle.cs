// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Subscribe to a topic ID's messages from a mirror node. You will receive
    /// all messages for the specified topic or within the defined start and end
    /// time.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/consensus/get-topic-message">Hedera Documentation</a>
    /// </summary>
    public sealed class SubscriptionHandle
    {
        public Action? OnUnsubscribe;
        /// <summary>
        /// Constructor.
        /// </summary>
        public SubscriptionHandle() { }

        /// <summary>
        /// Assign the callback method.
        /// </summary>
        /// <param name="onUnsubscribe">the callback method</param>
        public void SetOnUnsubscribe(Action onUnsubscribe)
        {
            this.OnUnsubscribe = onUnsubscribe;
        }

        /// <summary>
        /// Call the callback.
        /// </summary>
        public void Unsubscribe()
        {
            var unsubscribe = this.OnUnsubscribe;

            // Set onUnsubscribe back to null to make sure it is run just once.
            this.OnUnsubscribe = null;

			unsubscribe?.Invoke();
		}
    }
}