// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="CustomRoyaltyFee.cs.xml" path='docs/member[@name="T:CustomRoyaltyFee"]/*' />
    public class CustomRoyaltyFee : CustomFeeBase<CustomRoyaltyFee>
    {
		/// <include file="CustomRoyaltyFee.cs.xml" path='docs/member[@name="M:CustomRoyaltyFee.FromProtobuf(Proto.Services.RoyaltyFee)"]/*' />
		public static CustomRoyaltyFee FromProtobuf(Proto.Services.RoyaltyFee royaltyFee)
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

		public override Proto.Services.CustomFee ToProtobuf()
		{
			return FinishToProtobuf(new Proto.Services.CustomFee
			{
				RoyaltyFee = ToRoyaltyFeeProtobuf()
			});
		}
		/// <include file="CustomRoyaltyFee.cs.xml" path='docs/member[@name="M:CustomRoyaltyFee.ToRoyaltyFeeProtobuf"]/*' />
		public virtual Proto.Services.RoyaltyFee ToRoyaltyFeeProtobuf()
        {
            Proto.Services.RoyaltyFee proto = new()
            { 
                ExchangeValueFraction = new Proto.Services.Fraction
                {
					Numerator = Numerator,
					Denominator = Denominator,
				}
			};

            if (FallbackFee != null)
				Proto.Services.FallbackFee = FallbackFee.ToFixedFeeProtobuf();

			return proto;
        }
    }
}
