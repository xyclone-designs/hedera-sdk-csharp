// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class CustomRoyaltyFeeTest
    {
        private static readonly bool allCollectorsAreExempt = true;
        private static readonly AccountId feeCollectorAccountId = new AccountId(1, 2, 3);
        private static readonly int numerator = 4;
        private static readonly int denominator = 5;
        private static readonly CustomFixedFee fallbackFee = new CustomFixedFee { Amount = 6 };
        private readonly Proto.RoyaltyFee fee = new Proto.RoyaltyFee
        {
            FallbackFee = new Proto.FixedFee { Amount = 6 },
            ExchangeValueFraction = new Proto.Fraction
            {
                Numerator = numerator,
                Denominator = denominator,
            },
        };
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(CustomRoyaltyFee.FromProtobuf(fee).ToString()).ToMatchSnapshot();
        }

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
            SnapshotMatcher.Expect(CustomRoyaltyFee.FromProtobuf(fee).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        public virtual void GetSetNumerator()
        {
            var customRoyaltyFee = new CustomRoyaltyFee { Numerator = numerator };
            
            Assert.Equal(customRoyaltyFee.Numerator, numerator);
        }

        public virtual void GetSetDenominator()
        {
            var customRoyaltyFee = new CustomRoyaltyFee { Denominator = denominator };
            
            Assert.Equal(customRoyaltyFee.Denominator, denominator);
        }

        public virtual void GetSetFallbackFee()
        {
            var customRoyaltyFee = new CustomRoyaltyFee { FallbackFee = fallbackFee };
            
            Assert.NotNull(customRoyaltyFee.FallbackFee);
            Assert.Equal(customRoyaltyFee.FallbackFee.Amount, fallbackFee.Amount);
        }
    }
}