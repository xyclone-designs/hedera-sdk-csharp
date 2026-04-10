// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="T:AccountDeleteTransaction"]/*' />
    public sealed class AccountDeleteTransaction : Transaction<AccountDeleteTransaction>
    {
        /// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountDeleteTransaction.#ctor"]/*' />
        public AccountDeleteTransaction() { }
		/// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountDeleteTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal AccountDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal AccountDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountDeleteTransaction.RequireNotFrozen"]/*' />
		public AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountDeleteTransaction.RequireNotFrozen_2"]/*' />
		public AccountId? TransferAccountId { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountDeleteTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.CryptoDelete;

			if (body.DeleteAccountID is not null)
				AccountId = AccountId.FromProtobuf(body.DeleteAccountID);

			if (body.TransferAccountID is not null)
				TransferAccountId = AccountId.FromProtobuf(body.TransferAccountID);
		}

		/// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountDeleteTransaction.ToProtobuf"]/*' />
		public Proto.CryptoDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.CryptoDeleteTransactionBody();

            if (AccountId != null)
                builder.DeleteAccountID = AccountId.ToProtobuf();

            if (TransferAccountId != null)
                builder.TransferAccountID = TransferAccountId.ToProtobuf();

            return builder;
        }

		public override void ValidateChecksums(Client client)
		{
			AccountId?.ValidateChecksum(client);
			TransferAccountId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoDelete = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.cryptoDelete);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}