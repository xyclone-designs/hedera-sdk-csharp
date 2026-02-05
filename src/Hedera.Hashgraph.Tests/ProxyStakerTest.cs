// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ProxyStakerTest
    {
        private static readonly ProxyStaker proxyStaker = ProxyStaker.NewBuilder().SetAccountID(new AccountId(0, 0, 100).ToProtobuf()).SetAmount(10).Build();
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void FromProtobuf()
        {
            SnapshotMatcher.Expect(com.hedera.hashgraph.sdk.ProxyStaker.FromProtobuf(proxyStaker).ToString()).ToMatchSnapshot();
        }
    }
}