using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * An Exception thrown on error status by {@link TransactionId#getReceipt(Client)}.
     * <p>
     * The receipt is included, though only the {@link TransactionReceipt#status} field will be
     * initialized; all the getters should throw.
     */
    [Obsolete]
    public class HederaReceiptStatusException : ReceiptStatusException 
    {
        public HederaReceiptStatusException(TransactionId transactionId, TransactionReceipt receipt) : base(transactionId, receipt) { }
	}
}