using System;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * An Exception thrown on error status by {@link TransactionId#getReceipt(Client)}.
	 * <p>
	 * The receipt is included, though only the {@link TransactionReceipt#status} field will be
	 * initialized; all the getters should throw.
	 */
	public class ReceiptStatusException : Exception
	{
		/**
		 * Constructor.
		 *
		 * @param transactionId             the transaction id
		 * @param receipt                   the receipt
		 */
		public ReceiptStatusException(TransactionId? transactionId, TransactionReceipt receipt) : base()
		{
			TransactionId = transactionId;
			Receipt = receipt;
		}

		/**
		* The ID of the transaction that failed, in case that context is no longer available
		* (e.g. the exception was bubbled up).
		*/
		public TransactionId? TransactionId { get; }
		/**
		 * The receipt of the transaction that failed; the only initialized field is
		 * {@link TransactionReceipt#status}.
		 */
		public TransactionReceipt Receipt { get; }

        public override string Message => "receipt for transaction " + TransactionId + " raised status " + Receipt.Status;
	}
}