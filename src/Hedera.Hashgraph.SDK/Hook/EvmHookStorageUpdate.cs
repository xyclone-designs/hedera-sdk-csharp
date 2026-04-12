// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using System;

namespace Hedera.Hashgraph.SDK.Hook
{
    public abstract class EvmHookStorageUpdate
    {
        public abstract Proto.Services.EvmHookStorageUpdate ToProtobuf();
        public static EvmHookStorageUpdate FromProtobuf(Proto.Services.EvmHookStorageUpdate proto)
        {
            return Proto.Services.UpdateCase switch
            {
                Proto.Services.EvmHookStorageUpdate.UpdateOneofCase.StorageSlot => EvmHookStorageSlot.FromProtobuf(Proto.Services.StorageSlot),
				Proto.Services.EvmHookStorageUpdate.UpdateOneofCase.MappingEntries => EvmHookMappingEntries.FromProtobuf(Proto.Services.MappingEntries),
                Proto.Services.EvmHookStorageUpdate.UpdateOneofCase.None or _ => throw new ArgumentException("EvmHookStorageUpdate must have either storage_slot or mapping_entries set")};
        }
    }
}
