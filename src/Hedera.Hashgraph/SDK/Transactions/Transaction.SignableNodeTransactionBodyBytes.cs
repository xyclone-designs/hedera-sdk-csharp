// SPDX-License-Identifier: Apache-2.0

using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="Transaction.SignableNodeTransactionBodyBytes.cs.xml" path='docs/member[@name="T:Transaction"]/*' />
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