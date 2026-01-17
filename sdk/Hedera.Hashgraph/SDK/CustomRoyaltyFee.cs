namespace Hedera.Hashgraph.SDK
{
	/**
     * Custom royalty fee utility class.
     * See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#royalty-fee">Hedera Documentation</a>
     */
    public class CustomRoyaltyFee : CustomFeeBase<CustomRoyaltyFee> 
    {
        public long Numerator { get; set; } = 0;
        public long Denominator { get; set; } = 1;
		/**
         * The fallback fee is a fixed fee that is charged to the NFT receiver
         * when there is no fungible value exchanged with the sender of the NFT.
         *
         * @param fallbackFee               the fallback fee amount
         * @return {@code this}
         */
		public CustomFixedFee? FallbackFee { get; set; } 

        /**
         * Constructor.
         */
        public CustomRoyaltyFee() {}

        /**
         * Create a custom royalty fee from a royalty fee protobuf.
         *
         * @param royaltyFee                the royalty fee protobuf
         * @return                          the new royalty fee object
         */
        public static CustomRoyaltyFee FromProtobuf(Proto.RoyaltyFee royaltyFee) 
        {
            return new CustomRoyaltyFee
            {
                Numerator = royaltyFee.ExchangeValueFraction.Numerator,
				Denominator = royaltyFee.ExchangeValueFraction.Denominator,
				FallbackFee = CustomFixedFee.FromProtobuf(royaltyFee.FallbackFee),
            };
        }

        /**
         * Convert the royalty fee object to a protobuf.
         *
         * @return                          the protobuf object
         */
        public Proto.RoyaltyFee ToRoyaltyFeeProtobuf()
        {
            return new Proto.RoyaltyFee
            {
                FallbackFee = FallbackFee?.ToFixedFeeProtobuf(),
                ExchangeValueFraction = new Proto.Fraction
                {
                    Numerator = Numerator,
					Denominator = Denominator,
                }
            };
        }

        public override Proto.CustomFee ToProtobuf() 
        {
            return FinishToProtobuf(new Proto.CustomFee
            {
                RoyaltyFee = ToRoyaltyFeeProtobuf()
            });
        }
		public override CustomRoyaltyFee DeepCloneSubclass()
		{
			return new CustomRoyaltyFee
			{
				Numerator = Numerator,
				Denominator = Denominator,
				FallbackFee = FallbackFee?.DeepCloneSubclass(),
				FeeCollectorAccountId = FeeCollectorAccountId,
				AllCollectorsAreExempt = AllCollectorsAreExempt,
			};
		}

	}
}