// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="BatchTransaction.cs.xml" path='docs/member[@name="T:BatchTransaction"]/*' />
    public sealed class BatchTransaction : Transaction<BatchTransaction>
    {
        /// <include file="BatchTransaction.cs.xml" path='docs/member[@name="M:BatchTransaction.typeof(FreezeTransaction)"]/*' />
        private static readonly HashSet<Type> BLACKLISTED_TRANSACTIONS = [typeof(FreezeTransaction), typeof(BatchTransaction)];

		/// <include file="BatchTransaction.cs.xml" path='docs/member[@name="M:BatchTransaction.#ctor"]/*' />
		public BatchTransaction() { }
		/// <include file="BatchTransaction.cs.xml" path='docs/member[@name="M:BatchTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal BatchTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="BatchTransaction.cs.xml" path='docs/member[@name="M:BatchTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal BatchTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="BatchTransaction.cs.xml" path='docs/member[@name="M:BatchTransaction.InitFromTransactionBody"]/*' />
		public ListGuarded<ITransaction> InnerTransactions 
		{
			init; get => field ??= new ListGuarded<ITransaction>
			{
				OnRequireNotFrozen = RequireNotFrozen,
				OnValidate = ValidateInnerTransaction
			};
		} 

		/// <include file="BatchTransaction.cs.xml" path='docs/member[@name="M:BatchTransaction.InitFromTransactionBody_2"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.AtomicBatch;

			foreach (var atomicTransactionBytes in body.Transactions)
			{
				var transaction = new Proto.Services.Transaction
				{
					SignedTransactionBytes = atomicTransactionBytes
				};

				InnerTransactions.Add(ITransaction.FromBytes(transaction.ToByteArray()));
			}
		}
		/// <include file="BatchTransaction.cs.xml" path='docs/member[@name="M:BatchTransaction.ValidateInnerTransaction(ITransaction)"]/*' />
		private void ValidateInnerTransaction(ITransaction transaction) 
		{
			if (BLACKLISTED_TRANSACTIONS.Contains(transaction.GetType()))
				throw new ArgumentException("Transaction type " + transaction.GetType().Name + " is not allowed in a batch transaction");

			if (!transaction.IsFrozen())
				throw new InvalidOperationException("Inner transaction should be frozen");

			if (transaction.BatchKey == null)
				throw new InvalidOperationException("Batch key needs to be set");
		}

		/// <include file="BatchTransaction.cs.xml" path='docs/member[@name="M:BatchTransaction.ToProtobuf"]/*' />
		public Proto.Services.AtomicBatchTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.AtomicBatchTransactionBody();

            foreach (var transaction in InnerTransactions)
            {
                builder.Transactions.Add(transaction.MakeRequest().SignedTransactionBytes);
            }

            return builder;
        }

		public override void ValidateChecksums(Client client)
		{
			foreach (ITransaction transaction in InnerTransactions)
				transaction.ValidateChecksums(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.AtomicBatch = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule Atomic Batch");
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.UtilService.UtilServiceClient.atomicBatch);

			return Proto.Services.UtilService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }

		
    }
}
