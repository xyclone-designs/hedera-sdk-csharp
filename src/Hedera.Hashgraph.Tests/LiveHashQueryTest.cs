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
    public class LiveHashQueryTest
    {
        private static readonly byte[] hash = new[]
        {
            0,
            1,
            2
        };
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void ShouldSerialize()
        {
            var builder = com.hedera.hashgraph.sdk.proto.Query.NewBuilder();
            new LiveHashQuery().SetAccountId(AccountId.FromString("0.0.100")).SetHash(hash).OnMakeRequest(builder, QueryHeader.NewBuilder().Build());
            SnapshotMatcher.Expect(builder.Build().ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}