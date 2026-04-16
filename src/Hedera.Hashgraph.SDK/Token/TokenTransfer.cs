// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenTransfer.cs.xml" path='docs/member[@name="T:TokenTransfer"]/*' />
    public class TokenTransfer
    {
        public readonly TokenId TokenId;
        public readonly AccountId AccountId;
        public uint? ExpectedDecimals;
        public long Amount;
        public bool IsApproved;
        public FungibleHookCall? HookCall;

		/// <include file="TokenTransfer.cs.xml" path='docs/member[@name="M:TokenTransfer.#ctor(TokenId,AccountId,System.Int64,System.Boolean)"]/*' />
		public TokenTransfer(TokenId tokenId, AccountId accountId, long amount, bool isApproved) : this(tokenId, accountId, amount, null, isApproved) { }

		/// <include file="TokenTransfer.cs.xml" path='docs/member[@name="M:TokenTransfer.#ctor(TokenId,AccountId,System.Int64,System.UInt32,System.Boolean)"]/*' />
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

        public static IList<TokenTransfer> FromProtobuf(Proto.Services.TokenTransferList tokenTransferList)
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
					AccountId.FromProtobuf(transfer.AccountId), 
                    transfer.Amount, 
                    tokenTransferList.ExpectedDecimals, 
                    transfer.IsApproval, 
                    typedHook));
            }

            return tokenTransfers;
        }

        /// <include file="TokenTransfer.cs.xml" path='docs/member[@name="M:TokenTransfer.ToProtobuf"]/*' />
        public virtual Proto.Services.AccountAmount ToProtobuf()
        {
			Proto.Services.AccountAmount proto = new()
            {
				Amount = Amount,
				IsApproval = IsApproved,
				AccountId = AccountId.ToProtobuf(),
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
