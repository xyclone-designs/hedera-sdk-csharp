// SPDX-License-Identifier: Apache-2.0

using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// Base class for all transactions that may be built and submitted to Hedera.
    /// </summary>
    /// <param name="<T>">The type of the transaction. Used to enable chaining.</param>
    public abstract partial class Transaction<T>
    {
		public class SignableNodeTransactionBodyBytes(AccountId nodeID, TransactionId transactionID, byte[] body)
        {
            public byte[] Body { get; } = body;
            public AccountId NodeID { get; } = nodeID;
            public TransactionId TransactionID { get; } = transactionID;
        }
	}
}