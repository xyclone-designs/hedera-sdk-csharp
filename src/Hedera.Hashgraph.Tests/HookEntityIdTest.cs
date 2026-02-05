// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class HookEntityIdTest
    {
        virtual void AccountVariantToFromProto()
        {
            var acct = new AccountId(0, 0, 1234);
            var id = new HookEntityId(acct);
            AssertThat(id.IsAccount()).IsTrue();
            AssertThat(id.IsContract()).IsFalse();
            Assert.Equal(id.GetAccountId(), acct);
            AssertThat(id.GetContractId()).IsNull();
            var proto = id.ToProtobuf();
            var parsed = HookEntityId.FromProtobuf(proto);
            Assert.Equal(parsed, id);
            Assert.Equal(parsed.GetHashCode(), id.GetHashCode());
        }

        virtual void ContractVariantToFromProto()
        {
            var contract = new ContractId(0, 0, 5678);
            var id = new HookEntityId(contract);
            AssertThat(id.IsAccount()).IsFalse();
            AssertThat(id.IsContract()).IsTrue();
            AssertThat(id.GetAccountId()).IsNull();
            Assert.Equal(id.GetContractId(), contract);
            var proto = id.ToProtobuf();
            var parsed = HookEntityId.FromProtobuf(proto);
            Assert.Equal(parsed, id);
        }
    }
}