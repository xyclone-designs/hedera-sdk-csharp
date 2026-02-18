// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.Tests.SDK.HBar
{
    public class ProxyStakerTest
    {
        private static readonly Proto.ProxyStaker proxyStaker = new Proto.ProxyStaker 
        { 
            AccountID = new AccountId(0, 0, 100).ToProtobuf(),
            Amount = 10
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
            SnapshotMatcher.Expect(ProxyStaker.FromProtobuf(proxyStaker).ToString()).ToMatchSnapshot();
        }
    }
}