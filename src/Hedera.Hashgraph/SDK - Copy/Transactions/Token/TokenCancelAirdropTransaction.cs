// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
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
            defaultMaxTransactionFee = Hbar.From(1);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenCancelAirdropTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenCancelAirdropTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.TokenCancelAirdropTransactionBody}</returns>
        public virtual Proto.TokenCancelAirdropTransactionBody Build()
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
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.TokenCancelAirdrop;

            foreach (var pendingAirdropId in body.PendingAirdrops)
            {
                PendingAirdropIds.Add(PendingAirdropId.FromProtobuf(pendingAirdropId));
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetCancelAirdropMethod();
        }

        override void OnFreeze(Builder bodyBuilder)
        {
            bodyBuilder.TokenCancelAirdrop = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenCancelAirdrop = Build();
        }
    }
}