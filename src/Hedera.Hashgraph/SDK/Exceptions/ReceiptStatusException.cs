// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <summary>
    /// An Exception thrown on error status by {@link TransactionId#getReceipt(Client)}.
    /// <p>
    /// The receipt is included, though only the {@link TransactionReceipt#status} field will be
    /// initialized; all the getters should throw.
    /// </summary>
    public class ReceiptStatusException : Exception
    {
        /// <summary>
        /// The ID of the transaction that failed, in case that context is no longer available
        /// (e.g. the exception was bubbled up).
        /// </summary>
        public readonly TransactionId TransactionId;
        /// <summary>
        /// The receipt of the transaction that failed; the only initialized field is
        /// {@link TransactionReceipt#status}.
        /// </summary>
        public readonly TransactionReceipt Receipt;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="transactionId">the transaction id</param>
        /// <param name="receipt">the receipt</param>
        internal ReceiptStatusException(TransactionId transactionId, TransactionReceipt receipt)
        {
            TransactionId = transactionId;
            Receipt = receipt;
        }

		public override string Message
		{
			get => "receipt for transaction " + TransactionId + " raised status " + Receipt.Status;
		}
    }
}