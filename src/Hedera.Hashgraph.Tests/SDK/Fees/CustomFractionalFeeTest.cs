// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Proto;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
        private static readonly FeeAssessmentMethod feeAssessmentMethod = FeeAssessmentMethod.EXCLUSIVE;
        private readonly FractionalFee fee = FractionalFee.NewBuilder().SetFractionalAmount(Fraction.NewBuilder().SetNumerator(numerator).SetDenominator(denominator)).SetMinimumAmount(minAmount).SetMaximumAmount(maxAmount).SetNetOfTransfers(true).Build();
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
            SnapshotMatcher.Expect(CustomFractionalFee.FromProtobuf(fee).ToString()).ToMatchSnapshot();
        }

        public virtual void DeepCloneSubclass()
        {
            var customFractionalFee = new CustomFractionalFee().SetFeeCollectorAccountId(feeCollectorAccountId).SetAllCollectorsAreExempt(allCollectorsAreExempt);
            var clonedCustomFractionalFee = customFractionalFee.DeepCloneSubclass();
            Assert.Equal(clonedCustomFractionalFee.GetFeeCollectorAccountId(), feeCollectorAccountId);
            Assert.Equal(clonedCustomFractionalFee.GetAllCollectorsAreExempt(), allCollectorsAreExempt);
        }

        public virtual void ToProtobuf()
        {
            SnapshotMatcher.Expect(CustomFractionalFee.FromProtobuf(fee).ToProtobuf().ToString()).ToMatchSnapshot();
        }

        public virtual void GetSetNumerator()
        {
            var customFractionalFee = new CustomFractionalFee().SetNumerator(numerator);
            Assert.Equal(customFractionalFee.GetNumerator(), numerator);
        }

        public virtual void GetSetDenominator()
        {
            var customFractionalFee = new CustomFractionalFee().SetDenominator(denominator);
            Assert.Equal(customFractionalFee.GetDenominator(), denominator);
        }

        public virtual void GetSetMinimumAmount()
        {
            var customFractionalFee = new CustomFractionalFee().SetMin(minAmount);
            Assert.Equal(customFractionalFee.GetMin(), minAmount);
        }

        public virtual void GetSetMaximumAmount()
        {
            var customFractionalFee = new CustomFractionalFee().SetMax(maxAmount);
            Assert.Equal(customFractionalFee.GetMax(), maxAmount);
        }

        public virtual void GetSetAssessmentMethod()
        {
            var customFractionalFee = new CustomFractionalFee().SetAssessmentMethod(feeAssessmentMethod);
            Assert.Equal(customFractionalFee.GetAssessmentMethod(), feeAssessmentMethod);
        }
    }
}