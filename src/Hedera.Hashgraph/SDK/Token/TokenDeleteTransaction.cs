// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenDeleteTransaction.cs.xml" path='docs/member[@name="T:TokenDeleteTransaction"]/*' />
    public class TokenDeleteTransaction : Transaction<TokenDeleteTransaction>
    {
        /// <include file="TokenDeleteTransaction.cs.xml" path='docs/member[@name="M:TokenDeleteTransaction.#ctor"]/*' />
        public TokenDeleteTransaction() { }
		/// <include file="TokenDeleteTransaction.cs.xml" path='docs/member[@name="M:TokenDeleteTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenDeleteTransaction.cs.xml" path='docs/member[@name="M:TokenDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenDeleteTransaction.cs.xml" path='docs/member[@name="M:TokenDeleteTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="TokenDeleteTransaction.cs.xml" path='docs/member[@name="M:TokenDeleteTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenDeletion;

            if (body.Token is not null)
				TokenId = TokenId.FromProtobuf(body.Token);
		}

        /// <include file="TokenDeleteTransaction.cs.xml" path='docs/member[@name="M:TokenDeleteTransaction.ToProtobuf"]/*' />
        public virtual Proto.TokenDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenDeleteTransactionBody();

            if (TokenId != null)
				builder.Token = TokenId.ToProtobuf();

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenDeletion = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenDeletion = ToProtobuf();
        }

        public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.deleteToken);

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