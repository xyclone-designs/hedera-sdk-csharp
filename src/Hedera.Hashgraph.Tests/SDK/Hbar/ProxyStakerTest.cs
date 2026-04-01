// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.HBar
{
    public class ProxyStakerTest
    {
        private static readonly Proto.ProxyStaker proxyStaker = new Proto.ProxyStaker 
        { 
            AccountID = new AccountId(0, 0, 100).ToProtobuf(),
            Amount = 10
        };

        public virtual void FromProtobuf()
        {
            Verifier.Verify(ProxyStaker.FromProtobuf(proxyStaker).ToString());
        }
    }
}