// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Token;

using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    /// <include file="test-token-info-query.ts.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Token.TokenInfoQueryTest"]' />
    public class TokenInfoQueryTest
    {
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        
        public virtual void ShouldSerialize()
        {
            var builder = new Proto.Services.Query();
            new TokenInfoQuery
            {
				TokenId = testTokenId,
				MaxQueryPayment = Hbar.FromTinybars(100000)

			}.OnMakeRequest(builder, new Proto.Services.QueryHeader());
            
            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
        [Fact]
        /// <include file="test-token-info-query.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenInfoQueryTest.GetSetTokenId"]' />
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
