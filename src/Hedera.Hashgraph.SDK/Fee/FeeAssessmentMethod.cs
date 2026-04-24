// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fee
{
    /// <include file="FeeAssessmentMethod.cs.xml" path='docs/member[@name="T:FeeAssessmentMethod"]/*' />
    public enum FeeAssessmentMethod
    {
        /// <include file="FeeAssessmentMethod.cs.xml" path='docs/member[@name="M:FeeAssessmentMethod.ToBool(FeeAssessmentMethod)"]/*' />
        Inclusive = 0,
        /// <include file="FeeAssessmentMethod.cs.xml" path='docs/member[@name="M:FeeAssessmentMethod.ToBool(FeeAssessmentMethod)_2"]/*' />
        Exclusive = 1,
    }

	public static class FeeAssessmentMethodExtensions
	{
        public static bool ToBool(this FeeAssessmentMethod _feeassessmentmethod) => _feeassessmentmethod == FeeAssessmentMethod.Exclusive;
        public static FeeAssessmentMethod ToFeeAssessmentMethod(this bool _bool) => _bool ? FeeAssessmentMethod.Inclusive : FeeAssessmentMethod.Exclusive;
	}
}
