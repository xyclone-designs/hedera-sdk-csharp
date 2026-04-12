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
		/// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountDeleteTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal AccountDeleteTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="AccountDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal AccountDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
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
		public Proto.Services.CryptoDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.CryptoDeleteTransactionBody();

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
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoDelete = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.CryptoService.CryptoServiceClient.cryptoDelete);

			return Proto.Services.CryptoService.Descriptor.FindMethodByName(methodname);
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
