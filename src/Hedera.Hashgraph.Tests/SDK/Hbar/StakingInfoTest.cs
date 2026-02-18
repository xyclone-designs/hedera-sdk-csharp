// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;

using System;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.Tests.SDK.HBar
{
    public class StakingInfoTest
    {
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual StakingInfo SpawnStakingInfoAccountExample()
        {
            return new StakingInfo(true, Timestamp.FromDateTimeOffset(validStart), Hbar.From(5), Hbar.From(10), AccountId.FromString("1.2.3"), null);
        }

        public virtual StakingInfo SpawnStakingInfoNodeExample()
        {
            return new StakingInfo(true, Timestamp.FromDateTimeOffset(validStart), Hbar.From(5), Hbar.From(10), null, 3);
        }

        public virtual void ShouldSerializeAccount()
        {
            var originalStakingInfo = SpawnStakingInfoAccountExample();
            byte[] stakingInfoBytes = originalStakingInfo.ToBytes();
            var copyStakingInfo = StakingInfo.FromBytes(stakingInfoBytes);
            Assert.Equal(Regex.Replace(copyStakingInfo.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(originalStakingInfo.ToString(), "@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(Regex.Replace(originalStakingInfo.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        public virtual void ShouldSerializeNode()
        {
            var originalStakingInfo = SpawnStakingInfoNodeExample();
            byte[] stakingInfoBytes = originalStakingInfo.ToBytes();
            var copyStakingInfo = StakingInfo.FromBytes(stakingInfoBytes);
            Assert.Equal(Regex.Replace(copyStakingInfo.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(originalStakingInfo.ToString(), "@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(Regex.Replace(originalStakingInfo.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}