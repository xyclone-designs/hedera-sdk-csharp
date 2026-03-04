// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenGrantKycTransaction.cs.xml" path='docs/member[@name="T:TokenGrantKycTransaction"]/*' />
    public class TokenGrantKycTransaction : Transaction<TokenGrantKycTransaction>
    {
        /// <include file="TokenGrantKycTransaction.cs.xml" path='docs/member[@name="M:TokenGrantKycTransaction.#ctor"]/*' />
        public TokenGrantKycTransaction() { }
		/// <include file="TokenGrantKycTransaction.cs.xml" path='docs/member[@name="M:TokenGrantKycTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenGrantKycTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenGrantKycTransaction.cs.xml" path='docs/member[@name="M:TokenGrantKycTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenGrantKycTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenGrantKycTransaction.cs.xml" path='docs/member[@name="M:TokenGrantKycTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenGrantKycTransaction.cs.xml" path='docs/member[@name="M:TokenGrantKycTransaction.RequireNotFrozen_2"]/*' />
		public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="TokenGrantKycTransaction.cs.xml" path='docs/member[@name="M:TokenGrantKycTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenGrantKyc;

            if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Account is not null)
                AccountId = AccountId.FromProtobuf(body.Account);
        }

        /// <include file="TokenGrantKycTransaction.cs.xml" path='docs/member[@name="M:TokenGrantKycTransaction.ToProtobuf"]/*' />
        public virtual Proto.TokenGrantKycTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenGrantKycTransactionBody();

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
            bodyBuilder.TokenGrantKyc = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenGrantKyc = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
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