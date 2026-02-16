// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.SDK.Hook
{
    class EvmHookMappingEntryTest
    {
        public virtual void OfKeyBuildsEntryAndCopiesArrays()
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
            Assert.True(entry.HasExplicitKey());
            Assert.False(entry.HasPreimageKey());
            Assert.Equal(key, entry.GetKey());
            Assert.Null(entry.GetPreimage());
            Assert.Equal(value, entry.GetValue());

            // Ensure defensive copies
            key[0] = 0x7F;
            value[0] = 0x7F;
            Assert.Equal(new byte[] { 0x01, 0x02 }, entry.GetKey());
            Assert.Equal(new byte[] { 0x03, 0x04 }, entry.GetValue());
        }

        public virtual void WithPreimageBuildsEntryAndCopiesArrays()
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
            Assert.True(entry.HasPreimageKey());
            Assert.Null(entry.GetKey());
            Assert.Equal(preimage, entry.GetPreimage());
            Assert.Equal(value, entry.GetValue());

            // Ensure defensive copies
            preimage[0] = 0x7F;
            value[0] = 0x7F;
            Assert.Equal(new byte[] { 0x11, 0x22 }, entry.GetPreimage());
            Assert.Equal(new byte[] { 0x33, 0x44 }, entry.GetValue());
        }

        public virtual void BuildersRejectNullInputs()
        {
            Assert.Throws<NullReferenceException>(() => EvmHookMappingEntry.OfKey(null, new byte[] { 0x01 }));
            Assert.Throws<NullReferenceException>(() => EvmHookMappingEntry.WithPreimage(null, new byte[] { 0x01 }));
            Assert.Throws<NullReferenceException>(() => EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, null));
            Assert.Throws<NullReferenceException>(() => EvmHookMappingEntry.WithPreimage(new byte[] { 0x01 }, null));
        }

        public virtual void ProtobufRoundTripForKeyAndPreimage()
        {
            var keyEntry = EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, new byte[] { 0x02 });
            var keyRoundTrip = EvmHookMappingEntry.FromProtobuf(keyEntry.ToProtobuf());
            Assert.Equal(keyEntry, keyRoundTrip);
            var preimageEntry = EvmHookMappingEntry.WithPreimage(new byte[] { 0x0A }, new byte[] { 0x0B });
            var preimageRoundTrip = EvmHookMappingEntry.FromProtobuf(preimageEntry.ToProtobuf());
            Assert.Equal(preimageEntry, preimageRoundTrip);
        }

        public virtual void FromProtobufWithoutKeyThrows()
        {
            var emptyProto = Proto.EvmHookMappingEntry.NewBuilder().Build();
            Assert.Throws<ArgumentException>(() => EvmHookMappingEntry.FromProtobuf(emptyProto));
        }

        public virtual void EqualsHashCodeAndToString()
        {
            var a = EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, new byte[] { 0x02 });
            var b = EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, new byte[] { 0x02 });
            var c = EvmHookMappingEntry.OfKey(new byte[] { 0x03 }, new byte[] { 0x04 });
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            AssertNotEquals(a, c);
            var s = a.ToString();
            Assert.True(s.Contains("key") || s.Contains("preimage"));
            Assert.True(s.Contains("value"));
        }
    }
}