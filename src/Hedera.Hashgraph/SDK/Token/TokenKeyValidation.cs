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
        FullValidation = Proto.TokenKeyValidation.FullValidation,
        /// <summary>
        /// Perform no validations at all for all passed token keys.
        /// </summary>
        NoValidation = Proto.TokenKeyValidation.NoValidation
    }
}