// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
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
        TokenClaimAirdropTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
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
        virtual TokenClaimAirdropTransactionBody.Builder Build()
        {
            var builder = TokenClaimAirdropTransactionBody.NewBuilder();
            foreach (var pendingAirdropId in PendingAirdropIds)
            {
                builder.AddPendingAirdrops(pendingAirdropId.ToProtobuf());
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetTokenClaimAirdrop();
            foreach (var pendingAirdropId in body.GetPendingAirdropsList())
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
            bodyBuilder.SetTokenClaimAirdrop(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetTokenClaimAirdrop(Build());
        }
    }
}