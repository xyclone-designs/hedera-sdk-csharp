
namespace Hedera.Hashgraph.SDK
{
	/**
	 * A transfer of Hbar that occurred within a transaction.
	 * <p>
	 * Returned with a {@link TransactionRecord}.
	 */
	public sealed class Transfer
	{
		Transfer(AccountId accountId, Hbar amount)
		{
			AccountId = accountId;
			Amount = amount;
		}

		/**
		 * The Account ID that sends or receives crypto-currency.
		 */
		public AccountId AccountId { get; }
		/**
		 * The amount that the account sends (negative) or receives (positive).
		 */
		public Hbar Amount { get; }

		/**
		 * Create the protobuf.
		 *
		 * @return                          the protobuf representation
		 */
		public Proto.AccountAmount ToProtobuf()
		{
			return new Proto.AccountAmount
			{
				AccountID = AccountId.ToProtobuf(),
				Amount = Amount.ToTinybars()
			};
		}

		/**
		 * Create a transfer from a protobuf.
		 *
		 * @param accountAmount             the protobuf
		 * @return                          the new transfer
		 */
		public static Transfer FromProtobuf(Proto.AccountAmount accountAmount)
		{
			return new Transfer(
				amount: Hbar.FromTinybars(accountAmount.Amount),
				accountId: AccountId.FromProtobuf(accountAmount.AccountID));
		}
	}
}