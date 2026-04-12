// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType"]/*' />
    public enum FeeDataType
    {
		/// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_2"]/*' />
		Default = Proto.Services.SubType.Default,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_3"]/*' />
        TokenFungibleCommon = Proto.Services.SubType.TokenFungibleCommon,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_4"]/*' />
        TokenNonFungibleUnique = Proto.Services.SubType.TokenNonFungibleUnique,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_5"]/*' />
        TokenFungibleCommonWithCustomFees = Proto.Services.SubType.TokenFungibleCommonWithCustomFees,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_6"]/*' />
        TokenNonFungibleUniqueWithCustomFees = Proto.Services.SubType.TokenNonFungibleUniqueWithCustomFees,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_7"]/*' />
        ScheduleCreateContractCall = Proto.Services.SubType.ScheduleCreateContractCall,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_8"]/*' />
        TopicCreateWithCustomFees = Proto.Services.SubType.TopicCreateWithCustomFees,
        /// <include file="FeeDataType.cs.xml" path='docs/member[@name="T:FeeDataType_9"]/*' />
        SubmitMessageWithCustomFees = Proto.Services.SubType.SubmitMessageWithCustomFees,
    }
}
