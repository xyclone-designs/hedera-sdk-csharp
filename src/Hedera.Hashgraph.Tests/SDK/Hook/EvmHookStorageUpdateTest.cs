// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class EvmHookStorageUpdateTest
    {
        public virtual void LambdaStorageSlotConstructsAndDefensiveCopies()
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
            Assert.Equal(key, slot.GetKey());
            Assert.Equal(value, slot.GetValue());

            // Defensive copies on construction/getters
            key[0] = 0x7F;
            value[0] = 0x7F;
            Assert.Equal(new byte[] { 0x01, 0x02 }, slot.GetKey());
            Assert.Equal(new byte[] { 0x03, 0x04 }, slot.GetValue());
        }

        public virtual void LambdaStorageSlotProtobufRoundTrip()
        {
            var original = new EvmHookStorageSlot(new byte[] { 0x0A }, new byte[] { 0x0B });
            var proto = original.ToProtobuf();
            var restored = EvmHookStorageUpdate.FromProtobuf(proto);
            Assert.Equal(original, restored);
            Assert.True(restored is EvmHookStorageUpdate.EvmHookStorageSlot);
            var restoredSlot = (EvmHookStorageUpdate.EvmHookStorageSlot)restored;
            Assert.Equal(new byte[] { 0x0A }, restoredSlot.GetKey());
            Assert.Equal(new byte[] { 0x0B }, restoredSlot.GetValue());
            Assert.True(original.ToString().Contains("key"));
        }

        public virtual void LambdaMappingEntriesConstructsValidatesAndCopies()
        {
            byte[] mappingSlot = new byte[]
            {
                0x05
            };
            var entry = EvmHookMappingEntry.OfKey(new byte[] { 0x10 }, new byte[] { 0x20 });
            var updates = new EvmHookMappingEntries(mappingSlot, List.Of(entry));
            Assert.Equal(mappingSlot, updates.GetMappingSlot());
            Assert.Equal(1, updates.GetEntries().Count);
            Assert.Equal(entry, updates.GetEntries()[0]);

            // Defensive copy of mappingSlot
            mappingSlot[0] = 0x7F;
            Assert.Equal(new byte[] { 0x05 }, updates.GetMappingSlot());

            // Returned entries list is a copy
            var list1 = updates.GetEntries();
            var list2 = updates.GetEntries();
            AssertNotSame(list1, list2);
            Assert.Equal(list1, list2);
        }

        public virtual void LambdaMappingEntriesValidation()
        {

            // mappingSlot cannot be null
            Assert.Throws<NullReferenceException>(() => new EvmHookMappingEntries(null, List.Of()));

            // entries cannot be null
            Assert.Throws<NullReferenceException>(() => new EvmHookMappingEntries(new byte[] { 0x01 }, null));

            // current behavior: length > 32 is allowed
            AssertDoesNotThrow(() => new EvmHookMappingEntries(new byte[33], List.Of()));

            // current behavior: leading zeros are allowed
            AssertDoesNotThrow(() => new EvmHookMappingEntries(new byte[] { 0x00, 0x01 }, List.Of()));
        }

        public virtual void LambdaMappingEntriesProtobufRoundTrip()
        {
            var entry1 = EvmHookMappingEntry.OfKey(new byte[] { 0x11 }, new byte[] { 0x22 });
            var entry2 = EvmHookMappingEntry.WithPreimage(new byte[] { 0x33 }, new byte[] { 0x44 });
            var original = new EvmHookMappingEntries(new byte[] { 0x09 }, List.Of(entry1, entry2));
            var proto = original.ToProtobuf();
            var restored = EvmHookStorageUpdate.FromProtobuf(proto);
            Assert.Equal(original, restored);
            Assert.True(restored is EvmHookStorageUpdate.EvmHookMappingEntries);
            var restoredME = (EvmHookStorageUpdate.EvmHookMappingEntries)restored;
            Assert.Equal(new byte[] { 0x09 }, restoredME.GetMappingSlot());
            Assert.Equal(List.Of(entry1, entry2), restoredME.GetEntries());
            Assert.True(original.ToString().Contains("mappingSlot"));
        }

        public virtual void FromProtobufWithoutUpdateThrows()
        {
            var emptyProto = Proto.EvmHookStorageUpdate.NewBuilder().Build();
            Assert.Throws<ArgumentException>(() => EvmHookStorageUpdate.FromProtobuf(emptyProto));
        }
    }
}