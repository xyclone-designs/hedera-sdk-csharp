// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
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
		/// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="M:TokenUnfreezeTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenUnfreezeTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenUnfreezeTransaction.cs.xml" path='docs/member[@name="M:TokenUnfreezeTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenUnfreezeTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
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
        public virtual Proto.TokenUnfreezeAccountTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenUnfreezeAccountTransactionBody();

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
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUnfreeze = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUnfreeze = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
            // TODO. Check 
			//string methodname = nameof(Proto.TokenService.TokenServiceClient.unfreezeTokenAccount);
			string methodname = nameof(Proto.TokenService.TokenServiceClient.freezeTokenAccount);

			return Proto.TokenService.Descriptor.FindMethodByName(methodname);
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