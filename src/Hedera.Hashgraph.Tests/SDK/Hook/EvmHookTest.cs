// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using Hedera.Hashgraph.SDK.Contract;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class EvmHookTest
    {
        public virtual void GettersReturnExpectedAndStorageUpdatesAreImmutable()
        {
            var contractId = new ContractId(0, 0, 123);
            var slot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
            var hook = new EvmHook(contractId, [slot]);
            Assert.Equal(contractId, hook.GetContractId());
            var updates = hook.GetStorageUpdates();
            Assert.Equal(1, updates.Count);
            Assert.Equal(slot, updates[0]);

            // list must be unmodifiable
            Assert.Throws<NotSupportedException>(() => updates.Add(slot));
        }

        public virtual void ProtobufRoundTripPreservesData()
        {
            var spec = new ContractId(0, 0, 77);
            var entry = EvmHookMappingEntry.OfKey(new byte[] { 0x0A }, new byte[] { 0x0B });
            var mappings = new EvmHookMappingEntries(new byte[] { 0x05 }, [entry]);
            var slot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
            var original = new EvmHook(spec, [slot, mappings]);
            var proto = original.ToProtobuf();
            var restored = EvmHook.FromProtobuf(proto);
            Assert.Equal(original, restored);
            Assert.Equal(original.GetContractId(), restored.GetContractId());
            Assert.Equal(original.GetStorageUpdates(), restored.GetStorageUpdates());
        }

        public virtual void EqualsAndHashCodeDependOnSpecAndUpdates()
        {
            var spec1 = new ContractId(0, 0, 1);
            var spec2 = new ContractId(0, 0, 2);
            IList<EvmHookStorageUpdate> u1 = [ new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 }) ];
            IList<EvmHookStorageUpdate> u2 = [ new EvmHookStorageSlot(new byte[] { 0x03 }, new byte[] { 0x04 }) ];
            var a = new EvmHook(spec1, u1);
            var b = new EvmHook(spec1, new List<EvmHookStorageUpdate>(u1));
            var c = new EvmHook(spec2, u1);
            var d = new EvmHook(spec1, u2);
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.NotEqual(a, c);
            Assert.NotEqual(a, d);
        }

        public virtual void ToStringContainsSpecAndUpdates()
        {
            var spec = new ContractId(0, 0, 10);
            var hook = new EvmHook(spec);
            var s = hook.ToString();
            Assert.True(s.Contains("contractId"));
            Assert.True(s.Contains("storageUpdates"));
        }
    }
}