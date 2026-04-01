// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Networking;

using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class NetworkVersionInfoTest
    {
        public virtual NetworkVersionInfo SpawnNetworkVerionInfoExample()
        {
            return new NetworkVersionInfo(new SemanticVersion(1, 2, 3), new SemanticVersion(4, 5, 6));
        }

        public virtual void ShouldSerialize()
        {
            var originalNetworkVersionInfo = SpawnNetworkVerionInfoExample();
            byte[] networkVersionInfoBytes = originalNetworkVersionInfo.ToBytes();
            var copyNetworkVersionInfo = NetworkVersionInfo.FromBytes(networkVersionInfoBytes);
            
            Assert.Equal(Regex.Replace(originalNetworkVersionInfo.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(copyNetworkVersionInfo.ToString(), "@[A-Za-z0-9]+", ""));
            
            Verifier.Verify(Regex.Replace(originalNetworkVersionInfo.ToString(), "@[A-Za-z0-9]+", ""));
        }
    }
}