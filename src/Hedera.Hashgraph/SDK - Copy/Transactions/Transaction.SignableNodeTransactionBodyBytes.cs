// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// Base class for all transactions that may be built and submitted to Hedera.
    /// </summary>
    /// <param name="<T>">The type of the transaction. Used to enable chaining.</param>
    public abstract partial class Transaction<T> : Executable<T, Proto.Transaction, Proto.TransactionResponse, TransactionResponse> where T : Transaction<T>
    {
		public class SignableNodeTransactionBodyBytes
		{
			public byte[] Body { get; }
			public AccountId NodeID { get; }
			public TransactionId TransactionID { get; }

			public SignableNodeTransactionBodyBytes(AccountId nodeID, TransactionId transactionID, byte[] body)
			{
				NodeID = nodeID;
				TransactionID = transactionID;
				Body = body;
			}
		}
	}
}