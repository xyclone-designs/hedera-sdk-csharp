// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text.RegularExpressions;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Transactions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Fees
{
    public class FeeSchedulesTest
    {
        public virtual FeeSchedules SpawnFeeSchedulesExample()
        {
            return new FeeSchedules
            {
                Current = new FeeSchedule
                {
                    ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1554158542),
                    TransactionFeeSchedules = [ new TransactionFeeSchedule
                    {
                        Fees = [ new FeeData
                        {
                            NodeData = new FeeComponents(),
                            NetworkData = new FeeComponents { Min = 2, Max = 5 },
                            ServiceData = new FeeComponents(),
                        }]
                    }]
                },
                Next = new FeeSchedule
                {
                    ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1554158222),
                    TransactionFeeSchedules = [ new TransactionFeeSchedule
                    {
                        Fees = [ new FeeData
                        {
                            NodeData = new FeeComponents(),
                            NetworkData = new FeeComponents { Min = 1, Max = 2 },
                            ServiceData = new FeeComponents(),
                        }]
                    }]
                },
            };
        }
        [Fact]
        public virtual void ShouldSerialize()
        {
            var originalFeeSchedules = SpawnFeeSchedulesExample();
            byte[] feeSchedulesBytes = originalFeeSchedules.ToBytes();
            var copyFeeSchedules = FeeSchedules.FromBytes(feeSchedulesBytes);
            
            Assert.Equal(Regex.Replace(copyFeeSchedules.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(originalFeeSchedules.ToString(), "@[A-Za-z0-9]+", ""));
            
            Verifier.Verify(Regex.Replace(originalFeeSchedules.ToString(), "@[A-Za-z0-9]+", ""));
        }
        [Fact]
        public virtual void ShouldSerializeNull()
        {
            var originalFeeSchedules = new FeeSchedules();
            byte[] feeSchedulesBytes = originalFeeSchedules.ToBytes();
            var copyFeeSchedules = FeeSchedules.FromBytes(feeSchedulesBytes);

            Assert.Equal(copyFeeSchedules.ToString(), originalFeeSchedules.ToString());
        }
    }
}