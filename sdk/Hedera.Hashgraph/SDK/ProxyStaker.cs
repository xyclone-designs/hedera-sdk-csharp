namespace Hedera.Hashgraph.SDK
{
	/**
	 * Information about a single account that is proxy staking.
	 */
	public sealed class ProxyStaker
	{
		/**
		 * Constructor.
		 *
		 * @param accountId                 the account id
		 * @param amount                    the amount
		 */
		private ProxyStaker(AccountId accountId, long amount)
		{
			AccountId = accountId;
			Amount = Hbar.FromTinybars(amount);
		}

		/**
		 * The Account ID that is proxy staking.
		 */
		public AccountId AccountId { get; }
		/**
		 * The number of hbars that are currently proxy staked.
		 */
		public Hbar Amount { get; }

		/**
		 * Create a proxy staker object from a protobuf.
		 *
		 * @param proxyStaker               the protobuf
		 * @return                          the new proxy staker object
		 */
		public static ProxyStaker FromProtobuf(Proto.ProxyStaker proxyStaker)
		{
			return new ProxyStaker(AccountId.FromProtobuf(proxyStaker.AccountID), proxyStaker.Amount);
		}

		public override string ToString()
		{
			return "ProxyStaker{" + "accountId=" + AccountId + ", amount=" + Amount + '}';
		}
	}

}