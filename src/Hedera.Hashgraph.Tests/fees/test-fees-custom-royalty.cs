// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Cryptocurrency;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class CustomRoyaltyFeeTest
    {
        private static readonly bool allCollectorsAreExempt = true;
        private static readonly AccountId feeCollectorAccountId = new AccountId(1, 2, 3);
        private static readonly int numerator = 4;
        private static readonly int denominator = 5;
        private static readonly CustomFixedFee fallbackFee = new CustomFixedFee { Amount = 6 };
        private readonly Proto.Services.RoyaltyFee fee = new Proto.Services.RoyaltyFee
        {
            FallbackFee = new Proto.Services.FixedFee { Amount = 6 },
            ExchangeValueFraction = new Proto.Services.Fraction
            {
                Numerator = numerator,
                Denominator = denominator,
            },
        };

        public virtual void FromProtobuf()
        {
            Verifier.Verify(CustomRoyaltyFee.FromProtobuf(fee).ToString());
        }
        [Fact]
        public virtual void DeepCloneSubclass()
        {
            var customRoyaltyFee = new CustomRoyaltyFee
            {
				FeeCollectorAccountId = feeCollectorAccountId,
				AllCollectorsAreExempt = allCollectorsAreExempt,
			};
            var clonedCustomRoyaltyFee = customRoyaltyFee.DeepCloneSubclass();
            
            Assert.Equal(clonedCustomRoyaltyFee.FeeCollectorAccountId, feeCollectorAccountId);
            Assert.Equal(clonedCustomRoyaltyFee.AllCollectorsAreExempt, allCollectorsAreExempt);
        }

        public virtual void ToProtobuf()
        {
            Verifier.Verify(CustomRoyaltyFee.FromProtobuf(fee).ToProtobuf().ToString());
        }
        [Fact]
        public virtual void GetSetNumerator()
        {
            var customRoyaltyFee = new CustomRoyaltyFee { Numerator = numerator };
            
            Assert.Equal(customRoyaltyFee.Numerator, numerator);
        }
        [Fact]
        public virtual void GetSetDenominator()
        {
            var customRoyaltyFee = new CustomRoyaltyFee { Denominator = denominator };
            
            Assert.Equal(customRoyaltyFee.Denominator, denominator);
        }
        [Fact]
        public virtual void GetSetFallbackFee()
        {
            var customRoyaltyFee = new CustomRoyaltyFee { FallbackFee = fallbackFee };
            
            Assert.NotNull(customRoyaltyFee.FallbackFee);
            Assert.Equal(customRoyaltyFee.FallbackFee.Amount, fallbackFee.Amount);
        }
    }
}