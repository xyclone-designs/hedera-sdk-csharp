// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class CustomRoyaltyFeeTest
    {
        private static readonly bool allCollectorsAreExempt = true;
        private static readonly AccountId feeCollectorAccountId = new AccountId(1, 2, 3);
        private static readonly int numerator = 4;
        private static readonly int denominator = 5;
        private static readonly CustomFixedFee fallbackFee = new CustomFixedFee().SetAmount(6);
        private readonly RoyaltyFee fee = RoyaltyFee.NewBuilder().SetExchangeValueFraction(Fraction.NewBuilder().SetNumerator(numerator).SetDenominator(denominator)).SetFallbackFee(FixedFee.NewBuilder().SetAmount(6).Build()).Build();
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(CustomRoyaltyFee.FromProtobuf(fee).ToString()).ToMatchSnapshot();
        }

        virtual void DeepCloneSubclass()
        {
            var customRoyaltyFee = new CustomRoyaltyFee().SetFeeCollectorAccountId(feeCollectorAccountId).SetAllCollectorsAreExempt(allCollectorsAreExempt);
            var clonedCustomRoyaltyFee = customRoyaltyFee.DeepCloneSubclass();
            Assert.Equal(clonedCustomRoyaltyFee.GetFeeCollectorAccountId(), feeCollectorAccountId);
            Assert.Equal(clonedCustomRoyaltyFee.GetAllCollectorsAreExempt(), allCollectorsAreExempt);
        }

        virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(CustomRoyaltyFee.FromProtobuf(fee).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        virtual void GetSetNumerator()
        {
            var customRoyaltyFee = new CustomRoyaltyFee().SetNumerator(numerator);
            Assert.Equal(customRoyaltyFee.GetNumerator(), numerator);
        }

        virtual void GetSetDenominator()
        {
            var customRoyaltyFee = new CustomRoyaltyFee().SetDenominator(denominator);
            Assert.Equal(customRoyaltyFee.GetDenominator(), denominator);
        }

        virtual void GetSetFallbackFee()
        {
            var customRoyaltyFee = new CustomRoyaltyFee().SetFallbackFee(fallbackFee);
            AssertThat(customRoyaltyFee.GetFallbackFee()).IsNotNull();
            Assert.Equal(customRoyaltyFee.GetFallbackFee().GetAmount(), fallbackFee.GetAmount());
        }
    }
}