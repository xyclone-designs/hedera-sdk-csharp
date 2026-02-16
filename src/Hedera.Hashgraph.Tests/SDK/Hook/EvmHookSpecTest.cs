// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class EvmHookSpecTest
    {
        public virtual void ConstructorRejectsNullContractId()
        {
            NullReferenceException ex = Assert.Throws<NullReferenceException>(() => new EvmHook((ContractId)null));
            Assert.True(ex.GetMessage().Contains("contractId cannot be null"));
        }

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
            AssertNotEquals(a, c);
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