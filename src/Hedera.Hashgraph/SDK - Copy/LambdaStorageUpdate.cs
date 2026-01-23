// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Google.Protobuf;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Abstract base class for lambda storage updates.
    /// <p>
    /// Storage updates define how to modify the storage of a lambda EVM hook.
    /// This can be done either by directly specifying storage slots or by
    /// updating Solidity mapping entries.
    /// </summary>
    public abstract class LambdaStorageUpdate
    {
        /// <summary>
        /// Convert this storage update to a protobuf message.
        /// </summary>
        /// <returns>the protobuf LambdaStorageUpdate</returns>
        public abstract Proto.LambdaStorageUpdate ToProtobuf();
        /// <summary>
        /// Create a LambdaStorageUpdate from a protobuf message.
        /// </summary>
        /// <param name="proto">the protobuf LambdaStorageUpdate</param>
        /// <returns>a new LambdaStorageUpdate instance</returns>
        public static LambdaStorageUpdate FromProtobuf(Proto.LambdaStorageUpdate proto)
        {
            return proto.GetUpdateCase() switch
            {
                StorageSlot => LambdaStorageSlot.FromProtobuf(proto.GetStorageSlot()),
                MappingEntries => LambdaMappingEntries.FromProtobuf(proto.GetMappingEntries()),
                UpdateNotSet => new ArgumentException("LambdaStorageUpdate must have either storage_slot or mapping_entries set")};
        }

        /// <summary>
        /// Represents a direct storage slot update.
        /// <p>
        /// This class allows direct manipulation of storage slots in the lambda's storage.
        /// </summary>
        public class LambdaStorageSlot : LambdaStorageUpdate
        {
            private readonly byte[] key;
            private readonly byte[] value;
            /// <summary>
            /// Create a new storage slot update.
            /// </summary>
            /// <param name="key">the storage slot key (max 32 bytes, minimal representation)</param>
            /// <param name="value">the storage slot value (max 32 bytes, minimal representation)</param>
            public LambdaStorageSlot(byte[] key, byte[] value)
            {
                key = Objects.RequireNonNull(key, "key cannot be null").Clone();
                value = value != null ? value.Clone() : new byte[0];
            }

            /// <summary>
            /// Get the storage slot key.
            /// </summary>
            /// <returns>a copy of the key bytes</returns>
            public virtual byte[] GetKey()
            {
                return key.Clone();
            }

            /// <summary>
            /// Get the storage slot value.
            /// </summary>
            /// <returns>a copy of the value bytes</returns>
            public virtual byte[] GetValue()
            {
                return value.Clone();
            }

            public override Proto.LambdaStorageUpdate ToProtobuf()
            {
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

            public override bool Equals(object? o)
            {
                if (this == o)
                    return true;
                if (o == null || GetType() != o.GetType())
                    return false;
                LambdaStorageSlot that = (LambdaStorageSlot)o;
                return Equals(key, that.key) && Equals(value, that.value);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(HashCode.Combine(key), HashCode.Combine(value));
            }
        }

        /// <summary>
        /// Represents storage updates via Solidity mapping entries.
        /// <p>
        /// This class allows updates to be specified in terms of Solidity mapping
        /// entries rather than raw storage slots, making it easier to work with
        /// high-level data structures.
        /// </summary>
        public class LambdaMappingEntries : LambdaStorageUpdate
        {
            private readonly byte[] mappingSlot;
            private readonly IList<LambdaMappingEntry> entries;
            /// <summary>
            /// Create a new mapping entries update.
            /// </summary>
            /// <param name="mappingSlot">the slot that corresponds to the Solidity mapping (minimal representation)</param>
            /// <param name="entries">the entries to update in the mapping</param>
            public LambdaMappingEntries(byte[] mappingSlot, IEnumerable<LambdaMappingEntry> entries)
            {
                mappingSlot = mappingSlot.CopyArray();
                entries = [.. entries];
            }

            /// <summary>
            /// Get the mapping slot.
            /// </summary>
            /// <returns>a copy of the mapping slot bytes</returns>
            public virtual byte[] GetMappingSlot()
            {
                return mappingSlot.Clone();
            }

            /// <summary>
            /// Get the mapping entries.
            /// </summary>
            /// <returns>a copy of the entries list</returns>
            public virtual List<LambdaMappingEntry> GetEntries()
            {
                return new List(entries);
            }

			public override Proto.LambdaStorageUpdate ToProtobuf()
            {
                var builder = Proto.LambdaMappingEntries.NewBuilder().SetMappingSlot(ByteString.CopyFrom(mappingSlot));
                foreach (LambdaMappingEntry entry in entries)
                {
                    builder.AddEntries(entry.ToProtobuf());
                }

                return Proto.LambdaStorageUpdate.NewBuilder().SetMappingEntries(builder.Build()).Build();
            }

            public static LambdaMappingEntries FromProtobuf(Proto.LambdaMappingEntries proto)
            {
                var entries = new List<LambdaMappingEntry>();
                foreach (var protoEntry in proto.Entries)
                {
                    entries.Add(LambdaMappingEntry.FromProtobuf(protoEntry));
                }

                return new LambdaMappingEntries(proto.MappingSlot.ToByteArray(), entries);
            }

            public override bool Equals(object? o)
            {
                if (this == o)
                    return true;
                if (o == null || GetType() != o.GetType())
                    return false;

                LambdaMappingEntries that = (LambdaMappingEntries)o;

                return Equals(mappingSlot, that.mappingSlot) && entries.Equals(that.entries);
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(HashCode.Combine(mappingSlot), entries);
            }
        }
    }
}