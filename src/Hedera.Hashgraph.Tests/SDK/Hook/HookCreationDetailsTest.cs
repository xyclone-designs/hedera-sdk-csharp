// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.SDK.Hook
{
    class HookCreationDetailsTest
    {
        public virtual void ConstructorRejectsNullsWhereNotAllowed()
        {
            var lambda = new EvmHook(new ContractId(0, 0, 123));
            NullReferenceException ex1 = await Assert.Throws<NullReferenceException>(() => new HookCreationDetails(null, 1, lambda));
            Assert.True(ex1.GetMessage().Contains("extensionPoint cannot be null"));
            NullReferenceException ex2 = await Assert.Throws<NullReferenceException>(() => new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, null));
            Assert.True(ex2.GetMessage().Contains("hook cannot be null"));
        }

        public virtual void GettersAndHasAdminKeyWork()
        {
            var cid = new ContractId(0, 0, 77);
            var lambda = new EvmHook(cid);
            var admin = PrivateKey.GenerateED25519().GetPublicKey();
            var withAdmin = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 9, lambda, admin);
            Assert.Equal(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, withAdmin.GetExtensionPoint());
            Assert.Equal(9, withAdmin.GetHookId());
            Assert.Equal(lambda, withAdmin.GetHook());
            Assert.True(withAdmin.HasAdminKey());
            Assert.Equal(admin, withAdmin.GetAdminKey());
            var withoutAdmin = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 10, lambda);
            Assert.False(withoutAdmin.HasAdminKey());
            Assert.Null(withoutAdmin.GetAdminKey());
        }

        public virtual void ProtobufRoundTripPreservesValues()
        {
            var cid = new ContractId(0, 0, 1234);
            var lambda = new EvmHook(cid);
            var details = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 42, lambda, PrivateKey.GenerateED25519().GetPublicKey());
            var proto = details.ToProtobuf();
            var restored = HookCreationDetails.FromProtobuf(proto);
            Assert.Equal(details.GetExtensionPoint(), restored.GetExtensionPoint());
            Assert.Equal(details.GetHookId(), restored.GetHookId());
            Assert.Equal(details.GetHook(), restored.GetHook());
            Assert.Equal(details.GetAdminKey(), restored.GetAdminKey());
            Assert.Equal(details, restored);
            Assert.Equal(details.GetHashCode(), restored.GetHashCode());
        }

        public virtual void EqualsAndHashCodeVaryByFields()
        {
            var lambda1 = new EvmHook(new ContractId(0, 0, 1));
            var lambda2 = new EvmHook(new ContractId(0, 0, 2));
            var a = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambda1);
            var b = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambda1);
            var c = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 2, lambda1);
            var d = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambda2);
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            AssertNotEquals(a, c);
            AssertNotEquals(a, d);
        }

        public virtual void ToStringContainsKeyFields()
        {
            var lambda = new EvmHook(new ContractId(0, 0, 3));
            var details = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 7, lambda);
            var s = details.ToString();
            Assert.True(s.Contains("extensionPoint"));
            Assert.True(s.Contains("hookId"));
            Assert.True(s.Contains("hook"));
        }
    }
}