// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="T:TokenUnfreezeTransaction"]/*' />
    public class TokenUnfreezeTransaction : Transaction<TokenUnfreezeTransaction>
    {
        /// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="M:TokenUnfreezeTransaction.#ctor"]/*' />
        public TokenUnfreezeTransaction() { }
		/// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="M:TokenUnfreezeTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenUnfreezeTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="M:TokenUnfreezeTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenUnfreezeTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="M:TokenUnfreezeTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="M:TokenUnfreezeTransaction.RequireNotFrozen_2"]/*' />
		public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="M:TokenUnfreezeTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenUnfreeze;

            if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Account is not null)
				AccountId = AccountId.FromProtobuf(body.Account);
		}

        /// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="M:TokenUnfreezeTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenUnfreezeAccountTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenUnfreezeAccountTransactionBody();

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
            bodyBuilder.TokenUnfreeze = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUnfreeze = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
            // TODO. Check 
			//string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.unfreezeTokenAccount);
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
