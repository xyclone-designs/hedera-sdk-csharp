// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
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
		/// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="M:TokenDissociateTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenDissociateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenDissociateTransaction.cs.xml" path='docs/member[@name="M:TokenDissociateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenDissociateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
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
        public virtual Proto.TokenDissociateTransactionBody ToProtobuf()
        {
			Proto.TokenDissociateTransactionBody builder = new ();

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
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenDissociate = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenDissociate = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.dissociateTokens);

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