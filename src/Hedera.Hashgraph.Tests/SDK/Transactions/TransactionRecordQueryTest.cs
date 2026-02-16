// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class TransactionRecordQueryTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }
        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerialize()
        {
            var builder = new Proto.Query();
            SpawnQuery().OnMakeRequest(builder, new Proto.QueryHeader());

            SnapshotMatcher.Expect(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        private TransactionRecordQuery SpawnQuery()
        {
            return new TransactionRecordQuery
            {
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5005"), Timestamp.FromDateTimeOffset(validStart)),
				IncludeChildren = true,
				IncludeDuplicates = true,
			};
        }
    }
}