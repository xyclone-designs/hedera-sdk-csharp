// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Token;

using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    /// <include file="test-token-relationship.cs.xml" path="docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Token.TokenRelationshipTest"]" />
    public class TokenRelationshipTest
    {
        public virtual TokenRelationship SpawnTokenRelationshipExample()
        {
            return new TokenRelationship(TokenId.FromString("1.2.3"), "ABC", 55, true, true, 4, true);
        }
        [Fact]
        /// <include file="test-token-relationship.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Token.TokenRelationshipTest.ShouldSerializeTokenRelationship"]" />
        public virtual void ShouldSerializeTokenRelationship()
        {
            var originalTokenRelationship = SpawnTokenRelationshipExample();
            byte[] tokenRelationshipBytes = originalTokenRelationship.ToBytes();
            var copyTokenRelationship = TokenRelationship.FromBytes(tokenRelationshipBytes);
            
            Assert.Equal(Regex.Replace(copyTokenRelationship.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(originalTokenRelationship.ToString(), "@[A-Za-z0-9]+", ""));
            
            Verifier.Verify(Regex.Replace(originalTokenRelationship.ToString(), "@[A-Za-z0-9]+", ""));
        }
    }
}
