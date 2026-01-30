// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

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
        private IList<Transaction> innerTransactions = [];
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
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        BatchTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor for internal use when recreating a transaction from a TransactionBody.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        BatchTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
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
        public BatchTransaction SetInnerTransactions(IList<Transaction> transactions)
        {
            ArgumentNullException.ThrowIfNull(transactions);
            RequireNotFrozen();

            // Validate all transactions before setting
            transactions.ForEach(ValidateInnerTransaction());
            innerTransactions = new List(transactions);
            return this;
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
        public BatchTransaction AddInnerTransaction(Transaction<T> transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            RequireNotFrozen();
            ValidateInnerTransaction(transaction);
            innerTransactions.Add(transaction);
            return this;
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
        private void ValidateInnerTransaction<T>(Transaction<T> transaction) where T : Transaction<T>
        {
            if (BLACKLISTED_TRANSACTIONS.Contains(transaction.GetType()))
            {
                throw new ArgumentException("Transaction type " + transaction.GetType().Name + " is not allowed in a batch transaction");
            }

            if (!transaction.IsFrozen())
            {
                throw new InvalidOperationException("Inner transaction should be frozen");
            }

            if (transaction.GetBatchKey() == null)
            {
                throw new InvalidOperationException("Batch key needs to be set");
            }
        }

        /// <summary>
        /// Get the list of transactions this BatchTransaction is currently configured to execute.
        /// <p>
        /// Note: This returns the actual list of transactions. Modifications to this list will affect
        /// the batch transaction if it is not frozen.
        /// </summary>
        /// <returns>The list of inner transactions</returns>
        public IList<Transaction> GetInnerTransactions()
        {
            return innerTransactions;
        }

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
        public IList<TransactionId> GetInnerTransactionIds()
        {
            return innerTransactions.Select(_ => Transaction.Id); .Stream().Map(Transaction.GetTransactionId()).ToList();
        }

        override void ValidateChecksums(Client client)
        {
            foreach (Transaction<T> transaction in innerTransactions)
            {
                transaction.ValidateChecksums(client);
            }
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.AtomicBatch;

            foreach (var atomicTransactionBytes in body.Transactions)
            {
                var transaction = new Proto.Transaction
                {
					SignedTransactionBytes = atomicTransactionBytes
				};

                innerTransactions.Add(FromBytes(transaction.ToByteArray()));
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return UtilServiceGrpc.GetAtomicBatchMethod();
        }

        /// <summary>
        /// Create the builder.
        /// </summary>
        /// <returns>the transaction builder</returns>
        public Proto.AtomicBatchTransactionBody Build()
        {
            var builder = new Proto.AtomicBatchTransactionBody();

            foreach (var transaction in innerTransactions)
            {
                builder.Transactions.Add(transaction.MakeRequest().GetSignedTransactionBytes());
            }

            return builder;
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.AtomicBatch = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule Atomic Batch");
        }
    }
}