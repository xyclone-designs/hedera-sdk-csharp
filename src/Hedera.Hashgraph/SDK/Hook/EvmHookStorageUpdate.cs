// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using System;

namespace Hedera.Hashgraph.SDK.Hook
{
    public abstract class EvmHookStorageUpdate
    {
        public abstract Proto.EvmHookStorageUpdate ToProtobuf();
        public static EvmHookStorageUpdate FromProtobuf(Proto.EvmHookStorageUpdate proto)
        {
            return proto.UpdateCase switch
            {
                STORAGE_SLOT => EvmHookStorageSlot.FromProtobuf(proto.StorageSlot),
                MAPPING_ENTRIES => EvmHookMappingEntries.FromProtobuf(proto.MappingEntries),
                UPDATE_NOT_SET => new ArgumentException("EvmHookStorageUpdate must have either storage_slot or mapping_entries set")};
        }
    }
}