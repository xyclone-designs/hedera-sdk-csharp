using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * A token transfer record.
	 * <p>
	 * Internal utility class.
	 */
	public class TokenTransfer
	{
		private TokenId TokenId { get; }
		private AccountId AccountId { get; }
		private FungibleHookCall? HookCall { get; set; }
		private long Amount { get; set; }
		private bool IsApproved { get; set; }
		private int? ExpectedDecimals { get; set; }

		/**
		 * Constructor.
		 *
		 * @param tokenId    the token id
		 * @param accountId  the account id
		 * @param amount     the amount
		 * @param isApproved is it approved
		 */
		TokenTransfer(TokenId tokenId, AccountId accountId, long amount, bool isApproved) : 
		this(tokenId, accountId, amount, null, isApproved) { }
		/**
		 * Constructor.
		 *
		 * @param tokenId          the token id
		 * @param accountId        the account id
		 * @param amount           the amount
		 * @param expectedDecimals the expected decimals
		 * @param isApproved       is it approved
		 */
		TokenTransfer(TokenId tokenId, AccountId accountId, long amount, int? expectedDecimals, bool isApproved) :
		this(tokenId, accountId, amount, expectedDecimals, isApproved, null) { }
		TokenTransfer(TokenId tokenId, AccountId accountId, long amount, int? expectedDecimals, bool isApproved, FungibleHookCall? hookCall)
		{
			TokenId = tokenId;
			AccountId = accountId;
			Amount = amount;
			ExpectedDecimals = expectedDecimals;
			IsApproved = isApproved;
			HookCall = hookCall;
		}

		public static List<TokenTransfer> FromProtobuf(TokenTransferList tokenTransferList)
		{
			var token = TokenId.FromProtobuf(tokenTransferList.TokenId);
			var tokenTransfers = new ArrayList<TokenTransfer>();

			for (var transfer : tokenTransferList.getTransfersList())
			{
				FungibleHookCall typedHook = null;
				if (transfer.hasPreTxAllowanceHook())
				{
					typedHook = toFungibleHook(transfer.getPreTxAllowanceHook(), FungibleHookType.PreTxAllowanceHook);
				}
				else if (transfer.hasPrePostTxAllowanceHook())
				{
					typedHook = toFungibleHook(
							transfer.getPrePostTxAllowanceHook(), FungibleHookType.PrePostTxAllowanceHook);
				}

				var acctId = AccountId.FromProtobuf(transfer.getAccountID());
				Integer expectedDecimals = tokenTransferList.hasExpectedDecimals()
						? tokenTransferList.getExpectedDecimals().getValue()
						: null;

				tokenTransfers.Add(new TokenTransfer(
						token, acctId, transfer.getAmount(), expectedDecimals, transfer.getIsApproval(), typedHook));
			}
			return tokenTransfers;
		}

		/**
		 * Create the protobuf.
		 *
		 * @return an account amount protobuf
		 */
		public Proto.AccountAmount ToProtobuf()
		{
			Proto.AccountAmount proto = new ()
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