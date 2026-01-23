// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <summary>
    /// Signals that a transaction has failed the pre-check.
    /// <p>
    /// Before a node submits a transaction to the rest of the network,
    /// it attempts some cheap assertions. This process is called the "pre-check".
    /// </summary>
    [Obsolete("Obsolete")]
    public sealed class HederaPreCheckStatusException : PrecheckStatusException
    {
		internal HederaPreCheckStatusException(Status status, TransactionId transactionId) : base(status, transactionId) { }
    }
}