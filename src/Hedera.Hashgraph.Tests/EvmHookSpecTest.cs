// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class EvmHookSpecTest
    {
        virtual void ConstructorRejectsNullContractId()
        {
            NullReferenceException ex = await Assert.ThrowsAsync<NullReferenceException>(() => new EvmHook((ContractId)null));
            AssertTrue(ex.GetMessage().Contains("contractId cannot be null"));
        }

        virtual void GetContractIdReturnsProvidedValue()
        {
            var cid = new ContractId(0, 0, 1234);
            var spec = new EvmHook(cid);
            AssertEquals(cid, spec.GetContractId());
        }

        virtual void EqualsAndHashCodeDependOnContractId()
        {
            var a = new EvmHook(new ContractId(0, 0, 1));
            var b = new EvmHook(new ContractId(0, 0, 1));
            var c = new EvmHook(new ContractId(0, 0, 2));
            AssertEquals(a, b);
            AssertEquals(a.GetHashCode(), b.GetHashCode());
            AssertNotEquals(a, c);
        }

        virtual void ToStringContainsContractId()
        {
            var cid = new ContractId(0, 0, 42);
            var spec = new EvmHook(cid);
            var s = spec.ToString();
            AssertTrue(s.Contains("contractId"));
            AssertTrue(s.Contains("0.0.42"));
        }
    }
}