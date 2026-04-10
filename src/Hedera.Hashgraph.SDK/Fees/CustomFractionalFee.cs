// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="CustomFractionalFee.cs.xml" path='docs/member[@name="T:CustomFractionalFee"]/*' />
    public class CustomFractionalFee : CustomFeeBase<CustomFractionalFee>
    {
        /// <include file="CustomFractionalFee.cs.xml" path='docs/member[@name="M:CustomFractionalFee.FromProtobuf(Proto.FractionalFee)"]/*' />
        public static CustomFractionalFee FromProtobuf(Proto.FractionalFee fractionalFee)
        {
			return new CustomFractionalFee
			{
				Numerator = fractionalFee.FractionalAmount.Numerator,
				Denominator = fractionalFee.FractionalAmount.Denominator,
				Min = fractionalFee.MinimumAmount,
				Max = fractionalFee.MaximumAmount,
				AssessmentMethod = fractionalFee.NetOfTransfers.ToFeeAssessmentMethod(),
			};
        }        

        /// <include file="CustomFractionalFee.cs.xml" path='docs/member[@name="P:CustomFractionalFee.Numerator"]/*' />
        public virtual long Numerator { get; set; } = 0;
		/// <include file="CustomFractionalFee.cs.xml" path='docs/member[@name="P:CustomFractionalFee.Denominator"]/*' />
		public virtual long Denominator { get; set; } = 1;
		/// <include file="CustomFractionalFee.cs.xml" path='docs/member[@name="P:CustomFractionalFee.Min"]/*' />
		public virtual long Min { get; set; } = 0;
		/// <include file="CustomFractionalFee.cs.xml" path='docs/member[@name="P:CustomFractionalFee.Max"]/*' />
		public virtual long Max { get; set; } = 0;
        /// <include file="CustomFractionalFee.cs.xml" path='docs/member[@name="P:CustomFractionalFee.AssessmentMethod"]/*' />
        public virtual FeeAssessmentMethod AssessmentMethod { get; set; } = FeeAssessmentMethod.Inclusive;

		/// <include file="CustomFractionalFee.cs.xml" path='docs/member[@name="M:CustomFractionalFee.ToFractionalFeeProtobuf"]/*' />
		public virtual Proto.FractionalFee ToFractionalFeeProtobuf()
        {
            return new Proto.FractionalFee
            {
				FractionalAmount = new Proto.Fraction
                {
					Numerator = Numerator,
					Denominator = Denominator,
				},
				MinimumAmount = Min,
				MaximumAmount = Max,
				NetOfTransfers = AssessmentMethod.ToBool(),
			};
        }

        public override Proto.CustomFee ToProtobuf()
        {
            return FinishToProtobuf(new Proto.CustomFee
            {
				FractionalFee = ToFractionalFeeProtobuf()
			});
        }
		public override CustomFractionalFee DeepCloneSubclass()
		{
			return new CustomFractionalFee
			{
				Numerator = Numerator,
				Denominator = Denominator,
				Min = Min,
				Max = Max,
				AssessmentMethod = AssessmentMethod,

			}.FinishDeepClone(this);
		}
	}
}