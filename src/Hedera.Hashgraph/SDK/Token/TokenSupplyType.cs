// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Possible Token Supply Types (IWA Compatibility).
    /// <p>
    /// Indicates how many tokens can have during its lifetime.
    /// <p>
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/tokensupplytype">Hedera Documentation</a>
    /// </summary>
    public enum TokenSupplyType
    {
		/// <summary>
		/// Indicates that tokens of that type have an upper bound of Long.MAX_VALUE.
		/// </summary>
		Infinite = Proto.TokenSupplyType.Infinite,

        /// <summary>
        /// Indicates that tokens of that type have an upper bound of maxSupply, provided on token creation.
        /// </summary>
        Finite = Proto.TokenSupplyType.Finite, 
    }
}