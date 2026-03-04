// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="HederaPreCheckStatusException.cs.xml" path='docs/member[@name="M:Obsolete(&quot;Obsolete&quot;)"]/*' />
    [Obsolete("Obsolete")]
    public sealed class HederaPreCheckStatusException : PrecheckStatusException
    {
		internal HederaPreCheckStatusException(ResponseStatus status, TransactionId transactionId) : base(status, transactionId) { }
    }
}