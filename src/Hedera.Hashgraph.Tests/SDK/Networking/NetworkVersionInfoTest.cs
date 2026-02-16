// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class NetworkVersionInfoTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual NetworkVersionInfo SpawnNetworkVerionInfoExample()
        {
            return new NetworkVersionInfo(new SemanticVersion(1, 2, 3), new SemanticVersion(4, 5, 6));
        }

        public virtual void ShouldSerialize()
        {
            var originalNetworkVersionInfo = SpawnNetworkVerionInfoExample();
            byte[] networkVersionInfoBytes = originalNetworkVersionInfo.ToBytes();
            var copyNetworkVersionInfo = NetworkVersionInfo.FromBytes(networkVersionInfoBytes);
            Assert.Equal(originalNetworkVersionInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""), copyNetworkVersionInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(originalNetworkVersionInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}