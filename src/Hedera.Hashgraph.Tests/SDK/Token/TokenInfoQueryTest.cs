// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Token;

using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenInfoQueryTest
    {
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        
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
            new TokenInfoQuery
            {
				TokenId = testTokenId,
				MaxQueryPayment = Hbar.FromTinybars(100000)

			}.OnMakeRequest(builder, new Proto.QueryHeader());
            
            SnapshotMatcher.Expect(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
        public virtual void GetSetTokenId()
        {
            var tokenInfoQuery = new TokenInfoQuery
            {
				TokenId = testTokenId
			};

            Assert.Equal(tokenInfoQuery.TokenId, testTokenId);
        }
    }
}