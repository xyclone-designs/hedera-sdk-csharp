// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;

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
        // INFINITE(Proto.TokenSupplyType.INFINITE)
        INFINITE,
        /// <summary>
        /// Indicates that tokens of that type have an upper bound of maxSupply, provided on token creation.
        /// </summary>
        // /**
        //  * Indicates that tokens of that type have an upper bound of maxSupply, provided on token creation.
        //  */
        // FINITE(Proto.TokenSupplyType.FINITE)
        FINITE 

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