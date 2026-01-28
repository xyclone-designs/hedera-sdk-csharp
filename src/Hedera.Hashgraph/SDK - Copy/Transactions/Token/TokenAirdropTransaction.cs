// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
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
    /// Token Airdrop
    /// An "airdrop" is a distribution of tokens from a funding account
    /// to one or more recipient accounts, ideally with no action required
    /// by the recipient account(s).
    /// </summary>
    public class TokenAirdropTransaction : AbstractTokenTransferTransaction<TokenAirdropTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenAirdropTransaction() : base()
        {
            defaultMaxTransactionFee = new Hbar(1);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenAirdropTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenAirdropTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenAirdropTransactionBody}</returns>
        public virtual TokenAirdropTransactionBody.Builder Build()
        {
            var transfers = SortTransfersAndBuild();
            var builder = TokenAirdropTransactionBody.NewBuilder();
            foreach (var transfer in transfers)
            {
                builder.AddTokenTransfers(transfer.ToProtobuf());
            }

            return builder;
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetAirdropTokensMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenAirdrop = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenAirdrop = Build();
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.TokenAirdrop();
            foreach (var tokenTransferList in body.GetTokenTransfersList())
            {
                var token = TokenId.FromProtobuf(tokenTransferList.GetToken());
                foreach (var transfer in tokenTransferList.GetTransfersList())
                {
                    tokenTransfers.Add(new TokenTransfer(token, AccountId.FromProtobuf(transfer.GetAccountID()), transfer.GetAmount(), tokenTransferList.HasExpectedDecimals() ? tokenTransferList.GetExpectedDecimals().GetValue() : null, transfer.GetIsApproval()));
                }

                foreach (var transfer in tokenTransferList.GetNftTransfersList())
                {
                    nftTransfers.Add(new TokenNftTransfer(token, AccountId.FromProtobuf(transfer.GetSenderAccountID()), AccountId.FromProtobuf(transfer.GetReceiverAccountID()), transfer.GetSerialNumber(), transfer.GetIsApproval()));
                }
            }
        }
    }
}