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
    /// <include file="TokenAssociateTransaction.cs.xml" path='docs/member[@name="T:TokenAssociateTransaction"]/*' />
    public class TokenAssociateTransaction : Transaction<TokenAssociateTransaction>
    {
        /// <include file="TokenAssociateTransaction.cs.xml" path='docs/member[@name="M:TokenAssociateTransaction.#ctor"]/*' />
        public TokenAssociateTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
		/// <include file="TokenAssociateTransaction.cs.xml" path='docs/member[@name="M:TokenAssociateTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenAssociateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenAssociateTransaction.cs.xml" path='docs/member[@name="M:TokenAssociateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenAssociateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenAssociateTransaction.cs.xml" path='docs/member[@name="M:TokenAssociateTransaction.RequireNotFrozen"]/*' />
        public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenAssociateTransaction.cs.xml" path='docs/member[@name="M:TokenAssociateTransaction.RequireNotFrozen_2"]/*' />
        public virtual List<TokenId> TokenIds { get; set { RequireNotFrozen(); field = [.. value]; } } = [];

		/// <include file="TokenAssociateTransaction.cs.xml" path='docs/member[@name="M:TokenAssociateTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.TokenAssociate;

			if (body.Account is not null)
			{
				AccountId = AccountId.FromProtobuf(body.Account);
			}

			foreach (var token in body.Tokens)
			{
				TokenIds.Add(TokenId.FromProtobuf(token));
			}
		}
		/// <include file="TokenAssociateTransaction.cs.xml" path='docs/member[@name="M:TokenAssociateTransaction.ToProtobuf"]/*' />
		public virtual Proto.TokenAssociateTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenAssociateTransactionBody();

            if (AccountId != null)
				builder.Account = AccountId.ToProtobuf();

			foreach (var token in TokenIds)
				if (token != null)
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
            bodyBuilder.TokenAssociate = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenAssociate = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.associateTokens);

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