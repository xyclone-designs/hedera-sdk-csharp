using System;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Subscribe to a topic ID's messages from a mirror node. You will receive
	 * all messages for the specified topic or within the defined start and end
	 * time.
	 *
	 * See <a href="https://docs.hedera.com/guides/docs/sdks/consensus/get-topic-message">Hedera Documentation</a>
	 */
	public sealed class SubscriptionHandle
	{
		/**
		 * Constructor.
		 */
		SubscriptionHandle() { }

		public Action? OnUnsubscribe { set; private get; }

		/**
		 * Call the callback.
		 */
		public void Unsubscribe()
		{
			Action? unsubscribe = OnUnsubscribe;

			// Set onUnsubscribe back to null to make sure it is run just once.
			OnUnsubscribe = null;
			unsubscribe?.Invoke();
		}
	}

}