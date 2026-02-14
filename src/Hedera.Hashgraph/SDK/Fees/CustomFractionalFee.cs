// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// Custom fractional fee utility class.
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#fractional-fee">Hedera Documentation</a>
    /// </summary>
    public class CustomFractionalFee : CustomFeeBase<CustomFractionalFee>
    {
        /// <summary>
        /// Create a custom fractional fee from a fee protobuf.
        /// </summary>
        /// <param name="fractionalFee">the fractional fee protobuf</param>
        /// <returns>the new custom fractional fee object</returns>
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

        /// <summary>
        /// The numerator.
        /// </summary>
        /// <returns>the numerator</returns>
        public virtual long Numerator { get; set; } = 0;
		/// <summary>
		/// Extract the denominator.
		/// Assign the denominator can not be zero (0).
		/// </summary>
		/// <returns>the denominator</returns>
		public virtual long Denominator { get; set; } = 1;
		/// <summary>
		/// The minimum fee amount.
		/// </summary>
		/// <returns>the minimum fee amount</returns>
		public virtual long Min { get; set; } = 0;
		/// <summary>
		/// The maximumn fee amount.
		/// </summary>
		public virtual long Max { get; set; } = 0;
        /// <summary>
        /// Assign the assessment method inclusive / exclusive.
        /// <p>
        /// If the assessment method field is set, the token's custom fee is charged
        /// to the sending account and the receiving account receives the full token
        /// transfer amount. If this field is set to false, the receiver pays for
        /// the token custom fees and gets the remaining token balance.
        /// INCLUSIVE(false)
        /// EXCLUSIVE(true)
        /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#fractional-fee">Hedera Documentation</a>
        /// </summary>
        public virtual FeeAssessmentMethod AssessmentMethod { get; set; } = FeeAssessmentMethod.Inclusive;

		/// <summary>
		/// Convert the fractional fee object to a protobuf.
		/// </summary>
		/// <returns>the protobuf object</returns>
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