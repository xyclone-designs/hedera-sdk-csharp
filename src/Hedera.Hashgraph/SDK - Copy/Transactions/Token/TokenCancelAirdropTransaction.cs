// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Proto;
using Hedera.Hashgraph.SDK.Proto.TransactionBody;
using Io.Grpc;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;

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
        public virtual TokenCancelAirdropTransactionBody.Builder Build()
        {
            var builder = TokenCancelAirdropTransactionBody.NewBuilder();
            foreach (var pendingAirdropId in PendingAirdropIds)
            {
                builder.AddPendingAirdrops(pendingAirdropId.ToProtobuf());
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.TokenCancelAirdrop();
            foreach (var pendingAirdropId in body.GetPendingAirdropsList())
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