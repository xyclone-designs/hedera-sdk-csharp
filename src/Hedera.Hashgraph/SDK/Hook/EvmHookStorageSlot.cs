// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Hook
{
    public class EvmHookStorageSlot : EvmHookStorageUpdate
    {
        public EvmHookStorageSlot(byte[] key, byte[]? value)
        {
            Key = key.CopyArray();
            Value = value?.CopyArray() ?? [];
        }

		public static EvmHookStorageSlot FromProtobuf(Proto.EvmHookStorageSlot proto)
		{
			return new EvmHookStorageSlot(proto.GetKey().ToByteArray(), proto.GetValue().ToByteArray());
		}

		public virtual byte[] Key
        {
            get => field.CopyArray();
        }
        public virtual byte[] Value
        {
            get => field.CopyArray();
        }

        public override Proto.EvmHookStorageUpdate ToProtobuf()
        {
            return new Proto.EvmHookStorageUpdate
            {
                StorageSlot = new Proto.EvmHookStorageSlot
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