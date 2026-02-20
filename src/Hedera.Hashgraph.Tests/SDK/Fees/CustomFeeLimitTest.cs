// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using Com.Google.Protobuf;
using Proto;
using Io.Github.JsonSnapshot;
using Java.Util;
using Java.Util.Stream;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class CustomFeeLimitTest
    {
        private static readonly AccountId TEST_PAYER_ID = new AccountId(0, 0, 1234);
        // Creating a sample FixedFee protobuf for testing
        private static readonly FixedFee TEST_FIXED_FEE_PROTO = FixedFee.NewBuilder().SetAmount(1000).Build();
        // Using fromProtobuf() to properly initialize CustomFixedFee
        private static readonly CustomFixedFee TEST_CUSTOM_FIXED_FEE = CustomFixedFee.FromProtobuf(TEST_FIXED_FEE_PROTO);
        private static readonly List<CustomFixedFee> TEST_FEES = [TEST_CUSTOM_FIXED_FEE];
        // Instead of using a constructor, we initialize via fromProtobuf()
        private static readonly CustomFeeLimit TEST_CUSTOM_FEE_LIMIT;
        static CustomFeeLimitTest()
        {
            try
            {
                TEST_CUSTOM_FEE_LIMIT = CreateTestCustomFeeLimit();
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new Exception(e);
            }
        }

        private static CustomFeeLimit CreateTestCustomFeeLimit()
        {

            // Step 1: Build the Protobuf representation
            var proto = Proto.CustomFeeLimit.NewBuilder().SetAccountId(TEST_PAYER_ID.ToProtobuf()).AddAllFees(TEST_FEES.Stream().Map(CustomFixedFee.ToFixedFeeProtobuf()).ToList()).Build();

            // Step 2: Convert Protobuf to CustomFeeLimit instance
            return CustomFeeLimit.ParseFrom(proto.ToByteArray());
        }

        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void TestGetPayerId()
        {
            Assert.Equal(TEST_PAYER_ID, AccountId.FromProtobuf(TEST_CUSTOM_FEE_LIMIT.GetAccountId()));
        }

        public virtual void TestSetPayerId()
        {
            AccountId newPayerId = new AccountId(0, 0, 5678);

            // Create a new instance using the builder
            CustomFeeLimit updatedFeeLimit = CustomFeeLimit.NewBuilder().SetAccountId(newPayerId.ToProtobuf()).AddAllFees(TEST_FEES.Stream().Map(CustomFixedFee.ToFixedFeeProtobuf()).ToList()).Build();
            Assert.Equal(newPayerId, AccountId.FromProtobuf(updatedFeeLimit.GetAccountId()));
        }

        public virtual void TestGetCustomFees()
        {
            Assert.Equal(TEST_FEES.Stream().Map(CustomFixedFee.ToFixedFeeProtobuf()).Collect(Collectors.ToList()), TEST_CUSTOM_FEE_LIMIT.GetFeesList());
        }

        public virtual void TestSetCustomFees()
        {
            IList<CustomFixedFee> newFees = Collections.EmptyList();

            // Create a new instance using the builder
            CustomFeeLimit updatedFeeLimit = CustomFeeLimit.NewBuilder().SetAccountId(TEST_PAYER_ID.ToProtobuf()).AddAllFees(newFees.Stream().Map(CustomFixedFee.ToFixedFeeProtobuf()).ToList()).Build();
            Assert.Equal(newFees, updatedFeeLimit.GetFeesList());
        }

        public virtual void TestToProtobuf()
        {

            // Create a protobuf representation manually
            var proto = CreateTestCustomFeeLimit();

            // Validate fields
            Assert.Equal(TEST_PAYER_ID.ToProtobuf(), proto.GetAccountId());
            Assert.False(proto.GetFeesList().Count == 0);
        }

        public virtual void TestFromProtobuf()
        {
            var proto = CustomFeeLimit.NewBuilder().SetAccountId(TEST_PAYER_ID.ToProtobuf()).AddAllFees(TEST_FEES.Stream().Map(CustomFixedFee.ToFixedFeeProtobuf()).ToList()).Build();
            com.hedera.hashgraph.sdk.CustomFeeLimit converted = com.hedera.hashgraph.sdk.CustomFeeLimit.FromProtobuf(proto);
            Assert.Equal(TEST_PAYER_ID, converted.GetPayerId());
            Assert.Equal(TEST_FEES[0].feeCollectorAccountId, converted.GetCustomFees()[0].feeCollectorAccountId);
        }
    }
}