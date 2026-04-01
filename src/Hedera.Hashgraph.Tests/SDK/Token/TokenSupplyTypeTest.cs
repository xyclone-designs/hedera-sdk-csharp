// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Token;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenSupplyTypeTest
    {
        private readonly TokenSupplyType tokenSupplyTypeInfinite = TokenSupplyType.Infinite;
        private readonly TokenSupplyType tokenSupplyTypeFinite = TokenSupplyType.Finite;

        public virtual void FromProtobuf()
        {
            Verifier.Verify(tokenSupplyTypeInfinite.ToString(), tokenSupplyTypeFinite.ToString());
		}

        public virtual void ToProtobuf()
        {
            //Verifier.Verify((Proto.TokenSupplyType)tokenSupplyTypeInfinite, (Proto.TokenSupplyType)tokenSupplyTypeFinite);
        }

        public virtual void TokenSupplyTestToString()
        {
            Assert.Equal(TokenSupplyType.Infinite.ToString(), "INFINITE");
            Assert.Equal(TokenSupplyType.Finite.ToString(), "FINITE");
        }
    }
}