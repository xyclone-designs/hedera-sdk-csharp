// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class HookCreationDetailsTest
    {
        public virtual void GettersAndHasAdminKeyWork()
        {
            var cid = new ContractId(0, 0, 77);
            var lambda = new EvmHook(cid);
            var admin = PrivateKey.GenerateED25519().GetPublicKey();
            var withAdmin = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 9, lambda, admin);
            Assert.Equal(HookExtensionPoint.AccountAllowanceHook, withAdmin.ExtensionPoint);
            Assert.Equal(9, withAdmin.HookId);
            Assert.Equal(lambda, withAdmin.Hook);
            Assert.True(withAdmin.HasAdminKey);
            Assert.Equal(admin, withAdmin.AdminKey);
            var withoutAdmin = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 10, lambda);
            Assert.False(withoutAdmin.HasAdminKey);
            Assert.Null(withoutAdmin.AdminKey);
        }

        public virtual void ProtobufRoundTripPreservesValues()
        {
            var cid = new ContractId(0, 0, 1234);
            var lambda = new EvmHook(cid);
            var details = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 42, lambda, PrivateKey.GenerateED25519().GetPublicKey());
            var proto = details.ToProtobuf();
            var restored = HookCreationDetails.FromProtobuf(proto);
            Assert.Equal(details.ExtensionPoint, restored.ExtensionPoint);
            Assert.Equal(details.HookId, restored.HookId);
            Assert.Equal(details.Hook, restored.Hook);
            Assert.Equal(details.AdminKey, restored.AdminKey);
            Assert.Equal(details, restored);
            Assert.Equal(details.GetHashCode(), restored.GetHashCode());
        }

        public virtual void EqualsAndHashCodeVaryByFields()
        {
            var lambda1 = new EvmHook(new ContractId(0, 0, 1));
            var lambda2 = new EvmHook(new ContractId(0, 0, 2));
            var a = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambda1);
            var b = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambda1);
            var c = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, lambda1);
            var d = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambda2);
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.NotEqual(a, c);
            Assert.NotEqual(a, d);
        }

        public virtual void ToStringContainsKeyFields()
        {
            var lambda = new EvmHook(new ContractId(0, 0, 3));
            var details = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 7, lambda);
            var s = details.ToString();
            Assert.True(s.Contains("extensionPoint"));
            Assert.True(s.Contains("hookId"));
            Assert.True(s.Contains("hook"));
        }
    }
}