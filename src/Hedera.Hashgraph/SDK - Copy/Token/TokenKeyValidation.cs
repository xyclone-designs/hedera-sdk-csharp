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

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Types of validation strategies for token keys.
    /// </summary>
    public enum TokenKeyValidation
    {
        /// <summary>
        /// Currently the default behaviour. It will perform all token key validations.
        /// </summary>
        // /**
        //  * Currently the default behaviour. It will perform all token key validations.
        //  */
        // FULL_VALIDATION(Proto.TokenKeyValidation.FULL_VALIDATION)
        FULL_VALIDATION,
        /// <summary>
        /// Perform no validations at all for all passed token keys.
        /// </summary>
        // /**
        //  * Perform no validations at all for all passed token keys.
        //  */
        // NO_VALIDATION(Proto.TokenKeyValidation.NO_VALIDATION)
        NO_VALIDATION 

        // --------------------
        // TODO enum body members
        // final Proto.TokenKeyValidation code;
        // /**
        //  * Constructor.
        //  *
        //  * @param code the token key validation
        //  */
        // TokenKeyValidation(Proto.TokenKeyValidation code) {
        //     code = code;
        // }
        // static TokenKeyValidation valueOf(Proto.TokenKeyValidation code) {
        //     return switch(code) {
        //         case FULL_VALIDATION ->
        //             FULL_VALIDATION;
        //         case NO_VALIDATION ->
        //             NO_VALIDATION;
        //         default ->
        //             throw new IllegalStateException("(BUG) unhandled TokenKeyValidation");
        //     };
        // }
        // @Override
        // public String toString() {
        //     return switch(this) {
        //         case FULL_VALIDATION ->
        //             "FULL_VALIDATION";
        //         case NO_VALIDATION ->
        //             "NO_VALIDATION";
        //     };
        // }
        // public Proto.TokenKeyValidation toProtobuf() {
        //     return code;
        // }
        // --------------------
    }
}