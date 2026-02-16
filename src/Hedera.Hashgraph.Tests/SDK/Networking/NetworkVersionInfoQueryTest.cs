// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Networking;

using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class NetworkVersionInfoQueryTest
    {
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
            new NetworkVersionInfoQuery
            {
				MaxQueryPayment = Hbar.FromTinybars(100000)

			}.OnMakeRequest(builder, new Proto.QueryHeader
            {
                Payment = new Proto.Transaction
                {
                    SignedTransactionBytes = ByteString.FromHex("deadbeef")
                }
            });

            SnapshotMatcher.Expect(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}