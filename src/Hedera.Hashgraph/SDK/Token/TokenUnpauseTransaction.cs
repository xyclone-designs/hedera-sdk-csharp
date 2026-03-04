// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenUnpauseTransaction.cs.xml" path='docs/member[@name="T:TokenUnpauseTransaction"]/*' />
    public class TokenUnpauseTransaction : Transaction<TokenUnpauseTransaction>
    {
		/// <include file="TokenUnpauseTransaction.cs.xml" path='docs/member[@name="M:TokenUnpauseTransaction.#ctor"]/*' />
		public TokenUnpauseTransaction() { }
		/// <include file="TokenUnpauseTransaction.cs.xml" path='docs/member[@name="M:TokenUnpauseTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenUnpauseTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenUnpauseTransaction.cs.xml" path='docs/member[@name="M:TokenUnpauseTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenUnpauseTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenUnpauseTransaction.cs.xml" path='docs/member[@name="M:TokenUnpauseTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="TokenUnpauseTransaction.cs.xml" path='docs/member[@name="M:TokenUnpauseTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenUnpause;

            if (body.Token is not null)
            {
                TokenId = TokenId.FromProtobuf(body.Token);
            }
        }
        /// <include file="TokenUnpauseTransaction.cs.xml" path='docs/member[@name="M:TokenUnpauseTransaction.ToProtobuf"]/*' />
        public virtual Proto.TokenUnpauseTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenUnpauseTransactionBody();

            if (TokenId != null)
            {
                builder.Token = TokenId.ToProtobuf();
            }

            return builder;
        }

		public override void ValidateChecksums(Client client)
		{
			if (TokenId != null)
			{
				TokenId.ValidateChecksum(client);
			}
		}

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUnpause = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUnpause = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.unpauseToken);

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