// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class HookEntityIdTest
    {
        public virtual void AccountVariantToFromProto()
        {
            var acct = new AccountId(0, 0, 1234);
            var id = new HookEntityId(acct);
            Assert.True(id.IsAccount);
            Assert.False(id.IsContract);
            Assert.Equal(id.AccountId, acct);
            Assert.Null(id.ContractId;
            var proto = id.ToProtobuf();
            var parsed = HookEntityId.FromProtobuf(proto);
            Assert.Equal(parsed, id);
            Assert.Equal(parsed.GetHashCode(), id.GetHashCode());
        }

        public virtual void ContractVariantToFromProto()
        {
            var contract = new ContractId(0, 0, 5678);
            var id = new HookEntityId(contract);
            Assert.False(id.IsAccount);
            Assert.True(id.IsContract);
            Assert.Null(id.AccountId);
            Assert.Equal(id.ContractId, contract);
            var proto = id.ToProtobuf();
            var parsed = HookEntityId.FromProtobuf(proto);
            Assert.Equal(parsed, id);
        }
    }
}