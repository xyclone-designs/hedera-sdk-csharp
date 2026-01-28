// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
{
    /// <summary>
    /// Token claim airdrop<br/>
    /// Complete one or more pending transfers on behalf of the
    /// recipient(s) for an airdrop.
    /// 
    /// The sender MUST have sufficient balance to fulfill the airdrop at the
    /// time of claim. If the sender does not have sufficient balance, the
    /// claim SHALL fail.<br/>
    /// Each pending airdrop successfully claimed SHALL be removed from state and
    /// SHALL NOT be available to claim again.<br/>
    /// Each claim SHALL be represented in the transaction body and
    /// SHALL NOT be restated in the record file.<br/>
    /// All claims MUST succeed for this transaction to succeed.
    /// 
    /// ### Block Stream Effects
    /// The completed transfers SHALL be present in the transfer list.
    /// </summary>
    public class TokenClaimAirdropTransaction : PendingAirdropLogic<TokenClaimAirdropTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenClaimAirdropTransaction()
        {
            defaultMaxTransactionFee = Hbar.From(1);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenClaimAirdropTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenClaimAirdropTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.TokenClaimAirdropTransactionBody}</returns>
        public virtual Proto.TokenClaimAirdropTransactionBody Build()
        {
            var builder = new Proto.TokenClaimAirdropTransactionBody();

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
            var body = sourceTransactionBody.TokenClaimAirdrop;

            foreach (var pendingAirdropId in body.PendingAirdrops)
            {
                PendingAirdropIds.Add(PendingAirdropId.FromProtobuf(pendingAirdropId));
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetClaimAirdropMethod();
        }

        override void OnFreeze(Builder bodyBuilder)
        {
            bodyBuilder.TokenClaimAirdrop = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenClaimAirdrop = Build();
        }
    }
}