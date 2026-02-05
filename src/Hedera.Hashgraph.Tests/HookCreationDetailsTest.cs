// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class HookCreationDetailsTest
    {
        virtual void ConstructorRejectsNullsWhereNotAllowed()
        {
            var lambda = new EvmHook(new ContractId(0, 0, 123));
            NullReferenceException ex1 = await Assert.Throws<NullReferenceException>(() => new HookCreationDetails(null, 1, lambda));
            AssertTrue(ex1.GetMessage().Contains("extensionPoint cannot be null"));
            NullReferenceException ex2 = await Assert.Throws<NullReferenceException>(() => new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, null));
            AssertTrue(ex2.GetMessage().Contains("hook cannot be null"));
        }

        virtual void GettersAndHasAdminKeyWork()
        {
            var cid = new ContractId(0, 0, 77);
            var lambda = new EvmHook(cid);
            var admin = PrivateKey.GenerateED25519().GetPublicKey();
            var withAdmin = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 9, lambda, admin);
            AssertEquals(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, withAdmin.GetExtensionPoint());
            AssertEquals(9, withAdmin.GetHookId());
            AssertEquals(lambda, withAdmin.GetHook());
            AssertTrue(withAdmin.HasAdminKey());
            AssertEquals(admin, withAdmin.GetAdminKey());
            var withoutAdmin = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 10, lambda);
            Assert.False(withoutAdmin.HasAdminKey());
            Assert.Null(withoutAdmin.GetAdminKey());
        }

        virtual void ProtobufRoundTripPreservesValues()
        {
            var cid = new ContractId(0, 0, 1234);
            var lambda = new EvmHook(cid);
            var details = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 42, lambda, PrivateKey.GenerateED25519().GetPublicKey());
            var proto = details.ToProtobuf();
            var restored = HookCreationDetails.FromProtobuf(proto);
            AssertEquals(details.GetExtensionPoint(), restored.GetExtensionPoint());
            AssertEquals(details.GetHookId(), restored.GetHookId());
            AssertEquals(details.GetHook(), restored.GetHook());
            AssertEquals(details.GetAdminKey(), restored.GetAdminKey());
            AssertEquals(details, restored);
            AssertEquals(details.GetHashCode(), restored.GetHashCode());
        }

        virtual void EqualsAndHashCodeVaryByFields()
        {
            var lambda1 = new EvmHook(new ContractId(0, 0, 1));
            var lambda2 = new EvmHook(new ContractId(0, 0, 2));
            var a = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambda1);
            var b = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambda1);
            var c = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 2, lambda1);
            var d = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambda2);
            AssertEquals(a, b);
            AssertEquals(a.GetHashCode(), b.GetHashCode());
            AssertNotEquals(a, c);
            AssertNotEquals(a, d);
        }

        virtual void ToStringContainsKeyFields()
        {
            var lambda = new EvmHook(new ContractId(0, 0, 3));
            var details = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 7, lambda);
            var s = details.ToString();
            AssertTrue(s.Contains("extensionPoint"));
            AssertTrue(s.Contains("hookId"));
            AssertTrue(s.Contains("hook"));
        }
    }
}