// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// Enum for the fee data types.
    /// </summary>
    public enum FeeDataType
    {
        /// <summary>
        /// The resource cost for the transaction type has no additional attributes
        /// </summary>
        // /**
        //  * The resource cost for the transaction type has no additional attributes
        //  */
        Default = Proto.SubType.Default,
        /// <summary>
        /// The resource cost for the transaction type includes an operation on a
        /// fungible/common token
        /// </summary>
        // /**
        //  * The resource cost for the transaction type includes an operation on a
        //  * fungible/common token
        //  */
        TokenFungibleCommon = Proto.SubType.TokenFungibleCommon,
        /// <summary>
        /// The resource cost for the transaction type includes an operation on
        /// a non-fungible/unique token
        /// </summary>
        // /**
        //  * The resource cost for the transaction type includes an operation on
        //  * a non-fungible/unique token
        //  */
        TokenNonFungibleUnique = Proto.SubType.TokenNonFungibleUnique,
        /// <summary>
        /// The resource cost for the transaction type includes an operation on a
        /// fungible/common token with a custom fee schedule
        /// </summary>
        // /**
        //  * The resource cost for the transaction type includes an operation on a
        //  * fungible/common token with a custom fee schedule
        //  */
        TokenFungibleCommonWithCustomFees = Proto.SubType.TokenFungibleCommonWithCustomFees,
        /// <summary>
        /// The resource cost for the transaction type includes an operation on a
        /// non-fungible/unique token with a custom fee schedule
        /// </summary>
        // /**
        //  * The resource cost for the transaction type includes an operation on a
        //  * non-fungible/unique token with a custom fee schedule
        //  */
        TokenNonFungibleUniqueWithCustomFees = Proto.SubType.TokenNonFungibleUniqueWithCustomFees,
        /// <summary>
        /// The resource cost for the transaction type includes a ScheduleCreate
        /// containing a ContractCall.
        /// </summary>
        // /**
        //  * The resource cost for the transaction type includes a ScheduleCreate
        //  * containing a ContractCall.
        //  */
        ScheduleCreateContractCall = Proto.SubType.ScheduleCreateContractCall,
        /// <summary>
        /// The resource cost for the transaction type includes a TopicCreate
        /// with custom fees.
        /// </summary>
        // /**
        //  * The resource cost for the transaction type includes a TopicCreate
        //  * with custom fees.
        //  */
        TopicCreateWithCustomFees = Proto.SubType.TopicCreateWithCustomFees,
        /// <summary>
        /// The resource cost for the transaction type includes a ConsensusSubmitMessage
        /// for a topic with custom fees.
        /// </summary>
        // /**
        //  * The resource cost for the transaction type includes a ConsensusSubmitMessage
        //  * for a topic with custom fees.
        //  */
        SubmitMessageWithCustomFees = Proto.SubType.SubmitMessageWithCustomFees,}
}