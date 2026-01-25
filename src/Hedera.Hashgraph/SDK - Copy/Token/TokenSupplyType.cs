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
		// /**
		//  * Indicates that tokens of that type have an upper bound of Long.MAX_VALUE.
		//  */
		Infinite = Proto.TokenSupplyType.Infinite,

        /// <summary>
        /// Indicates that tokens of that type have an upper bound of maxSupply, provided on token creation.
        /// </summary>
        // /**
        //  * Indicates that tokens of that type have an upper bound of maxSupply, provided on token creation.
        //  */
        Finite = Proto.TokenSupplyType.Finite, 

        // --------------------
        // TODO enum body members
        // final Proto.TokenSupplyType code;
        // /**
        //  * Constructor.
        //  *
        //  * @param code the token supply type
        //  */
        // TokenSupplyType(Proto.TokenSupplyType code) {
        //     code = code;
        // }
        // /**
        //  * What type are we.
        //  *
        //  * @param code the token supply type in question
        //  * @return the token supply type
        //  */
        // static TokenSupplyType valueOf(Proto.TokenSupplyType code) {
        //     return switch(code) {
        //         case INFINITE ->
        //             INFINITE;
        //         case FINITE ->
        //             FINITE;
        //         default ->
        //             throw new IllegalStateException("(BUG) unhandled TokenSupplyType");
        //     };
        // }
        // @Override
        // public String toString() {
        //     return switch(this) {
        //         case INFINITE ->
        //             "INFINITE";
        //         case FINITE ->
        //             "FINITE";
        //     };
        // }
        // public Proto.TokenSupplyType toProtobuf() {
        //     return code;
        // }
        // --------------------
    }
}