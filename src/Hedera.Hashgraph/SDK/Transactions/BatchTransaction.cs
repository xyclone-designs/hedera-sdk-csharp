// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// Execute multiple transactions in a single consensus event. This allows for atomic execution of multiple
    /// transactions, where they either all succeed or all fail together.
    /// <p>
    /// Requirements:
    /// <ul>
    ///     <li>All inner transactions must be frozen before being added to the batch</li>
    ///     <li>All inner transactions must have a batch key set</li>
    ///     <li>All inner transactions must be signed as required for each individual transaction</li>
    ///     <li>The BatchTransaction must be signed by all batch keys of the inner transactions</li>
    ///     <li>Certain transaction types (FreezeTransaction, BatchTransaction) are not allowed in a batch</li>
    /// </ul>
    /// <p>
    /// Important notes:
    /// <ul>
    ///     <li>Fees are assessed for each inner transaction separately</li>
    ///     <li>The maximum number of inner transactions in a batch is limited to 25</li>
    ///     <li>Inner transactions cannot be scheduled transactions</li>
    /// </ul>
    /// <p>
    /// Example usage:
    /// <pre>
    /// var batchKey = PrivateKey.generateED25519();
    /// 
    /// // Create and prepare inner transaction
    /// var transaction = new TransferTransaction()
    ///     .addHbarTransfer(sender, amount.negated())
    ///     .addHbarTransfer(receiver, amount)
    ///     .batchify(client, batchKey);
    /// 
    /// // Create and execute batch transaction
    /// var response = new BatchTransaction()
    ///     .addInnerTransaction(transaction)
    ///     .freezeWith(client)
    ///     .sign(batchKey)
    ///     .execute(client);
    /// </pre>
    /// </summary>
    public sealed class BatchTransaction : Transaction<BatchTransaction>
    {
		/// <summary>
        /// List of transaction types that are not allowed in a batch transaction.
        /// These transactions are prohibited due to their special nature or network-level implications.
        /// </summary>
        private static readonly HashSet<Type> BLACKLISTED_TRANSACTIONS = [typeof(FreezeTransaction), typeof(BatchTransaction)];

        /// <summary>
        /// Creates a new empty BatchTransaction.
        /// </summary>
        public BatchTransaction()
        {
        }
		/// <summary>
		/// Constructor for internal use when recreating a transaction from a TransactionBody.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public BatchTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor for internal use when recreating a transaction from a TransactionBody.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public BatchTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// Get the list of transactions this BatchTransaction is currently configured to execute.
		/// <p>
		/// Note: This returns the actual list of transactions. Modifications to this list will affect
		/// the batch transaction if it is not frozen.
		/// </summary>
		/// <returns>The list of inner transactions</returns>
		public IList<Transaction<T>> InnerTransactions { get; private set; } = [];
		/// <summary>
		/// Get the list of transaction IDs of each inner transaction of this BatchTransaction.
		/// <p>
		/// This method is particularly useful after execution to:
		/// <ul>
		///     <li>Track individual transaction results</li>
		///     <li>Query receipts for specific inner transactions</li>
		///     <li>Monitor the status of each transaction in the batch</li>
		/// </ul>
		/// <p>
		/// <b>NOTE:</b> Transaction IDs will only be meaningful after the batch transaction has been
		/// executed or the IDs have been explicitly set on the inner transactions.
		/// </summary>
		/// <returns>The list of inner transaction IDs</returns>
		public IEnumerable<TransactionId> InnerTransactionIds
		{
			get => InnerTransactions.Select(_ => _.TransactionId);
		}

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.AtomicBatch;

			foreach (var atomicTransactionBytes in body.Transactions)
			{
				var transaction = new Proto.Transaction
				{
					SignedTransactionBytes = atomicTransactionBytes
				};

				InnerTransactions.Add(Transaction<T>.FromBytes(transaction.ToByteArray()));
			}
		}
		/// <summary>
		/// Validates if a transaction is allowed in a batch transaction.
		/// <p>
		/// A transaction is valid if:
		/// <ul>
		///     <li>It is not a blacklisted type (FreezeTransaction or BatchTransaction)</li>
		///     <li>It is frozen</li>
		///     <li>It has a batch key set</li>
		/// </ul>
		/// </summary>
		/// <param name="transaction">The transaction to validate</param>
		/// <exception cref="IllegalArgumentException">if the transaction is blacklisted</exception>
		/// <exception cref="IllegalStateException">if the transaction is not frozen or missing a batch key</exception>
		private void ValidateInnerTransaction(Transaction<T> transaction)
		{
			if (BLACKLISTED_TRANSACTIONS.Contains(transaction.GetType()))
			{
				throw new ArgumentException("Transaction type " + transaction.GetType().Name + " is not allowed in a batch transaction");
			}

			if (!transaction.IsFrozen())
			{
				throw new InvalidOperationException("Inner transaction should be frozen");
			}

			if (transaction.BatchKey == null)
			{
				throw new InvalidOperationException("Batch key needs to be set");
			}
		}

		/// <summary>
		/// Append a transaction to the list of transactions this BatchTransaction will execute.
		/// <p>
		/// Requirements for the inner transaction:
		/// <ul>
		///     <li>Must be frozen (use {@link Transaction#freeze()} or {@link Transaction#freezeWith(Client)})</li>
		///     <li>Must have a batch key set (use {@link Transaction#setBatchKey(Key)}} or {@link Transaction#batchify(Client, Key)})</li>
		///     <li>Must not be a blacklisted transaction type</li>
		/// </ul>
		/// </summary>
		/// <param name="transaction">The transaction to be added</param>
		/// <returns>{@code this}</returns>
		/// <exception cref="NullPointerException">if transaction is null</exception>
		/// <exception cref="IllegalStateException">if this transaction is frozen</exception>
		/// <exception cref="IllegalStateException">if the inner transaction is not frozen or missing a batch key</exception>
		/// <exception cref="IllegalArgumentException">if the transaction is of a blacklisted type</exception>
		public BatchTransaction AddInnerTransaction<T>(Transaction<T> transaction)
		{
			ArgumentNullException.ThrowIfNull(transaction);
			RequireNotFrozen();
			ValidateInnerTransaction(transaction);
			InnerTransactions.Add(transaction);
			return this;
		}
		/// <summary>
		/// Set the list of transactions to be executed as part of this BatchTransaction.
		/// <p>
		/// Requirements for each inner transaction:
		/// <ul>
		///     <li>Must be frozen (use {@link Transaction#freeze()} or {@link Transaction#freezeWith(Client)})</li>
		///     <li>Must have a batch key set (use {@link Transaction#setBatchKey(Key)}} or {@link Transaction#batchify(Client, Key)})</li>
		///     <li>Must not be a blacklisted transaction type</li>
		/// </ul>
		/// <p>
		/// Note: This method creates a defensive copy of the provided list.
		/// </summary>
		/// <param name="transactions">The list of transactions to be executed</param>
		/// <returns>{@code this}</returns>
		/// <exception cref="NullPointerException">if transactions is null</exception>
		/// <exception cref="IllegalStateException">if this transaction is frozen</exception>
		/// <exception cref="IllegalStateException">if any inner transaction is not frozen or missing a batch key</exception>
		/// <exception cref="IllegalArgumentException">if any transaction is of a blacklisted type</exception>
		public BatchTransaction SetInnerTransactions(IList<Transaction<T>> transactions)
        {
            ArgumentNullException.ThrowIfNull(transactions);
            RequireNotFrozen();

			// Validate all transactions before setting
			foreach (Transaction<T> transaction in transactions) 
                ValidateInnerTransaction(transaction);

			InnerTransactions = [.. transactions];

            return this;
        }

        /// <summary>
        /// Create the builder.
        /// </summary>
        /// <returns>the transaction builder</returns>
        public Proto.AtomicBatchTransactionBody ToProtobuf()
        {
            var builder = new Proto.AtomicBatchTransactionBody();

            foreach (var transaction in InnerTransactions)
            {
                builder.Transactions.Add(transaction.MakeRequest().SignedTransactionBytes);
            }

            return builder;
        }

		public override void ValidateChecksums(Client client)
		{
			foreach (Transaction<T> transaction in InnerTransactions)
				transaction.ValidateChecksums(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.AtomicBatch = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule Atomic Batch");
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.UtilService.UtilServiceClient.atomicBatch);

			return Proto.UtilService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }

        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}