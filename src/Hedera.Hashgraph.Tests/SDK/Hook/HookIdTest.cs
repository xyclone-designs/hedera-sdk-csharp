// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.SDK.Hook
{
    class HookIdTest
    {
        public virtual void ToFromProtoAndEquality()
        {
            var acct = new AccountId(0, 0, 1001);
            var entity = new HookEntityId(acct);
            var hookId = new HookId(entity, 42);
            var proto = hookId.ToProtobuf();
            var parsed = HookId.FromProtobuf(proto);
            Assert.Equal(parsed, hookId);
            Assert.Equal(parsed.GetHashCode(), hookId.GetHashCode());
            Assert.Equal(parsed.GetEntityId(), entity);
            Assert.Equal(parsed.GetHookId(), 42);
        }
    }
}