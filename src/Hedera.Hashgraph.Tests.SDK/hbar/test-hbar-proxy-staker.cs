// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.HBar
{
    public class ProxyStakerTest
    {
        private static readonly Proto.Services.ProxyStaker proxyStaker = new Proto.Services.ProxyStaker 
        { 
            AccountId = new AccountId(0, 0, 100).ToProtobuf(),
            Amount = 10
        };

        public virtual void FromProtobuf()
        {
            Verifier.Verify(ProxyStaker.FromProtobuf(proxyStaker).ToString());
        }
    }
}