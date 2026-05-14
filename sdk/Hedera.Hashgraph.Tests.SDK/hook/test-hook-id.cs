// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    /// <include file="test-hook-id.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Hook.HookIdTest"]' />
    public class HookIdTest
    {
        [Fact]
        /// <include file="test-hook-id.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Hook.HookIdTest.ToFromProtoAndEquality"]' />
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
