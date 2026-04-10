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
		/// <include file="TokenCancelAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenCancelAirdropTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenCancelAirdropTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenCancelAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenCancelAirdropTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenCancelAirdropTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenCancelAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenCancelAirdropTransaction.ToProtobuf"]/*' />
        public virtual Proto.TokenCancelAirdropTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenCancelAirdropTransactionBody();

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

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
		{
			bodyBuilder.TokenCancelAirdrop = ToProtobuf();
		}
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			scheduled.TokenCancelAirdrop = ToProtobuf();
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.cancelAirdrop);

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