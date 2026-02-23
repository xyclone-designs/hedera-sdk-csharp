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
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;

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
        private readonly Proto.FractionalFee fee = new Proto.FractionalFee
		{
			NetOfTransfers = true,
			MinimumAmount = minAmount,
			MaximumAmount = maxAmount,
			FractionalAmount = new Proto.Fraction
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
            SnapshotMatcher.Expect(CustomFractionalFee.FromProtobuf(fee).ToString()).ToMatchSnapshot();
        }

        public virtual void DeepCloneSubclass()
        {
            var customFractionalFee = new CustomFractionalFee
            {
				FeeCollectorAccountId = feeCollectorAccountId
			
            }
            .SetAllCollectorsAreExempt(allCollectorsAreExempt);
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
            var customFractionalFee = new CustomFractionalFee { Numerator = numerator };

            Assert.Equal(customFractionalFee.Numerator, numerator);
        }

        public virtual void GetSetDenominator()
        {
            var customFractionalFee = new CustomFractionalFee { Denominator = denominator };

            Assert.Equal(customFractionalFee.Denominator, denominator);
        }

        public virtual void GetSetMinimumAmount()
        {
            var customFractionalFee = new CustomFractionalFee { Min = minAmount };

            Assert.Equal(customFractionalFee.Min, minAmount);
        }

        public virtual void GetSetMaximumAmount()
        {
            var customFractionalFee = new CustomFractionalFee { Max = maxAmount };

            Assert.Equal(customFractionalFee.Max, maxAmount);
        }

        public virtual void GetSetAssessmentMethod()
        {
            var customFractionalFee = new CustomFractionalFee { AssessmentMethod = feeAssessmentMethod };

            Assert.Equal(customFractionalFee.AssessmentMethod, feeAssessmentMethod);
        }
    }
}