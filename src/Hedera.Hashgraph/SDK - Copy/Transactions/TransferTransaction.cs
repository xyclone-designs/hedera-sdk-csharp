// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
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
            readonly AccountId accountId;
            Hbar amount;
            bool isApproved;
            FungibleHookCall hookCall;
            HbarTransfer(AccountId accountId, Hbar amount, bool isApproved)
            {
                accountId = accountId;
                amount = amount;
                isApproved = isApproved;
                hookCall = null;
            }

            HbarTransfer(AccountId accountId, Hbar amount, bool isApproved, FungibleHookCall hookCall)
            {
                accountId = accountId;
                amount = amount;
                isApproved = isApproved;
                hookCall = hookCall;
            }

            virtual AccountAmount ToProtobuf()
            {
                var builder = AccountAmount.NewBuilder().SetAccountID(accountId.ToProtobuf()).SetAmount(amount.ToTinybars()).SetIsApproval(isApproved);

                // Add hook call if present
                if (hookCall != null)
                {
                    switch (hookCall.GetType())
                    {
                        case PRE_TX_ALLOWANCE_HOOK:
                            builder.SetPreTxAllowanceHook(hookCall.ToProtobuf());
                            break;
                        case PRE_POST_TX_ALLOWANCE_HOOK:
                            builder.SetPrePostTxAllowanceHook(hookCall.ToProtobuf());
                            break;
                    }
                }

                return proto;
            }

            static HbarTransfer FromProtobuf(AccountAmount transfer)
            {
                FungibleHookCall typedHook = null;
                if (transfer.HasPreTxAllowanceHook())
                {
                    typedHook = ToFungibleHook(transfer.GetPreTxAllowanceHook(), FungibleHookType.PRE_TX_ALLOWANCE_HOOK);
                }
                else if (transfer.HasPrePostTxAllowanceHook())
                {
                    typedHook = ToFungibleHook(transfer.GetPrePostTxAllowanceHook(), FungibleHookType.PRE_POST_TX_ALLOWANCE_HOOK);
                }

                return new HbarTransfer(AccountId.FromProtobuf(transfer.GetAccountID()), Hbar.FromTinybars(transfer.GetAmount()), transfer.GetIsApproval(), typedHook);
            }

            public override string ToString()
            {
                return MoreObjects.ToStringHelper(this).Add("accountId", accountId).Add("amount", amount).Add("isApproved", isApproved).Add("hookCall", hookCall).ToString();
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
        TransferTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
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
            Dictionary<AccountId, Hbar> transfers = new HashMap();
            foreach (var transfer in hbarTransfers)
            {
                transfers.Put(transfer.accountId, transfer.amount);
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
                    transfer.amount = Hbar.FromTinybars(combinedTinybars);
                    transfer.isApproved = transfer.isApproved || isApproved;
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
            Objects.RequireNonNull(tokenId, "tokenId cannot be null");
            Objects.RequireNonNull(accountId, "accountId cannot be null");
            Objects.RequireNonNull(hookCall, "hookCall cannot be null");
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
            Objects.RequireNonNull(nftId, "nftId cannot be null");
            Objects.RequireNonNull(senderAccountId, "senderAccountId cannot be null");
            Objects.RequireNonNull(receiverAccountId, "receiverAccountId cannot be null");
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
            Objects.RequireNonNull(accountId, "accountId cannot be null");
            Objects.RequireNonNull(amount, "amount cannot be null");
            Objects.RequireNonNull(hookCall, "hookCall cannot be null");
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
                    transfer.isApproved = isApproved;
                    return this;
                }
            }

            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.CryptoTransferTransactionBody}</returns>
        virtual CryptoTransferTransactionBody.Builder Build()
        {
            var transfers = SortTransfersAndBuild();
            var builder = CryptoTransferTransactionBody.NewBuilder();
            hbarTransfers.Sort(Comparator.Comparing((HbarTransfer a) => a.accountId).ThenComparing((a) => a.isApproved));
            var hbarTransfersList = TransferList.NewBuilder();
            foreach (var transfer in hbarTransfers)
            {
                hbarTransfersList.AddAccountAmounts(transfer.ToProtobuf());
            }

            builder.SetTransfers(hbarTransfersList);
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

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetCryptoTransfer(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetCryptoTransfer(Build());
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetCryptoTransfer();
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