using Google.Protobuf;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Abstract base class for lambda storage updates.
     * <p>
     * Storage updates define how to modify the storage of a lambda EVM hook.
     * This can be done either by directly specifying storage slots or by
     * updating Solidity mapping entries.
     */
    public abstract class LambdaStorageUpdate 
    {
        /**
         * Convert this storage update to a protobuf message.
         *
         * @return the protobuf LambdaStorageUpdate
         */
        public abstract Proto.LambdaStorageUpdate ToProtobuf();

        /**
         * Create a LambdaStorageUpdate from a protobuf message.
         *
         * @param proto the protobuf LambdaStorageUpdate
         * @return a new LambdaStorageUpdate instance
         */
        public static LambdaStorageUpdate FromProtobuf(Proto.LambdaStorageUpdate proto) 
        {
            return proto.UpdateCase switch
            {
                Proto.LambdaStorageUpdate.UpdateOneofCase.StorageSlot => LambdaStorageSlot.FromProtobuf(proto.StorageSlot),
                Proto.LambdaStorageUpdate.UpdateOneofCase.MappingEntries => LambdaMappingEntries.FromProtobuf(proto.MappingEntries),

				_ => throw new ArgumentException("LambdaStorageUpdate must have either storage_slot or mapping_entries set")
            };
        }

        /**
         * Represents a direct storage slot update.
         * <p>
         * This class allows direct manipulation of storage slots in the lambda's storage.
         */
        public class LambdaStorageSlot : LambdaStorageUpdate
        {
            private readonly byte[] key;
            private readonly byte[] value;

            /**
             * Create a new storage slot update.
             *
             * @param key the storage slot key (max 32 bytes, minimal representation)
             * @param value the storage slot value (max 32 bytes, minimal representation)
             */
            public LambdaStorageSlot(byte[] key, byte[]? value)
            {
                this.key = (byte[])key.Clone();
                this.value = (byte[]?)value?.Clone() ?? [];
            }

            public byte[] Key { get => (byte[])key.Clone(); }
			public byte[] Value { get => (byte[])value.Clone(); }

			public override Proto.LambdaStorageUpdate ToProtobuf() {
                return new Proto.LambdaStorageUpdate
                {
                    StorageSlot = new Proto.LambdaStorageSlot
					{
						Key = ByteString.CopyFrom(key),
						Value = ByteString.CopyFrom(value),
					}
				};
            }

            public static LambdaStorageSlot FromProtobuf(Proto.LambdaStorageSlot proto) 
            {
                return new LambdaStorageSlot(proto.Key.ToByteArray(), proto.Value.ToByteArray());
            }

			public override int GetHashCode()
			{
				return HashCode.Combine(key, value);
			}
			public override bool Equals(object? obj) 
            {
                if (this == obj) return true;
                if (obj == null || GetType() != obj.GetType()) return false;

                LambdaStorageSlot that = (LambdaStorageSlot) obj;

                return Equals(key, that.key) && Equals(value, that.value);
            }
        }

        /**
         * Represents storage updates via Solidity mapping entries.
         * <p>
         * This class allows updates to be specified in terms of Solidity mapping
         * entries rather than raw storage slots, making it easier to work with
         * high-level data structures.
         */
        public class LambdaMappingEntries : LambdaStorageUpdate 
        {
            /**
             * Create a new mapping entries update.
             *
             * @param mappingSlot the slot that corresponds to the Solidity mapping (minimal representation)
             * @param entries the entries to update in the mapping
             */
            public LambdaMappingEntries(byte[] mappingSlot, List<LambdaMappingEntry> entries) 
            {
				Entries = entries;
				MappingSlot = (byte[])mappingSlot.Clone();
            }
			public static LambdaMappingEntries FromProtobuf(Proto.LambdaMappingEntries proto)
			{
				return new LambdaMappingEntries(
                    proto.MappingSlot.ToByteArray(),
					proto.Entries.Select(_ => LambdaMappingEntry.FromProtobuf(_)).ToList());
			}

			public byte[] MappingSlot { get; }
			public List<LambdaMappingEntry> Entries { get; }

            public override int GetHashCode() 
            {
                return HashCode.Combine(MappingSlot, Entries);
            }
			public override bool Equals(object? obj)
			{
				if (this == obj) return true;
				if (obj == null || GetType() != obj.GetType()) return false;

				LambdaMappingEntries that = (LambdaMappingEntries)obj;

				return Equals(MappingSlot, that.MappingSlot) && Entries.Equals(that.Entries);
			}
			public override Proto.LambdaStorageUpdate ToProtobuf()
			{
                Proto.LambdaMappingEntries protobuf = new()
                {
                    MappingSlot = ByteString.CopyFrom(MappingSlot),
				};

                protobuf.Entries.AddRange(Entries.Select(_ => _.ToProtobuf()));

                return new Proto.LambdaStorageUpdate
                { 
                    MappingEntries = protobuf
                };
			}
		}
    }
}