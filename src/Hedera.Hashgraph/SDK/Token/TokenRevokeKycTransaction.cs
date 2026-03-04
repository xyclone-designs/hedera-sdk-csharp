// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenRevokeKycTransaction.cs.xml" path='docs/member[@name="T:TokenRevokeKycTransaction"]/*' />
    public class TokenRevokeKycTransaction : Transaction<TokenRevokeKycTransaction>
    {
        /// <include file="TokenRevokeKycTransaction.cs.xml" path='docs/member[@name="M:TokenRevokeKycTransaction.#ctor"]/*' />
        public TokenRevokeKycTransaction() { }
		/// <include file="TokenRevokeKycTransaction.cs.xml" path='docs/member[@name="M:TokenRevokeKycTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenRevokeKycTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenRevokeKycTransaction.cs.xml" path='docs/member[@name="M:TokenRevokeKycTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenRevokeKycTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenRevokeKycTransaction.cs.xml" path='docs/member[@name="M:TokenRevokeKycTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="TokenRevokeKycTransaction.cs.xml" path='docs/member[@name="M:TokenRevokeKycTransaction.RequireNotFrozen_2"]/*' />
		public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }

		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenRevokeKyc;

            if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Account is not null)
                AccountId = AccountId.FromProtobuf(body.Account);
        }

        /// <include file="TokenRevokeKycTransaction.cs.xml" path='docs/member[@name="M:TokenRevokeKycTransaction.ToProtobuf"]/*' />
        public virtual Proto.TokenRevokeKycTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenRevokeKycTransactionBody();

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
            bodyBuilder.TokenRevokeKyc = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenRevokeKyc = ToProtobuf();
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