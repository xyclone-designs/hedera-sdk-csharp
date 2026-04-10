// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
	/// <include file="HederaReceiptStatusException.cs.xml" path='docs/member[@name="T:HederaReceiptStatusException"]/*' />
	[Obsolete("Obsolete")]
    public class HederaReceiptStatusException : ReceiptStatusException
    {
		internal HederaReceiptStatusException(TransactionId transactionId, TransactionReceipt receipt) : base(transactionId, receipt) { }
    }
}