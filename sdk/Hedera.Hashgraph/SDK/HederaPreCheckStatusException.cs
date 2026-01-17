using System;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Signals that a transaction has failed the pre-check.
	 * <p>
	 * Before a node submits a transaction to the rest of the network,
	 * it attempts some cheap assertions. This process is called the "pre-check".
	 */
	[Obsolete]
	public sealed class HederaPreCheckStatusException : PrecheckStatusException
	{
		HederaPreCheckStatusException(Status status, TransactionId? transactionId) : base(status, transactionId) { }
	}

}