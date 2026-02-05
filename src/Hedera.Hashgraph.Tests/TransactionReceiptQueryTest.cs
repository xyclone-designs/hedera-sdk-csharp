// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.Proto;
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
    public class TransactionReceiptQueryTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
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
            new TransactionReceiptQuery().SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5005"), validStart)).OnMakeRequest(builder, QueryHeader.NewBuilder().Build());
            SnapshotMatcher.Expect(builder.Build().ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}