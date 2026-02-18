// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Contract;
using System;
using System.Linq;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class EvmHookSpecTest
    {
        public virtual void GetContractIdReturnsProvidedValue()
        {
            var cid = new ContractId(0, 0, 1234);
            var spec = new EvmHook(cid);
            Assert.Equal(cid, spec.GetContractId());
        }

        public virtual void EqualsAndHashCodeDependOnContractId()
        {
            var a = new EvmHook(new ContractId(0, 0, 1));
            var b = new EvmHook(new ContractId(0, 0, 1));
            var c = new EvmHook(new ContractId(0, 0, 2));
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.NotEqual(a, c);
        }

        public virtual void ToStringContainsContractId()
        {
            var cid = new ContractId(0, 0, 42);
            var spec = new EvmHook(cid);
            var s = spec.ToString();
            Assert.True(s.Contains("contractId"));
            Assert.True(s.Contains("0.0.42"));
        }
    }
}