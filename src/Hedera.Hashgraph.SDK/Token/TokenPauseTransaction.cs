// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenPauseTransaction.cs.xml" path='docs/member[@name="T:TokenPauseTransaction"]/*' />
    public class TokenPauseTransaction : Transaction<TokenPauseTransaction>
    {
		/// <include file="TokenPauseTransaction.cs.xml" path='docs/member[@name="M:TokenPauseTransaction.#ctor"]/*' />
		public TokenPauseTransaction() { }
		/// <include file="TokenPauseTransaction.cs.xml" path='docs/member[@name="M:TokenPauseTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenPauseTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenPauseTransaction.cs.xml" path='docs/member[@name="M:TokenPauseTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenPauseTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenPauseTransaction.cs.xml" path='docs/member[@name="M:TokenPauseTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }

		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenPause;

            if (body.Token is not null)
				TokenId = TokenId.FromProtobuf(body.Token);
		}

        /// <include file="TokenPauseTransaction.cs.xml" path='docs/member[@name="M:TokenPauseTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenPauseTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenPauseTransactionBody();

            if (TokenId is not null)
			    builder.Token = TokenId.ToProtobuf();

			return builder;
        }

		public override void ValidateChecksums(Client client)
		{
			TokenId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
		{
			bodyBuilder.TokenPause = ToProtobuf();
		}
		public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
		{
			scheduled.TokenPause = ToProtobuf();
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.pauseToken);

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
