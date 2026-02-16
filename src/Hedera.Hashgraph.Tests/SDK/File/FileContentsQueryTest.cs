// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.File;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.Tests.SDK.File
{
    public class FileContentsQueryTest
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
            new FileContentsQuery
            {
				FileId = FileId.FromString("0.0.5005")

			}.OnMakeRequest(builder, new Proto.QueryHeader());

            SnapshotMatcher.Expect(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}