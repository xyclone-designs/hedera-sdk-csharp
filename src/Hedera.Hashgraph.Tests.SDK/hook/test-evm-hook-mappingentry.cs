// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Hook;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    /// <include file="test-evm-hook-mappingentry.cs.xml" path="docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Hook.EvmHookMappingEntryTest"]" />
    public class EvmHookMappingEntryTest
    {
        [Fact]
        /// <include file="test-evm-hook-mappingentry.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Hook.EvmHookMappingEntryTest.OfKeyBuildsEntryAndCopiesArrays"]" />
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
            Assert.True(entry.HasExplicitKey);
            Assert.False(entry.HasPreimageKey);
            Assert.Equal(key, entry.Key);
            Assert.Null(entry.PreImage);
            Assert.Equal(value, entry.Value);

            // Ensure defensive copies
            key[0] = 0x7F;
            value[0] = 0x7F;
            Assert.Equal(new byte[] { 0x01, 0x02 }, entry.Key);
            Assert.Equal(new byte[] { 0x03, 0x04 }, entry.Value);
        }
        [Fact]
        /// <include file="test-evm-hook-mappingentry.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Hook.EvmHookMappingEntryTest.WithPreimageBuildsEntryAndCopiesArrays"]" />
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
            Assert.False(entry.HasExplicitKey);
            Assert.True(entry.HasPreimageKey);
            Assert.Null(entry.Key);
            Assert.Equal(preimage, entry.PreImage);
            Assert.Equal(value, entry.Value);

            // Ensure defensive copies
            preimage[0] = 0x7F;
            value[0] = 0x7F;
            Assert.Equal(new byte[] { 0x11, 0x22 }, entry.PreImage);
            Assert.Equal(new byte[] { 0x33, 0x44 }, entry.Value);
        }
        [Fact]
        /// <include file="test-evm-hook-mappingentry.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Hook.EvmHookMappingEntryTest.BuildersRejectNullInputs"]" />
        public virtual void BuildersRejectNullInputs()
        {
            Assert.Throws<NullReferenceException>(() => EvmHookMappingEntry.OfKey(null, new byte[] { 0x01 }));
            Assert.Throws<NullReferenceException>(() => EvmHookMappingEntry.WithPreimage(null, new byte[] { 0x01 }));
            Assert.Throws<NullReferenceException>(() => EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, null));
            Assert.Throws<NullReferenceException>(() => EvmHookMappingEntry.WithPreimage(new byte[] { 0x01 }, null));
        }
        [Fact]
        /// <include file="test-evm-hook-mappingentry.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Hook.EvmHookMappingEntryTest.ProtobufRoundTripForKeyAndPreimage"]" />
        public virtual void ProtobufRoundTripForKeyAndPreimage()
        {
            var keyEntry = EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, new byte[] { 0x02 });
            var keyRoundTrip = EvmHookMappingEntry.FromProtobuf(keyEntry.ToProtobuf());
            Assert.Equal(keyEntry, keyRoundTrip);
            var preimageEntry = EvmHookMappingEntry.WithPreimage(new byte[] { 0x0A }, new byte[] { 0x0B });
            var preimageRoundTrip = EvmHookMappingEntry.FromProtobuf(preimageEntry.ToProtobuf());
            Assert.Equal(preimageEntry, preimageRoundTrip);
        }
        [Fact]
        /// <include file="test-evm-hook-mappingentry.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Hook.EvmHookMappingEntryTest.FromProtobufWithoutKeyThrows"]" />
        public virtual void FromProtobufWithoutKeyThrows()
        {
            var emptyProto = new Proto.Services.EvmHookMappingEntry();

            Assert.Throws<ArgumentException>(() => EvmHookMappingEntry.FromProtobuf(emptyProto));
        }
        [Fact]
        /// <include file="test-evm-hook-mappingentry.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Hook.EvmHookMappingEntryTest.EqualsHashCodeAndToString"]" />
        public virtual void EqualsHashCodeAndToString()
        {
            var a = EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, new byte[] { 0x02 });
            var b = EvmHookMappingEntry.OfKey(new byte[] { 0x01 }, new byte[] { 0x02 });
            var c = EvmHookMappingEntry.OfKey(new byte[] { 0x03 }, new byte[] { 0x04 });
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.NotEqual(a, c);
            var s = a.ToString();
            Assert.True(s.Contains("key") || s.Contains("preimage"));
            Assert.True(s.Contains("value"));
        }
    }
}
