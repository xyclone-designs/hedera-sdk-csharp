// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class EvmHookMappingEntryTest
    {
        virtual void OfKeyBuildsEntryAndCopiesArrays()
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
            var entry = EvmHookMappingEntry.OfKey(key, value);
            AssertTrue(entry.HasExplicitKey());
            Assert.False(entry.HasPreimageKey());
            AssertArrayEquals(key, entry.GetKey());
            Assert.Null(entry.GetPreimage());
            AssertArrayEquals(value, entry.GetValue());

            // Ensure defensive copies
            key[0] = 0x7F;
            value[0] = 0x7F;
            AssertArrayEquals(new byte[] { 0x01, 0x02 }, entry.GetKey());
            AssertArrayEquals(new byte[] { 0x03, 0x04 }, entry.GetValue());
        }

        virtual void WithPreimageBuildsEntryAndCopiesArrays()
        {
            byte[] preimage = new byte[]
            {
                0x11,
                0x22
            };
            byte[] value = new byte[]
            {
                0x33,
                0x44
            };
            var entry = EvmHookMappingEntry.WithPreimage(preimage, value);
            Assert.False(entry.HasExplicitKey());
            AssertTrue(entry.HasPreimageKey());
            Assert.Null(entry.GetKey());
            AssertArrayEquals(preimage, entry.GetPreimage());
            AssertArrayEquals(value, entry.GetValue());

            // Ensure defensive copies
            preimage[0] = 0x7F;
            value[0] = 0x7F;
            AssertArrayEquals(new byte[] { 0x11, 0x22 }, entry.GetPreimage());
            AssertArrayEquals(new byte[] { 0x33, 0x44 }, entry.GetValue());
        }

        virtual void BuildersRejectNullInputs()
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => EvmHookMappingEntry.OfKey(null, new byte[] { 0x01 }));
            await Assert.ThrowsAsync<NullReferenceException>(() => EvmHookMappingEntry.WithPreimage(null, new byte[] { 0x01 }));
            await Assert.ThrowsAsync<NullReferenceException>(() => EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, null));
            await Assert.ThrowsAsync<NullReferenceException>(() => EvmHookMappingEntry.WithPreimage(new byte[] { 0x01 }, null));
        }

        virtual void ProtobufRoundTripForKeyAndPreimage()
        {
            var keyEntry = EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, new byte[] { 0x02 });
            var keyRoundTrip = EvmHookMappingEntry.FromProtobuf(keyEntry.ToProtobuf());
            AssertEquals(keyEntry, keyRoundTrip);
            var preimageEntry = EvmHookMappingEntry.WithPreimage(new byte[] { 0x0A }, new byte[] { 0x0B });
            var preimageRoundTrip = EvmHookMappingEntry.FromProtobuf(preimageEntry.ToProtobuf());
            AssertEquals(preimageEntry, preimageRoundTrip);
        }

        virtual void FromProtobufWithoutKeyThrows()
        {
            var emptyProto = com.hedera.hashgraph.sdk.proto.EvmHookMappingEntry.NewBuilder().Build();
            await Assert.ThrowsAsync<ArgumentException>(() => EvmHookMappingEntry.FromProtobuf(emptyProto));
        }

        virtual void EqualsHashCodeAndToString()
        {
            var a = EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, new byte[] { 0x02 });
            var b = EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, new byte[] { 0x02 });
            var c = EvmHookMappingEntry.OfKey(new byte[] { 0x03 }, new byte[] { 0x04 });
            AssertEquals(a, b);
            AssertEquals(a.GetHashCode(), b.GetHashCode());
            AssertNotEquals(a, c);
            var s = a.ToString();
            AssertTrue(s.Contains("key") || s.Contains("preimage"));
            AssertTrue(s.Contains("value"));
        }
    }
}