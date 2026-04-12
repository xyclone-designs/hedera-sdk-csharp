// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Airdrops;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenCancelAirdropTransaction.cs.xml" path='docs/member[@name="T:TokenCancelAirdropTransaction"]/*' />
    public class TokenCancelAirdropTransaction : PendingAirdropLogic<TokenCancelAirdropTransaction>
    {
        /// <include file="TokenCancelAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenCancelAirdropTransaction.#ctor"]/*' />
        public TokenCancelAirdropTransaction()
        {
            DefaultMaxTransactionFee = Hbar.From(1);
        }
		/// <include file="TokenCancelAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenCancelAirdropTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenCancelAirdropTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenCancelAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenCancelAirdropTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenCancelAirdropTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenCancelAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenCancelAirdropTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenCancelAirdropTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenCancelAirdropTransactionBody();

            foreach (var pendingAirdropId in PendingAirdropIds)
            {
                builder.PendingAirdrops.Add(pendingAirdropId.ToProtobuf());
            }

            return builder;
        }

        /// <include file="TokenCancelAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenCancelAirdropTransaction.InitFromTransactionBody"]/*' />
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenCancelAirdrop;

            foreach (var pendingAirdropId in body.PendingAirdrops)
            {
                PendingAirdropIds.Add(PendingAirdropId.FromProtobuf(pendingAirdropId));
            }
        }

		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
		{
			bodyBuilder.TokenCancelAirdrop = ToProtobuf();
		}
		public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
		{
			scheduled.TokenCancelAirdrop = ToProtobuf();
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.cancelAirdrop);

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
