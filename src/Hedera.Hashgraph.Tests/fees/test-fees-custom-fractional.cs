// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Fee;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class CustomFractionalFeeTest
    {
        private static readonly bool allCollectorsAreExempt = true;
        private static readonly AccountId feeCollectorAccountId = new AccountId(1, 2, 3);
        private static readonly int numerator = 4;
        private static readonly int denominator = 5;
        private static readonly int minAmount = 6;
        private static readonly int maxAmount = 7;
        private static readonly FeeAssessmentMethod feeAssessmentMethod = FeeAssessmentMethod.Exclusive;
        private readonly Proto.Services.FractionalFee fee = new Proto.Services.FractionalFee
		{
			NetOfTransfers = true,
			MinimumAmount = minAmount,
			MaximumAmount = maxAmount,
			FractionalAmount = new Proto.Services.Fraction
			{
				Numerator = numerator,
				Denominator = denominator,
			},
		};

        public virtual void FromProtobuf()
        {
            Verifier.Verify(CustomFractionalFee.FromProtobuf(fee).ToString());
        }
        [Fact]
        public virtual void DeepCloneSubclass()
        {
            var customFractionalFee = new CustomFractionalFee
            {
				FeeCollectorAccountId = feeCollectorAccountId,
                AllCollectorsAreExempt = allCollectorsAreExempt
            };
            var clonedCustomFractionalFee = customFractionalFee.DeepCloneSubclass();

            Assert.Equal(clonedCustomFractionalFee.FeeCollectorAccountId, feeCollectorAccountId);
            Assert.Equal(clonedCustomFractionalFee.AllCollectorsAreExempt, allCollectorsAreExempt);
        }

        public virtual void ToProtobuf()
        {
            Verifier.Verify(CustomFractionalFee.FromProtobuf(fee).ToProtobuf().ToString());
        }
        [Fact]
        public virtual void GetSetNumerator()
        {
            var customFractionalFee = new CustomFractionalFee { Numerator = numerator };

            Assert.Equal(customFractionalFee.Numerator, numerator);
        }
        [Fact]
        public virtual void GetSetDenominator()
        {
            var customFractionalFee = new CustomFractionalFee { Denominator = denominator };

            Assert.Equal(customFractionalFee.Denominator, denominator);
        }
        [Fact]
        public virtual void GetSetMinimumAmount()
        {
            var customFractionalFee = new CustomFractionalFee { Min = minAmount };

            Assert.Equal(customFractionalFee.Min, minAmount);
        }
        [Fact]
        public virtual void GetSetMaximumAmount()
        {
            var customFractionalFee = new CustomFractionalFee { Max = maxAmount };

            Assert.Equal(customFractionalFee.Max, maxAmount);
        }
        [Fact]
        public virtual void GetSetAssessmentMethod()
        {
            var customFractionalFee = new CustomFractionalFee { AssessmentMethod = feeAssessmentMethod };

            Assert.Equal(customFractionalFee.AssessmentMethod, feeAssessmentMethod);
        }
    }
}