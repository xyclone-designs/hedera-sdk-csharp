// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK.Hook
{
    public class EvmHookStorageSlot : EvmHookStorageUpdate
    {
        public EvmHookStorageSlot(byte[] key, byte[]? value)
        {
            Key = key.CopyArray();
            Value = value?.CopyArray() ?? [];
        }

		public static EvmHookStorageSlot FromProtobuf(Proto.Services.EvmHookStorageSlot proto)
		{
			return new EvmHookStorageSlot(Proto.Services.Key.ToByteArray(), Proto.Services.Value.ToByteArray());
		}

		public virtual byte[] Key
        {
            get => field.CopyArray();
        }
        public virtual byte[] Value
        {
            get => field.CopyArray();
        }

        public override Proto.Services.EvmHookStorageUpdate ToProtobuf()
        {
            return new Proto.Services.EvmHookStorageUpdate
            {
                StorageSlot = new Proto.Services.EvmHookStorageSlot
                {
                    Key = ByteString.CopyFrom(Key),
                    Value = ByteString.CopyFrom(Value),

                }
            };
        }
        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            EvmHookStorageSlot that = (EvmHookStorageSlot)o;

            return Equals(Key, that.Key) && Equals(Value, that.Value);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key.GetHashCode(), Value.GetHashCode());
        }
    }
}
