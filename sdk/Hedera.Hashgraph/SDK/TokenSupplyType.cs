using System.ComponentModel;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Possible Token Supply Types (IWA Compatibility).
	 * <p>
	 * Indicates how many tokens can have during its lifetime.
	 * <p>
	 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/tokensupplytype">Hedera Documentation</a>
	 */
	public enum TokenSupplyType
	{
		/**
		 * Indicates that tokens of that type have an upper bound of long.MAX_VALUE.
		 */
		Infinite = Proto.TokenSupplyType.Infinite,
		/**
		 * Indicates that tokens of that type have an upper bound of maxSupply, provided on token creation.
		 */
		Finite = Proto.TokenSupplyType.Finite,
	}
}