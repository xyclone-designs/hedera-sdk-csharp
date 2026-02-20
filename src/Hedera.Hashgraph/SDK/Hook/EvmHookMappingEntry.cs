// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Google.Protobuf;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Hook
{
    public class EvmHookMappingEntry
    {
        private EvmHookMappingEntry(byte[]? key, byte[]? preimage, byte[] value)
        {
            Key = key?.CopyArray();
            PreImage = preimage?.CopyArray();
            Value = value.CopyArray();

            HasExplicitKey = Key is not null;
            HasPreimageKey = PreImage is not null;
		}

		public static EvmHookMappingEntry OfKey(byte[] key, byte[] value)
		{
			return new EvmHookMappingEntry(key, null, value);
		}
		public static EvmHookMappingEntry WithPreimage(byte[] preimage, byte[] value)
		{
			return new EvmHookMappingEntry(null, preimage, value);
		}
		public static EvmHookMappingEntry FromProtobuf(Proto.EvmHookMappingEntry proto)
		{
			return proto.EntryKeyCase switch
			{
				KEY => EvmHookMappingEntry.OfKey(proto.Key.ToByteArray(), proto.Value().ToByteArray()),
				PREIMAGE => EvmHookMappingEntry.WithPreimage(proto.Preimage.ToByteArray(), proto.Value().ToByteArray()),
				ENTRYKEY_NOT_SET => new ArgumentException("EvmHookMappingEntry must have either key or preimage set")
			};
		}

		public virtual bool HasExplicitKey { get; }
		public virtual bool HasPreimageKey { get; }
        public virtual byte[]? Key
		{
			get => field?.CopyArray();
		}
        public virtual byte[]? PreImage
		{
            get => field?.CopyArray();
        }
        public virtual byte[] Value
        {
            get => field.CopyArray();
        }

        public virtual Proto.EvmHookMappingEntry ToProtobuf()
        {
            var builder = new Proto.EvmHookMappingEntry();

            if (Key != null)
            {
                builder.SetKey(ByteString.CopyFrom(Key));
            }
            
            if (PreImage != null)
            {
                builder.SetPreimage(ByteString.CopyFrom(PreImage));
            }

            if (Value.Length > 0)
            {
                builder.SetValue(ByteString.CopyFrom(Value));
            }

            return builder;
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            EvmHookMappingEntry that = (EvmHookMappingEntry)o;

            return Equals(Key, that.Key) && Equals(PreImage, that.PreImage) && Equals(Value, that.Value);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key?.GetHashCode(), PreImage?.GetHashCode(), Value.GetHashCode());
        }
    }
}