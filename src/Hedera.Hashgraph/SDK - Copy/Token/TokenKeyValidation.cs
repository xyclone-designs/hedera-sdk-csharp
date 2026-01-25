// SPDX-License-Identifier: Apache-2.0

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
        FullValidation = Proto.TokenKeyValidation.FullValidation,
        /// <summary>
        /// Perform no validations at all for all passed token keys.
        /// </summary>
        // /**
        //  * Perform no validations at all for all passed token keys.
        //  */
        NoValidation = Proto.TokenKeyValidation.NoValidation

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