
namespace Hedera.Hashgraph.SDK
{
	/**
     * Enum for the fee data types.
     */
    public enum FeeDataType 
    {
        /**
         * The resource cost for the transaction type has no additional attributes
         */
        Default = Proto.SubType.Default,

        /**
         * The resource cost for the transaction type includes an operation on a
         * fungible/common token
         */
        TokenFungibleCommon = Proto.SubType.TokenFungibleCommon,

        /**
         * The resource cost for the transaction type includes an operation on
         * a non-fungible/unique token
         */
        TokenNonFungibleUnique = Proto.SubType.TokenNonFungibleUnique,

        /**
         * The resource cost for the transaction type includes an operation on a
         * fungible/common token with a custom fee schedule
         */
        TokenFungibleCommonWithCustomFees = Proto.SubType.TokenFungibleCommonWithCustomFees,

        /**
         * The resource cost for the transaction type includes an operation on a
         * non-fungible/unique token with a custom fee schedule
         */
        TokenNonFungibleUniqueWithCustomFees = Proto.SubType.TokenNonFungibleUniqueWithCustomFees,

        /**
         * The resource cost for the transaction type includes a ScheduleCreate
         * containing a ContractCall.
         */
        ScheduleCreateContractCall = Proto.SubType.ScheduleCreateContractCall,

		/**
         * The resource cost for the transaction type includes a TopicCreate
         * with custom fees.
         */
		TopicCreateWithCustomFees = Proto.SubType.TopicCreateWithCustomFees,

        /**
         * The resource cost for the transaction type includes a ConsensusSubmitMessage
         * for a topic with custom fees.
         */
        SubmitMessageWithCustomFees = Proto.SubType.SubmitMessageWithCustomFees
    }

}