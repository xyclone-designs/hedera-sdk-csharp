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
    [Obsolete("Obsolete")]
    public class HederaReceiptStatusException : ReceiptStatusException
    {
		internal HederaReceiptStatusException(TransactionId transactionId, TransactionReceipt receipt) : base(transactionId, receipt) { }
    }
}