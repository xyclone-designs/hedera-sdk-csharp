// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;

using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.Tests.SDK.Nfts
{
    public class TokenNftInfoQueryTest
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
            new TokenNftInfoQuery
            {
                NftId = TokenId.FromString("0.0.5005").Nft(101), 
                MaxQueryPayment = Hbar.FromTinybars(100000)

            }.OnMakeRequest(builder, new Proto.QueryHeader());

            SnapshotMatcher.Expect(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        public virtual void PropertiesTest()
        {
            var tokenId = TokenId.FromString("0.0.5005");
            var query = new TokenNftInfoQuery
            {
                AccountId = AccountId.FromString("0.0.123"),
                TokenId = tokenId,
				Start = 5,
				End = 8,
				NftId = tokenId.Nft(101),
				MaxQueryPayment = Hbar.FromTinybars(100000)
			};

            Assert.Equal("0.0.5005/101", query.NftId.ToString());
            Assert.Equal("0.0.123", query.AccountId.ToString());
            Assert.Equal(query.TokenId, tokenId);
            Assert.Equal(query.Start, 5);
            Assert.Equal(query.End, 8);
        }
    }
}