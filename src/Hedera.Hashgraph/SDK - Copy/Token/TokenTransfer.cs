// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.TransferTransaction;
using Com.Google.Common.Base;
using Hedera.Hashgraph.SDK.Proto;
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
using Hedera.Hashgraph.SDK.Transactions.Account;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// A token transfer record.
    /// <p>
    /// Internal utility class.
    /// </summary>
    public class TokenTransfer
    {
        readonly TokenId tokenId;
        readonly AccountId accountId;
        int expectedDecimals;
        long amount;
        bool isApproved;
        FungibleHookCall hookCall;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="accountId">the account id</param>
        /// <param name="amount">the amount</param>
        /// <param name="isApproved">is it approved</param>
        TokenTransfer(TokenId tokenId, AccountId accountId, long amount, bool isApproved) : this(tokenId, accountId, amount, null, isApproved)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="accountId">the account id</param>
        /// <param name="amount">the amount</param>
        /// <param name="expectedDecimals">the expected decimals</param>
        /// <param name="isApproved">is it approved</param>
        TokenTransfer(TokenId tokenId, AccountId accountId, long amount, int expectedDecimals, bool isApproved)
        {
            tokenId = tokenId;
            accountId = accountId;
            amount = amount;
            expectedDecimals = expectedDecimals;
            isApproved = isApproved;
            hookCall = null;
        }

        TokenTransfer(TokenId tokenId, AccountId accountId, long amount, int expectedDecimals, bool isApproved, FungibleHookCall hookCall)
        {
            tokenId = tokenId;
            accountId = accountId;
            amount = amount;
            expectedDecimals = expectedDecimals;
            isApproved = isApproved;
            hookCall = hookCall;
        }

        static IList<TokenTransfer> FromProtobuf(TokenTransferList tokenTransferList)
        {
            var token = TokenId.FromProtobuf(tokenTransferList.GetToken());
            var tokenTransfers = new List<TokenTransfer>();
            foreach (var transfer in tokenTransferList.GetTransfersList())
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

                var acctId = AccountId.FromProtobuf(transfer.GetAccountID());
                int expectedDecimals = tokenTransferList.HasExpectedDecimals() ? tokenTransferList.GetExpectedDecimals().GetValue() : null;
                tokenTransfers.Add(new TokenTransfer(token, acctId, transfer.GetAmount(), expectedDecimals, transfer.GetIsApproval(), typedHook));
            }

            return tokenTransfers;
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>an account amount protobuf</returns>
        virtual AccountAmount ToProtobuf()
        {
            var builder = AccountAmount.NewBuilder().SetAccountID(accountId.ToProtobuf()).SetAmount(amount).SetIsApproval(isApproved);
            if (hookCall != null)
            {
                switch (hookCall.GetType())
                {
                    case PRE_TX_ALLOWANCE_HOOK:
                        builder.SetPreTxAllowanceHook(hookCall.ToProtobuf());
                    case PRE_POST_TX_ALLOWANCE_HOOK:
                        builder.SetPrePostTxAllowanceHook(hookCall.ToProtobuf());
                    default:
                    {
                    }

                        break;
                }
            }

            return proto;
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("tokenId", tokenId).Add("accountId", accountId).Add("amount", amount).Add("expectedDecimals", expectedDecimals).Add("isApproved", isApproved).ToString();
        }
    }
}