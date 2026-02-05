// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class EvmHookTest
    {
        virtual void ConstructorRejectsNulls()
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => new EvmHook((ContractId)null));
            await Assert.ThrowsAsync<NullReferenceException>(() => new EvmHook(new ContractId(0, 0, 1), null));
        }

        virtual void GettersReturnExpectedAndStorageUpdatesAreImmutable()
        {
            var contractId = new ContractId(0, 0, 123);
            var slot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
            var hook = new EvmHook(contractId, List.Of(slot));
            AssertEquals(contractId, hook.GetContractId());
            var updates = hook.GetStorageUpdates();
            AssertEquals(1, updates.Count);
            AssertEquals(slot, updates[0]);

            // list must be unmodifiable
            await Assert.ThrowsAsync<NotSupportedException>(() => updates.Add(slot));
        }

        virtual void ProtobufRoundTripPreservesData()
        {
            var spec = new ContractId(0, 0, 77);
            var entry = EvmHookMappingEntry.OfKey(new byte[] { 0x0A }, new byte[] { 0x0B });
            var mappings = new EvmHookMappingEntries(new byte[] { 0x05 }, List.Of(entry));
            var slot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
            var original = new EvmHook(spec, List.Of(slot, mappings));
            var proto = original.ToProtobuf();
            var restored = EvmHook.FromProtobuf(proto);
            AssertEquals(original, restored);
            AssertEquals(original.GetContractId(), restored.GetContractId());
            AssertEquals(original.GetStorageUpdates(), restored.GetStorageUpdates());
        }

        virtual void EqualsAndHashCodeDependOnSpecAndUpdates()
        {
            var spec1 = new ContractId(0, 0, 1);
            var spec2 = new ContractId(0, 0, 2);
            IList<EvmHookStorageUpdate> u1 = List.Of(new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 }));
            IList<EvmHookStorageUpdate> u2 = List.Of(new EvmHookStorageSlot(new byte[] { 0x03 }, new byte[] { 0x04 }));
            var a = new EvmHook(spec1, u1);
            var b = new EvmHook(spec1, new List<EvmHookStorageUpdate>(u1));
            var c = new EvmHook(spec2, u1);
            var d = new EvmHook(spec1, u2);
            AssertEquals(a, b);
            AssertEquals(a.GetHashCode(), b.GetHashCode());
            AssertNotEquals(a, c);
            AssertNotEquals(a, d);
        }

        virtual void ToStringContainsSpecAndUpdates()
        {
            var spec = new ContractId(0, 0, 10);
            var hook = new EvmHook(spec);
            var s = hook.ToString();
            AssertTrue(s.Contains("contractId"));
            AssertTrue(s.Contains("storageUpdates"));
        }
    }
}