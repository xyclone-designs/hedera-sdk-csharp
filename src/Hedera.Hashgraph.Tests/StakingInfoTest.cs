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
    public class StakingInfoTest
    {
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual StakingInfo SpawnStakingInfoAccountExample()
        {
            return new StakingInfo(true, validStart, Hbar.From(5), Hbar.From(10), AccountId.FromString("1.2.3"), null);
        }

        virtual StakingInfo SpawnStakingInfoNodeExample()
        {
            return new StakingInfo(true, validStart, Hbar.From(5), Hbar.From(10), null, 3);
        }

        virtual void ShouldSerializeAccount()
        {
            var originalStakingInfo = SpawnStakingInfoAccountExample();
            byte[] stakingInfoBytes = originalStakingInfo.ToBytes();
            var copyStakingInfo = StakingInfo.FromBytes(stakingInfoBytes);
            Assert.Equal(copyStakingInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""), originalStakingInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(originalStakingInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        virtual void ShouldSerializeNode()
        {
            var originalStakingInfo = SpawnStakingInfoNodeExample();
            byte[] stakingInfoBytes = originalStakingInfo.ToBytes();
            var copyStakingInfo = StakingInfo.FromBytes(stakingInfoBytes);
            Assert.Equal(copyStakingInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""), originalStakingInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(originalStakingInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}