// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="T:TokenDissociateTransaction"]/*' />
    public class TokenDissociateTransaction : Transaction<TokenDissociateTransaction>
    {
        /// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="M:TokenDissociateTransaction.#ctor"]/*' />
        public TokenDissociateTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
		/// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="M:TokenDissociateTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenDissociateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="M:TokenDissociateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenDissociateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="M:TokenDissociateTransaction.RequireNotFrozen"]/*' />
		public virtual AccountId? AccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="M:TokenDissociateTransaction.RequireNotFrozen_2"]/*' />
		public IList<TokenId> TokenIds 
        { 
            get => [..field]; 
            set 
            {
				RequireNotFrozen();
				field = [.. value];
            } 
        } = [];
		
        /// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="M:TokenDissociateTransaction.InitFromTransactionBody"]/*' />
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenDissociate;

            if (body.Account is not null)
            {
				AccountId = AccountId.FromProtobuf(body.Account);
            }

            foreach (var token in body.Tokens)
                TokenIds.Add(TokenId.FromProtobuf(token));
        }
        /// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="M:TokenDissociateTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenDissociateTransactionBody ToProtobuf()
        {
			Proto.Services.TokenDissociateTransactionBody builder = new ();

            if (AccountId != null)
				builder.Account = AccountId.ToProtobuf();

			foreach (var token in TokenIds)
				builder.Tokens.Add(token.ToProtobuf());

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);

			foreach (var token in TokenIds)
				token?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenDissociate = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenDissociate = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.dissociateTokens);

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
