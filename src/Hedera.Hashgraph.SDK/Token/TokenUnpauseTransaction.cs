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
		/// <include file="TokenUnpauseTransaction.cs.xml" path='docs/member[@name="M:TokenUnpauseTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenUnpauseTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenUnpauseTransaction.cs.xml" path='docs/member[@name="M:TokenUnpauseTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenUnpauseTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
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
        public virtual Proto.Services.TokenUnpauseTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenUnpauseTransactionBody();

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

		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUnpause = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUnpause = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.unpauseToken);

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
