// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.Tests.SDK.Hook
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
            Assert.Equal(parsed.EntityId, entity);
            Assert.Equal(parsed.HookId_, 42);
        }
    }
}