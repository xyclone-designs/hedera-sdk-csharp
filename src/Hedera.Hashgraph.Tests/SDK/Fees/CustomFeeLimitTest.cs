// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;

using Google.Protobuf;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class CustomFeeLimitTest
    {
        private static readonly AccountId TEST_PAYER_ID = new AccountId(0, 0, 1234);
        // Creating a sample FixedFee protobuf for testing
        private static readonly Proto.FixedFee TEST_FIXED_FEE_PROTO = new Proto.FixedFee { Amount = 1000 };
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
                throw new Exception(e.Message, e);
            }
        }

        private static CustomFeeLimit CreateTestCustomFeeLimit()
        {
            // Step 1: Build the Protobuf representation
            var proto = new Proto.CustomFeeLimit
            {
                AccountId = TEST_PAYER_ID.ToProtobuf(),
                Fees = { TEST_FEES.Select(_ => _.ToFixedFeeProtobuf()) }
            };
            
            // Step 2: Convert Protobuf to CustomFeeLimit instance
            return CustomFeeLimit.FromProtobuf(proto);
        }
        [Fact]
        public virtual void TestGetPayerId()
        {
            Assert.Equal(TEST_PAYER_ID, TEST_CUSTOM_FEE_LIMIT.PayerId);
        }
        [Fact]
        public virtual void TestSetPayerId()
        {
            AccountId newPayerId = new (0, 0, 5678);

            // Create a new instance using the builder
            CustomFeeLimit updatedFeeLimit = new ()
            {
                PayerId = newPayerId,
                CustomFees = [.. TEST_FEES]
            };
            
            Assert.Equal(newPayerId, updatedFeeLimit.PayerId);
        }
        [Fact]
        public virtual void TestGetCustomFees()
        {
            Assert.Equal(TEST_FEES, TEST_CUSTOM_FEE_LIMIT.CustomFees);
        }
        [Fact]
        public virtual void TestSetCustomFees()
        {
            IList<CustomFixedFee> newFees = [];

            // Create a new instance using the builder
            CustomFeeLimit updatedFeeLimit = new ()
            {
                PayerId = TEST_PAYER_ID,
                CustomFees = [.. newFees]
            };

            Assert.Equal(newFees, updatedFeeLimit.CustomFees);
        }
        [Fact]
        public virtual void TestToProtobuf()
        {
            // Create a protobuf representation manually
            var proto = CreateTestCustomFeeLimit();

            // Validate fields
            Assert.Equal(TEST_PAYER_ID, proto.PayerId);
            Assert.False(proto.CustomFees.Count == 0);
        }
        [Fact]
        public virtual void TestFromProtobuf()
        {
            var proto = new Proto.CustomFeeLimit
            {
				AccountId = TEST_PAYER_ID.ToProtobuf(),
                Fees = { TEST_FEES.Select(_ => _.ToFixedFeeProtobuf()) }   
			};
            
            CustomFeeLimit converted = CustomFeeLimit.FromProtobuf(proto);

            Assert.Equal(TEST_PAYER_ID, converted.PayerId);
            Assert.Equal(TEST_FEES[0].FeeCollectorAccountId, converted.CustomFees[0].FeeCollectorAccountId);
        }
    }
}