// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.LiveHashes;

using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.Tests.SDK.LiveHashes
{
    public class LiveHashQueryTest
    {
        private static readonly byte[] hash = [0, 1, 2];

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
            new LiveHashQuery
            {
                Hash = hash,
				AccountId = AccountId.FromString("0.0.100"),

			}.OnMakeRequest(builder, new Proto.QueryHeader());

            SnapshotMatcher.Expect(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}