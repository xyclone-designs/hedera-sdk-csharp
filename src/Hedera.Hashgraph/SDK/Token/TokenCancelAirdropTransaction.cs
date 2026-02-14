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
    /// <summary>
    /// Token cancel airdrop<br/>
    /// Remove one or more pending airdrops from state on behalf of the
    /// sender(s) for each airdrop.
    /// 
    /// Each pending airdrop canceled SHALL be removed from state and
    /// SHALL NOT be available to claim.<br/>
    /// Each cancellation SHALL be represented in the transaction body and
    /// SHALL NOT be restated in the record file.<br/>
    /// All cancellations MUST succeed for this transaction to succeed.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenCancelAirdropTransaction : PendingAirdropLogic<TokenCancelAirdropTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenCancelAirdropTransaction()
        {
            DefaultMaxTransactionFee = Hbar.From(1);
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenCancelAirdropTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenCancelAirdropTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.TokenCancelAirdropTransactionBody}</returns>
        public virtual Proto.TokenCancelAirdropTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenCancelAirdropTransactionBody();

            foreach (var pendingAirdropId in PendingAirdropIds)
            {
                builder.PendingAirdrops.Add(pendingAirdropId.ToProtobuf());
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
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
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}