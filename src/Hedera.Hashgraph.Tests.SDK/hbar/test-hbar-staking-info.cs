// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;

using System;
using System.Text.RegularExpressions;

using VerifyXunit;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK.HBar
{
    public class StakingInfoTest
    {
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual StakingInfo SpawnStakingInfoAccountExample()
        {
            return new StakingInfo(true, validStart, Hbar.From(5), Hbar.From(10), AccountId.FromString("1.2.3"), null);
        }

        public virtual StakingInfo SpawnStakingInfoNodeExample()
        {
            return new StakingInfo(true, validStart, Hbar.From(5), Hbar.From(10), null, 3);
        }
        [Fact]
        public virtual void ShouldSerializeAccount()
        {
            var originalStakingInfo = SpawnStakingInfoAccountExample();
            byte[] stakingInfoBytes = originalStakingInfo.ToBytes();
            var copyStakingInfo = StakingInfo.FromBytes(stakingInfoBytes);
            
            Assert.Equal(Regex.Replace(copyStakingInfo.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(originalStakingInfo.ToString(), "@[A-Za-z0-9]+", ""));
            
            Verifier.Verify(Regex.Replace(originalStakingInfo.ToString(), "@[A-Za-z0-9]+", ""));
        }
        [Fact]
        public virtual void ShouldSerializeNode()
        {
            var originalStakingInfo = SpawnStakingInfoNodeExample();
            byte[] stakingInfoBytes = originalStakingInfo.ToBytes();
            var copyStakingInfo = StakingInfo.FromBytes(stakingInfoBytes);
            
            Assert.Equal(Regex.Replace(copyStakingInfo.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(originalStakingInfo.ToString(), "@[A-Za-z0-9]+", ""));
            
            Verifier.Verify(Regex.Replace(originalStakingInfo.ToString(), "@[A-Za-z0-9]+", ""));
        }
    }
}