// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="CustomRoyaltyFee.cs.xml" path='docs/member[@name="T:CustomRoyaltyFee"]/*' />
    public class CustomRoyaltyFee : CustomFeeBase<CustomRoyaltyFee>
    {
		/// <include file="CustomRoyaltyFee.cs.xml" path='docs/member[@name="M:CustomRoyaltyFee.FromProtobuf(Proto.RoyaltyFee)"]/*' />
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

        /// <include file="CustomRoyaltyFee.cs.xml" path='docs/member[@name="P:CustomRoyaltyFee.Numerator"]/*' />
        public virtual long Numerator { get; set; } = 0;
        /// <include file="CustomRoyaltyFee.cs.xml" path='docs/member[@name="P:CustomRoyaltyFee.Denominator"]/*' />
        public virtual long Denominator { get; set; } = 1;

        /// <include file="CustomRoyaltyFee.cs.xml" path='docs/member[@name="M:CustomRoyaltyFee.DeepCloneSubclass"]/*' />
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
		/// <include file="CustomRoyaltyFee.cs.xml" path='docs/member[@name="M:CustomRoyaltyFee.ToRoyaltyFeeProtobuf"]/*' />
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