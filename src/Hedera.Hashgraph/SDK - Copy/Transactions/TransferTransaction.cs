// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Proto;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions.Account;
using Io.Grpc;
using Java.Util;
using Javax.Annotation;
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
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// A transaction that transfers hbars and tokens between Hedera accounts. You can enter multiple transfers in a single
    /// transaction. The net value of hbars between the sending accounts and receiving accounts must equal zero.
    /// <p>
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/cryptocurrency/transfer-cryptocurrency">Hedera
    /// Documentation</a>
    /// </summary>
    public class TransferTransaction : AbstractTokenTransferTransaction<TransferTransaction>
    {
        private readonly List<HbarTransfer> hbarTransfers = new ();
        private class HbarTransfer
        {
            readonly AccountId AccountId;
            Hbar Amount;
            bool IsApproved;
            FungibleHookCall HookCall;
            HbarTransfer(AccountId accountId, Hbar amount, bool isApproved)
            {
                accountId = accountId;
                amount = amount;
                isApproved = isApproved;
                HookCall = null;
            }

            HbarTransfer(AccountId accountId, Hbar amount, bool isApproved, FungibleHookCall hookCall)
            {
                accountId = accountId;
                amount = amount;
                isApproved = isApproved;
                hookCall = hookCall;
            }

            public virtual Proto.AccountAmount ToProtobuf()
            {
				Proto.AccountAmount proto = new ()
                {
					IsApproval = IsApproved,
					Amount = Amount.ToTinybars(),
					AccountID = AccountId.ToProtobuf(),
				};

				switch (HookCall?.Type)
				{
					case FungibleHookType.PreTxAllowanceHook:
						proto.PreTxAllowanceHook = HookCall.ToProtobuf());
						break;
					case FungibleHookType.PrePostTxAllowanceHook:
						proto.PrePostTxAllowanceHook = HookCall.ToProtobuf());
						break;

                    default: break;
				}

				return proto;
            }

            public static HbarTransfer FromProtobuf(Proto.AccountAmount transfer)
            {
                FungibleHookCall? typedHook = null;

                if (transfer.PreTxAllowanceHook is not null)
                {
                    typedHook = ToFungibleHook(transfer.PreTxAllowanceHook, FungibleHookType.PreTxAllowanceHook);
                }
                else if (transfer.PrePostTxAllowanceHook is not null)
                {
                    typedHook = ToFungibleHook(transfer.PrePostTxAllowanceHook, FungibleHookType.PrePostTxAllowanceHook);
                }

                return new HbarTransfer(Ids.AccountId.FromProtobuf(transfer.AccountID), Hbar.FromTinybars(transfer.Amount), transfer.IsApproval, typedHook);
            }

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TransferTransaction()
        {
            defaultMaxTransactionFee = new Hbar(1);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TransferTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TransferTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the of hbar transfers.
        /// </summary>
        /// <returns>list of hbar transfers</returns>
        public virtual Dictionary<AccountId, Hbar> GetHbarTransfers()
        {
            Dictionary<AccountId, Hbar> transfers = [];
            foreach (var transfer in hbarTransfers)
            {
                transfers.Add(transfer.AccountId, transfer.Amount);
            }

            return transfers;
        }

        private TransferTransaction DoAddHbarTransfer(AccountId accountId, Hbar value, bool isApproved, FungibleHookCall hookCall)
        {
            RequireNotFrozen();
            foreach (var transfer in hbarTransfers)
            {
                if (transfer.accountId.Equals(accountId))
                {
                    long combinedTinybars = transfer.amount.ToTinybars() + value.ToTinybars();
                    transfer.Amount = Hbar.FromTinybars(combinedTinybars);
                    transfer.IsApproved = transfer.IsApproved || isApproved;
                    return this;
                }
            }

            hbarTransfers.Add(new HbarTransfer(accountId, value, isApproved, hookCall));
            return this;
        }

        /// <summary>
        /// Add a non approved hbar transfer to an EVM address.
        /// </summary>
        /// <param name="evmAddress">the EVM address</param>
        /// <param name="value">the value</param>
        /// <returns>the updated transaction</returns>
        public virtual TransferTransaction AddHbarTransfer(EvmAddress evmAddress, Hbar value)
        {
            AccountId accountId = AccountId.FromEvmAddress(evmAddress, 0, 0);
            return DoAddHbarTransfer(accountId, value, false, null);
        }

        /// <summary>
        /// Add a non approved hbar transfer.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="value">the value</param>
        /// <returns>the updated transaction</returns>
        public virtual TransferTransaction AddHbarTransfer(AccountId accountId, Hbar value)
        {
            return DoAddHbarTransfer(accountId, value, false, null);
        }

        /// <summary>
        /// Add an approved hbar transfer.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="value">the value</param>
        /// <returns>the updated transaction</returns>
        public virtual TransferTransaction AddApprovedHbarTransfer(AccountId accountId, Hbar value)
        {
            return DoAddHbarTransfer(accountId, value, true, null);
        }

        /// <summary>
        /// Add an token transfer with allowance hook.
        /// </summary>
        /// <param name="tokenId">the tokenId</param>
        /// <param name="accountId">the accountId</param>
        /// <param name="value">the amount</param>
        /// <param name="hookCall">the hook</param>
        /// <returns>the updated transaction</returns>
        public virtual TransferTransaction AddTokenTransferWithHook(TokenId tokenId, AccountId accountId, long value, FungibleHookCall hookCall)
        {
            return DoAddTokenTransfer(tokenId, accountId, value, false, null, hookCall);
        }

        /// <summary>
        /// Add an NFT transfer with optional sender/receiver allowance hooks.
        /// </summary>
        /// <param name="nftId">the NFT id</param>
        /// <param name="senderAccountId">the sender</param>
        /// <param name="receiverAccountId">the receiver</param>
        /// <param name="senderHookCall">optional sender hook call</param>
        /// <param name="receiverHookCall">optional receiver hook call</param>
        /// <returns>the updated transaction</returns>
        public virtual TransferTransaction AddNftTransferWithHook(NftId nftId, AccountId senderAccountId, AccountId receiverAccountId, NftHookCall senderHookCall, NftHookCall receiverHookCall)
        {
            return DoAddNftTransfer(nftId, senderAccountId, receiverAccountId, false, senderHookCall, receiverHookCall);
        }

        /// <summary>
        /// Add an HBAR transfer with a fungible hook.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="hookCall">the fungible hook call to execute</param>
        /// <returns>the updated transaction</returns>
        /// <exception cref="IllegalArgumentException">if hookCall is null</exception>
        public virtual TransferTransaction AddHbarTransferWithHook(AccountId accountId, Hbar amount, FungibleHookCall hookCall)
        {
            RequireNotFrozen();
            return DoAddHbarTransfer(accountId, amount, false, hookCall);
        }

        /// <summary>
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="isApproved">whether the transfer is approved</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecated- Use {@link #addApprovedHbarTransfer(AccountId, Hbar)} instead</remarks>
        public virtual TransferTransaction SetHbarTransferApproval(AccountId accountId, bool isApproved)
        {
            RequireNotFrozen();
            foreach (var transfer in hbarTransfers)
            {
                if (transfer.accountId.Equals(accountId))
                {
                    transfer.IsApproved = isApproved;
                    return this;
                }
            }

            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.CryptoTransferTransactionBody}</returns>
        public virtual Proto.CryptoTransferTransactionBody Build()
        {
            var transfers = SortTransfersAndBuild();
            var builder = CryptoTransferTransactionBody.NewBuilder();
            hbarTransfers.Sort(Comparator.Comparing((HbarTransfer a) => a.AccountId).ThenComparing((a) => a.isApproved));
            var hbarTransfersList = TransferList.NewBuilder();
            foreach (var transfer in hbarTransfers)
            {
                hbarTransfersList.AddAccountAmounts(transfer.ToProtobuf());
            }

            builder.Transfers(hbarTransfersList);
            foreach (var transfer in transfers)
            {
                builder.AddTokenTransfers(transfer.ToProtobuf());
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            base.ValidateChecksums(client);
            foreach (var transfer in hbarTransfers)
            {
                transfer.accountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetCryptoTransferMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SetCryptoTransfer(Build());
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.SetCryptoTransfer(Build());
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.CryptoTransfer();
            foreach (var transfer in body.GetTransfers().GetAccountAmountsList())
            {
                hbarTransfers.Add(HbarTransfer.FromProtobuf(transfer));
            }

            foreach (var tokenTransferList in body.GetTokenTransfersList())
            {
                var fungibleTokenTransfers = TokenTransfer.FromProtobuf(tokenTransferList);
                tokenTransfers.AddAll(fungibleTokenTransfers);
                var nftTokenTransfers = TokenNftTransfer.FromProtobuf(tokenTransferList);
                nftTransfers.AddAll(nftTokenTransfers);
            }
        }

        static FungibleHookCall ToFungibleHook(Proto.HookCall proto, FungibleHookType type)
        {
            var base = HookCall.FromProtobuf(proto);
            return new FungibleHookCall(@base.GetHookId(), @base.GetEvmHookCall(), type);
        }

        static NftHookCall ToNftHook(Proto.HookCall proto, NftHookType type)
        {
            var base = HookCall.FromProtobuf(proto);
            return new NftHookCall(@base.GetHookId(), @base.GetEvmHookCall(), type);
        }
    }
}