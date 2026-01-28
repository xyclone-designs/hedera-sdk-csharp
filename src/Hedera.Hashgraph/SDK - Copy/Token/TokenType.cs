// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Possible Token Types (IWA Compatibility).
    /// <p>
    /// Apart from fungible and non-fungible, Tokens can have either a common or
    /// unique representation. This distinction might seem subtle, but it is
    /// important when considering how tokens can be traced and if they can have
    /// isolated and unique properties.
    /// <p>
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/tokentype">Hedera Documentation</a>
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Interchangeable value with one another, where any quantity of them has the same value as another equal quantity if they are in the same class.
        /// Share a single set of properties, not distinct from one another. Simply represented as a balance or quantity to a given Hedera account.
        /// </summary>
        FungibleCommon = Proto.TokenType.FungibleCommon,

		/// <summary>
		/// Unique, not interchangeable with other tokens of the same type as they typically have different values.
		/// Individually traced and can carry unique properties (e.g. serial number).
		/// </summary>
		NonFungibleUnique = Proto.TokenType.NonFungibleUnique,
    }
}