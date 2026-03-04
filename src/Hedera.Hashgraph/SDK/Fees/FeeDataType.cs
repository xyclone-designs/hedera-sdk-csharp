// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType"]/*' />
    public enum FeeDataType
    {
		/// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_2"]/*' />
		Default = Proto.SubType.Default,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_3"]/*' />
        TokenFungibleCommon = Proto.SubType.TokenFungibleCommon,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_4"]/*' />
        TokenNonFungibleUnique = Proto.SubType.TokenNonFungibleUnique,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_5"]/*' />
        TokenFungibleCommonWithCustomFees = Proto.SubType.TokenFungibleCommonWithCustomFees,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_6"]/*' />
        TokenNonFungibleUniqueWithCustomFees = Proto.SubType.TokenNonFungibleUniqueWithCustomFees,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_7"]/*' />
        ScheduleCreateContractCall = Proto.SubType.ScheduleCreateContractCall,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_8"]/*' />
        TopicCreateWithCustomFees = Proto.SubType.TopicCreateWithCustomFees,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_9"]/*' />
        SubmitMessageWithCustomFees = Proto.SubType.SubmitMessageWithCustomFees,
    }
}