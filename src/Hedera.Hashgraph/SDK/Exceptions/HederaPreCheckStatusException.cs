// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
	/// <include file="HederaPreCheckStatusException.cs.xml" path='docs/member[@name="T:HederaPreCheckStatusException"]/*' />
	[Obsolete("Obsolete")]
    public sealed class HederaPreCheckStatusException : PrecheckStatusException
    {
		internal HederaPreCheckStatusException(ResponseStatus status, TransactionId transactionId) : base(status, transactionId) { }
    }
}