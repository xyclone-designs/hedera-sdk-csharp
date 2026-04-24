// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.SDK.Transactions
{
    public partial class TransferTransaction 
    {
        private class HbarTransfer
        {
            public readonly AccountId AccountId;
            public Hbar Amount;
            public bool IsApproved;
            public FungibleHookCall? HookCall;

			public HbarTransfer(AccountId accountId, Hbar amount, bool isApproved, FungibleHookCall? hookCall = null)
            {
                AccountId = accountId;
                Amount = amount;
                IsApproved = isApproved;
                HookCall = hookCall;
            }
			public static HbarTransfer FromProtobuf(Proto.Services.AccountAmount transfer)
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

				return new HbarTransfer(AccountId.FromProtobuf(transfer.AccountId), Hbar.FromTinybars(transfer.Amount), transfer.IsApproval, typedHook);
			}

			public virtual Proto.Services.AccountAmount ToProtobuf()
            {
				Proto.Services.AccountAmount proto = new ()
                {
					IsApproval = IsApproved,
					Amount = Amount.ToTinybars(),
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
}
