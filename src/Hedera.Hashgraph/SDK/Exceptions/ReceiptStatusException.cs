// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="ReceiptStatusException.cs.xml" path='docs/member[@name="T:ReceiptStatusException"]/*' />
    public class ReceiptStatusException : Exception
    {
        /// <include file="ReceiptStatusException.cs.xml" path='docs/member[@name="M:ReceiptStatusException.#ctor(TransactionId,TransactionReceipt)"]/*' />
        public readonly TransactionId TransactionId;
        /// <include file="ReceiptStatusException.cs.xml" path='docs/member[@name="M:ReceiptStatusException.#ctor(TransactionId,TransactionReceipt)_2"]/*' />
        public readonly TransactionReceipt Receipt;
        /// <include file="ReceiptStatusException.cs.xml" path='docs/member[@name="M:ReceiptStatusException.#ctor(TransactionId,TransactionReceipt)_3"]/*' />
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