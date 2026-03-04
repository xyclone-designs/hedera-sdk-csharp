// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class EvmHookStorageUpdateTest
    {
        public virtual void EvmHookStorageSlotConstructsAndDefensiveCopies()
        {
            byte[] key = new byte[]
            {
                0x01,
                0x02
            };
            byte[] value = new byte[]
            {
                0x03,
                0x04
            };
            var slot = new EvmHookStorageSlot(key, value);
            Assert.Equal(key, slot.Key);
            Assert.Equal(value, slot.Value);

            // Defensive copies on construction/getters
            key[0] = 0x7F;
            value[0] = 0x7F;
            Assert.Equal(new byte[] { 0x01, 0x02 }, slot.Key);
            Assert.Equal(new byte[] { 0x03, 0x04 }, slot.Value);
        }

        public virtual void EvmHookStorageSlotProtobufRoundTrip()
        {
            var original = new EvmHookStorageSlot(new byte[] { 0x0A }, new byte[] { 0x0B });
            var proto = original.ToProtobuf();
            var restored = EvmHookStorageUpdate.FromProtobuf(proto);
            Assert.Equal(original, restored);
            Assert.True(restored is EvmHookStorageSlot);
            var restoredSlot = (EvmHookStorageSlot)restored;
            Assert.Equal(new byte[] { 0x0A }, restoredSlot.Key);
            Assert.Equal(new byte[] { 0x0B }, restoredSlot.Value);
            Assert.True(original.ToString().Contains("key"));
        }

        public virtual void EvmHookMappingEntriesConstructsValidatesAndCopies()
        {
            byte[] mappingSlot = new byte[]
            {
                0x05
            };
            var entry = EvmHookMappingEntry.OfKey(new byte[] { 0x10 }, new byte[] { 0x20 });
            var updates = new EvmHookMappingEntries(mappingSlot, [entry]);
            Assert.Equal(mappingSlot, updates.MappingSlot);
            Assert.Equal(1, updates.Entries.Count);
            Assert.Equal(entry, updates.Entries[0]);

            // Defensive copy of mappingSlot
            mappingSlot[0] = 0x7F;
            Assert.Equal(new byte[] { 0x05 }, updates.MappingSlot);

            // Returned entries list is a copy
            var list1 = updates.Entries;
            var list2 = updates.Entries;
            Assert.NotSame(list1, list2);
            Assert.Equal(list1, list2);
        }

        public virtual void EvmHookMappingEntriesValidation()
        {
            // mappingSlot cannot be null
            Assert.Throws<NullReferenceException>(() => new EvmHookMappingEntries(null, []));

            // entries cannot be null
            Assert.Throws<NullReferenceException>(() => new EvmHookMappingEntries(new byte[] { 0x01 }, null));

            // current behavior: length > 32 is allowed
            new EvmHookMappingEntries(new byte[33], []);

            // current behavior: leading zeros are allowed
            new EvmHookMappingEntries(new byte[] { 0x00, 0x01 }, []);
        }

        public virtual void EvmHookMappingEntriesProtobufRoundTrip()
        {
            var entry1 = EvmHookMappingEntry.OfKey(new byte[] { 0x11 }, new byte[] { 0x22 });
            var entry2 = EvmHookMappingEntry.WithPreimage(new byte[] { 0x33 }, new byte[] { 0x44 });
            var original = new EvmHookMappingEntries(new byte[] { 0x09 }, [entry1, entry2]);
            var proto = original.ToProtobuf();
            var restored = EvmHookStorageUpdate.FromProtobuf(proto);
            Assert.Equal(original, restored);
            Assert.True(restored is EvmHookMappingEntries);
            var restoredME = (EvmHookMappingEntries)restored;
            Assert.Equal(new byte[] { 0x09 }, restoredME.MappingSlot);
            Assert.Equal([entry1, entry2], restoredME.Entries);
            Assert.True(original.ToString().Contains("mappingSlot"));
        }

        public virtual void FromProtobufWithoutUpdateThrows()
        {
            var emptyProto = new Proto.EvmHookStorageUpdate();

            Assert.Throws<ArgumentException>(() => EvmHookStorageUpdate.FromProtobuf(emptyProto));
        }
    }
}