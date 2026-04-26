// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Token;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenTypeTest
    {
        private readonly TokenType tokenTypeFungible = TokenType.FungibleCommon;
        private readonly TokenType tokenTypeNonFungible = TokenType.NonFungibleUnique;

        public virtual void FromProtobuf()
        {
            Verifier.Verify(tokenTypeFungible.ToString(), tokenTypeNonFungible.ToString());

        }

        public virtual void ToProtobuf()
        {
            //Verifier.Verify((Proto.TokenType)tokenTypeFungible, (Proto.TokenType)tokenTypeNonFungible);
        }
    }
}