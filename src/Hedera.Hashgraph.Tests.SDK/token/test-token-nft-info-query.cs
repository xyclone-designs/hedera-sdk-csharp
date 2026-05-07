// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Cryptocurrency;

using System.Text.RegularExpressions;

using VerifyXunit;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK.Nfts
{
    public class TokenNftInfoQueryTest
    {
        public virtual void ShouldSerialize()
        {
            var builder = new Proto.Services.Query();
            new TokenNftInfoQuery
            {
                NftId = TokenId.FromString("0.0.5005").Nft(101), 
                MaxQueryPayment = Hbar.FromTinybars(100000)

            }.OnMakeRequest(builder, new Proto.Services.QueryHeader());

            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
        [Fact]
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