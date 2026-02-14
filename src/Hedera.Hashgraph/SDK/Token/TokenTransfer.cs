// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// A token transfer record.
    /// <p>
    /// Internal utility class.
    /// </summary>
    public class TokenTransfer
    {
        public readonly TokenId TokenId;
        public readonly AccountId AccountId;
        public uint? ExpectedDecimals;
        public long Amount;
        public bool IsApproved;
        public FungibleHookCall? HookCall;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="tokenId">the token id</param>
		/// <param name="accountId">the account id</param>
		/// <param name="amount">the amount</param>
		/// <param name="isApproved">is it approved</param>
		public TokenTransfer(TokenId tokenId, AccountId accountId, long amount, bool isApproved) : this(tokenId, accountId, amount, null, isApproved) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="tokenId">the token id</param>
		/// <param name="accountId">the account id</param>
		/// <param name="amount">the amount</param>
		/// <param name="expectedDecimals">the expected decimals</param>
		/// <param name="isApproved">is it approved</param>
		public TokenTransfer(TokenId tokenId, AccountId accountId, long amount, uint? expectedDecimals, bool isApproved)
        {
            TokenId = tokenId;
            AccountId = accountId;
            Amount = amount;
            ExpectedDecimals = expectedDecimals;
            IsApproved = isApproved;
            HookCall = null;
        }

        public TokenTransfer(TokenId tokenId, AccountId accountId, long amount, uint? expectedDecimals, bool isApproved, FungibleHookCall? hookCall)
        {
            TokenId = tokenId;
            AccountId = accountId;
            Amount = amount;
            ExpectedDecimals = expectedDecimals;
            IsApproved = isApproved;
            HookCall = hookCall;
        }

        public static IList<TokenTransfer> FromProtobuf(Proto.TokenTransferList tokenTransferList)
        {
            var token = TokenId.FromProtobuf(tokenTransferList.Token);
            var tokenTransfers = new List<TokenTransfer>();
            foreach (var transfer in tokenTransferList.Transfers)
            {
                FungibleHookCall? typedHook = null;

                if (transfer.PreTxAllowanceHook is not null)
                {
                    typedHook = TransferTransaction.ToFungibleHook(transfer.PreTxAllowanceHook, FungibleHookType.PreTxAllowanceHook);
                }
                else if (transfer.PrePostTxAllowanceHook is not null)
                {
                    typedHook = TransferTransaction.ToFungibleHook(transfer.PrePostTxAllowanceHook, FungibleHookType.PrePostTxAllowanceHook);
                }

                tokenTransfers.Add(new TokenTransfer(
                    token,
					AccountId.FromProtobuf(transfer.AccountID), 
                    transfer.Amount, 
                    tokenTransferList.ExpectedDecimals, 
                    transfer.IsApproval, 
                    typedHook));
            }

            return tokenTransfers;
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>an account amount protobuf</returns>
        public virtual Proto.AccountAmount ToProtobuf()
        {
			Proto.AccountAmount proto = new()
            {
				Amount = Amount,
				IsApproval = IsApproved,
				AccountID = AccountId.ToProtobuf(),
			};

			switch (HookCall?.Type)
			{
				case FungibleHookType.PreTxAllowanceHook:
					proto.PreTxAllowanceHook = HookCall.ToProtobuf();
					break;
				case FungibleHookType.PrePostTxAllowanceHook:
					proto.PrePostTxAllowanceHook = HookCall.ToProtobuf();
					break;
				default: break;
			}

			return proto;
        }
    }
}