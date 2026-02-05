// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// Custom royalty fee utility class.
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#royalty-fee">Hedera Documentation</a>
    /// </summary>
    public class CustomRoyaltyFee : CustomFeeBase<CustomRoyaltyFee>
    {
		/// <summary>
		/// Create a custom royalty fee from a royalty fee protobuf.
		/// </summary>
		/// <param name="royaltyFee">the royalty fee protobuf</param>
		/// <returns>                         the new royalty fee object</returns>
		public static CustomRoyaltyFee FromProtobuf(Proto.RoyaltyFee royaltyFee)
        {
            CustomRoyaltyFee customroyaltyfee = new ()
            {
				Numerator = royaltyFee.ExchangeValueFraction.Numerator,
				Denominator = royaltyFee.ExchangeValueFraction.Denominator
			};
            
            if (royaltyFee.FallbackFee is not null)
				customroyaltyfee.FallbackFee = CustomFixedFee.FromProtobuf(royaltyFee.FallbackFee);

			return customroyaltyfee;
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

        /// <summary>
        /// Extract the numerator.
        /// </summary>
        public virtual long Numerator { get; set; } = 0;
        /// <summary>
        /// Extract the denominator.
        /// </summary>
        public virtual long Denominator { get; set; } = 1;

        /// <summary>
        /// The fallback fee is a fixed fee that is charged to the NFT receiver
        /// when there is no fungible value exchanged with the sender of the NFT.
        /// </summary>
        public virtual CustomFixedFee? FallbackFee
        {
			get => field?.DeepCloneSubclass();
			set => field = value?.DeepCloneSubclass();
        }

		public override Proto.CustomFee ToProtobuf()
		{
			return FinishToProtobuf(new Proto.CustomFee
			{
				RoyaltyFee = ToRoyaltyFeeProtobuf()
			});
		}
		/// <summary>
		/// Convert the royalty fee object to a protobuf.
		/// </summary>
		/// <returns>                         the protobuf object</returns>
		public virtual Proto.RoyaltyFee ToRoyaltyFeeProtobuf()
        {
            Proto.RoyaltyFee proto = new()
            { 
                ExchangeValueFraction = new Proto.Fraction
                {
					Numerator = Numerator,
					Denominator = Denominator,
				}
			};

            if (FallbackFee != null)
				proto.FallbackFee = FallbackFee.ToFixedFeeProtobuf();

			return proto;
        }
    }
}