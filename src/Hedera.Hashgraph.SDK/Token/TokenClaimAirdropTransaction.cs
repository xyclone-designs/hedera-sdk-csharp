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
    /// <include file="TokenClaimAirdropTransaction.cs.xml" path='docs/member[@name="T:TokenClaimAirdropTransaction"]/*' />
    public class TokenClaimAirdropTransaction : PendingAirdropLogic<TokenClaimAirdropTransaction>
    {
        /// <include file="TokenClaimAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenClaimAirdropTransaction.#ctor"]/*' />
        public TokenClaimAirdropTransaction()
        {
            DefaultMaxTransactionFee = Hbar.From(1);
        }
		/// <include file="TokenClaimAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenClaimAirdropTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenClaimAirdropTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenClaimAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenClaimAirdropTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenClaimAirdropTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenClaimAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenClaimAirdropTransaction.ToProtobuf"]/*' />
        public virtual Proto.TokenClaimAirdropTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenClaimAirdropTransactionBody();

            foreach (var pendingAirdropId in PendingAirdropIds)
            {
                builder.PendingAirdrops.Add(pendingAirdropId.ToProtobuf());
            }

            return builder;
        }

        /// <include file="TokenClaimAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenClaimAirdropTransaction.InitFromTransactionBody"]/*' />
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenClaimAirdrop;

            foreach (var pendingAirdropId in body.PendingAirdrops)
            {
                PendingAirdropIds.Add(PendingAirdropId.FromProtobuf(pendingAirdropId));
            }
        }

        public override MethodDescriptor GetMethodDescriptor()
        {
            string methodname = nameof(Proto.TokenService.TokenServiceClient.claimAirdrop);
			
            return Proto.TokenService.Descriptor.FindMethodByName(methodname);
		}

        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenClaimAirdrop = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenClaimAirdrop = ToProtobuf();
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