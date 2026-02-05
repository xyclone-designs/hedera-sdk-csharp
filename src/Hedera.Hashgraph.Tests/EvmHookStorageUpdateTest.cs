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
    class EvmHookStorageUpdateTest
    {
        virtual void LambdaStorageSlotConstructsAndDefensiveCopies()
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
            AssertArrayEquals(key, slot.GetKey());
            AssertArrayEquals(value, slot.GetValue());

            // Defensive copies on construction/getters
            key[0] = 0x7F;
            value[0] = 0x7F;
            AssertArrayEquals(new byte[] { 0x01, 0x02 }, slot.GetKey());
            AssertArrayEquals(new byte[] { 0x03, 0x04 }, slot.GetValue());
        }

        virtual void LambdaStorageSlotProtobufRoundTrip()
        {
            var original = new EvmHookStorageSlot(new byte[] { 0x0A }, new byte[] { 0x0B });
            var proto = original.ToProtobuf();
            var restored = EvmHookStorageUpdate.FromProtobuf(proto);
            AssertEquals(original, restored);
            AssertTrue(restored is EvmHookStorageUpdate.EvmHookStorageSlot);
            var restoredSlot = (EvmHookStorageUpdate.EvmHookStorageSlot)restored;
            AssertArrayEquals(new byte[] { 0x0A }, restoredSlot.GetKey());
            AssertArrayEquals(new byte[] { 0x0B }, restoredSlot.GetValue());
            AssertTrue(original.ToString().Contains("key"));
        }

        virtual void LambdaMappingEntriesConstructsValidatesAndCopies()
        {
            byte[] mappingSlot = new byte[]
            {
                0x05
            };
            var entry = EvmHookMappingEntry.OfKey(new byte[] { 0x10 }, new byte[] { 0x20 });
            var updates = new EvmHookMappingEntries(mappingSlot, List.Of(entry));
            AssertArrayEquals(mappingSlot, updates.GetMappingSlot());
            AssertEquals(1, updates.GetEntries().Count);
            AssertEquals(entry, updates.GetEntries()[0]);

            // Defensive copy of mappingSlot
            mappingSlot[0] = 0x7F;
            AssertArrayEquals(new byte[] { 0x05 }, updates.GetMappingSlot());

            // Returned entries list is a copy
            var list1 = updates.GetEntries();
            var list2 = updates.GetEntries();
            AssertNotSame(list1, list2);
            AssertEquals(list1, list2);
        }

        virtual void LambdaMappingEntriesValidation()
        {

            // mappingSlot cannot be null
            await Assert.ThrowsAsync<NullReferenceException>(() => new EvmHookMappingEntries(null, List.Of()));

            // entries cannot be null
            await Assert.ThrowsAsync<NullReferenceException>(() => new EvmHookMappingEntries(new byte[] { 0x01 }, null));

            // current behavior: length > 32 is allowed
            AssertDoesNotThrow(() => new EvmHookMappingEntries(new byte[33], List.Of()));

            // current behavior: leading zeros are allowed
            AssertDoesNotThrow(() => new EvmHookMappingEntries(new byte[] { 0x00, 0x01 }, List.Of()));
        }

        virtual void LambdaMappingEntriesProtobufRoundTrip()
        {
            var entry1 = EvmHookMappingEntry.OfKey(new byte[] { 0x11 }, new byte[] { 0x22 });
            var entry2 = EvmHookMappingEntry.WithPreimage(new byte[] { 0x33 }, new byte[] { 0x44 });
            var original = new EvmHookMappingEntries(new byte[] { 0x09 }, List.Of(entry1, entry2));
            var proto = original.ToProtobuf();
            var restored = EvmHookStorageUpdate.FromProtobuf(proto);
            AssertEquals(original, restored);
            AssertTrue(restored is EvmHookStorageUpdate.EvmHookMappingEntries);
            var restoredME = (EvmHookStorageUpdate.EvmHookMappingEntries)restored;
            AssertArrayEquals(new byte[] { 0x09 }, restoredME.GetMappingSlot());
            AssertEquals(List.Of(entry1, entry2), restoredME.GetEntries());
            AssertTrue(original.ToString().Contains("mappingSlot"));
        }

        virtual void FromProtobufWithoutUpdateThrows()
        {
            var emptyProto = com.hedera.hashgraph.sdk.proto.EvmHookStorageUpdate.NewBuilder().Build();
            await Assert.ThrowsAsync<ArgumentException>(() => EvmHookStorageUpdate.FromProtobuf(emptyProto));
        }
    }
}