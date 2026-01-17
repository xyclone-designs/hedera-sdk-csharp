namespace Hedera.Hashgraph.SDK
{
    /**
     * Custom fractional fee utility class.
     * See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#fractional-fee">Hedera Documentation</a>
     */
    public class CustomFractionalFee : CustomFeeBase<CustomFractionalFee>
    {
        /**
         * Assign the assessment method inclusive / exclusive.
         * <p>
         * If the assessment method field is set, the token's custom fee is charged
         * to the sending account and the receiving account receives the full token
         * transfer amount. If this field is set to false, the receiver pays for
         * the token custom fees and gets the remaining token balance.
         * INCLUSIVE(false)
         * EXCLUSIVE(true)
         * See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#fractional-fee">Hedera Documentation</a>
         *
         * @param assessmentMethod inclusive / exclusive
         * @return {@code this}
         */
        public FeeAssessmentMethod AssessmentMethod { get; set; } = FeeAssessmentMethod.Inclusive;
		public long Numerator { get; set; } = 0;
		public long Denominator { get; set; } = 1;
		public long Min { get; set; } = 0;
		public long Max { get; set; } = 0;

		/**
         * Constructor.
         */
		public CustomFractionalFee() {}

        /**
         * Create a custom fractional fee from a fee protobuf.
         *
         * @param fractionalFee the fractional fee protobuf
         * @return the new custom fractional fee object
         */
        public static CustomFractionalFee FromProtobuf(Proto.FractionalFee fractionalFee) 
        {
			return new CustomFractionalFee
			{
				Numerator = fractionalFee.FractionalAmount.Numerator,
				Denominator = fractionalFee.FractionalAmount.Denominator,
				Min = fractionalFee.MinimumAmount,
				Max = fractionalFee.MaximumAmount,
				AssessmentMethod = fractionalFee.NetOfTransfers ? FeeAssessmentMethod.Exclusive : FeeAssessmentMethod.Inclusive,

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
			return (CustomFractionalFee) new CustomFractionalFee
			{
				Numerator = Numerator,
				Denominator = Denominator,
				Min = Min,
				Max = Max,
				AssessmentMethod = AssessmentMethod,

			}.FinishDeepClone(this);
		}
		/**
         * Convert the fractional fee object to a protobuf.
         *
         * @return the protobuf object
         */
		public Proto.FractionalFee ToFractionalFeeProtobuf()
		{
			return new Proto.FractionalFee
			{
				MinimumAmount = Min,
				MaximumAmount = Max,
				NetOfTransfers = AssessmentMethod == FeeAssessmentMethod.Exclusive,
				FractionalAmount = new Proto.Fraction
				{
					Numerator = Numerator,
					Denominator = Denominator,
				},

			};
		}   
    }
}