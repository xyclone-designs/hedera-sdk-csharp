// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Io.Github.JsonSnapshot;
using Java.Time;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class FeeSchedulesTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual FeeSchedules SpawnFeeSchedulesExample()
        {
            return new FeeSchedules().SetCurrent(new FeeSchedule().SetExpirationTime(Instant.OfEpochSecond(1554158542)).AddTransactionFeeSchedule(new TransactionFeeSchedule().AddFee(new FeeData().SetNodeData(new FeeComponents()).SetNetworkData(new FeeComponents().SetMin(2).SetMax(5)).SetServiceData(new FeeComponents())))).SetNext(new FeeSchedule().SetExpirationTime(Instant.OfEpochSecond(1554158222)).AddTransactionFeeSchedule(new TransactionFeeSchedule().AddFee(new FeeData().SetNodeData(new FeeComponents().SetMin(1).SetMax(2)).SetNetworkData(new FeeComponents()).SetServiceData(new FeeComponents()))));
        }

        virtual void ShouldSerialize()
        {
            var originalFeeSchedules = SpawnFeeSchedulesExample();
            byte[] feeSchedulesBytes = originalFeeSchedules.ToBytes();
            var copyFeeSchedules = FeeSchedules.FromBytes(feeSchedulesBytes);
            Assert.Equal(copyFeeSchedules.ToString().ReplaceAll("@[A-Za-z0-9]+", ""), originalFeeSchedules.ToString().ReplaceAll("@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(originalFeeSchedules.ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        virtual void ShouldSerializeNull()
        {
            var originalFeeSchedules = new FeeSchedules();
            byte[] feeSchedulesBytes = originalFeeSchedules.ToBytes();
            var copyFeeSchedules = FeeSchedules.FromBytes(feeSchedulesBytes);
            Assert.Equal(copyFeeSchedules.ToString(), originalFeeSchedules.ToString());
        }
    }
}