// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenFreezeTransaction.cs.xml" path='docs/member[@name="T:TokenFreezeTransaction"]/*' />
    public class TokenFreezeTransaction : Transaction<TokenFreezeTransaction>
    {
        /// <include file="TokenFreezeTransaction.cs.xml" path='docs/member[@name="M:TokenFreezeTransaction.#ctor"]/*' />
        public TokenFreezeTransaction() { }
		/// <include file="TokenFreezeTransaction.cs.xml" path='docs/member[@name="M:TokenFreezeTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenFreezeTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenFreezeTransaction.cs.xml" path='docs/member[@name="M:TokenFreezeTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenFreezeTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenFreezeTransaction.cs.xml" path='docs/member[@name="M:TokenFreezeTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenFreezeTransaction.cs.xml" path='docs/member[@name="M:TokenFreezeTransaction.RequireNotFrozen_2"]/*' />
        public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="TokenFreezeTransaction.cs.xml" path='docs/member[@name="M:TokenFreezeTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenFreeze;

            if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Account is not null)
				AccountId = AccountId.FromProtobuf(body.Account);
		}

        /// <include file="TokenFreezeTransaction.cs.xml" path='docs/member[@name="M:TokenFreezeTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenFreezeAccountTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenFreezeAccountTransactionBody();

            if (TokenId != null)
                builder.Token = TokenId.ToProtobuf();

            if (AccountId != null)
				builder.Account = AccountId.ToProtobuf();

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            TokenId?.ValidateChecksum(client);
			AccountId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenFreeze = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenFreeze = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.freezeTokenAccount);

			return Proto.Services.TokenService.Descriptor.FindMethodByName(methodname);
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
