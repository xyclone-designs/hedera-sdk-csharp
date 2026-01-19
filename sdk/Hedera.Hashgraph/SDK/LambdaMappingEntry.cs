using Google.Protobuf;
using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Represents an entry in a Solidity mapping.
     * <p>
     * This class is used to specify updates to Solidity mapping entries in a
     * lambda EVM hook's storage. It supports both explicit Key bytes and
     * PreImage-based Keys for variable-length mapping Keys.
     */
    public class LambdaMappingEntry 
    {
        private byte[]? _Key;
		private byte[]? _PreImage;
		private byte[] _Value = [];

		private LambdaMappingEntry(byte[]? key, byte[]? preimage, byte[] value)
		{
			key?.CopyTo(_Key = []);
			value.CopyTo(_Value = []);
			preimage?.CopyTo(_PreImage = []);
		}

		public byte[]? Key { get => (byte[]?)_Key?.Clone(); }
        public byte[]? PreImage { get => (byte[]?)_PreImage?.Clone(); }
        public byte[] Value { get => (byte[])_Value.Clone(); }
		public bool HasKey { get => Key is not null; }
		public bool HasPreImage { get => PreImage is not null; }

		/**
         * Create a new mapping entry with an explicit Key.
         *
         * @param Key the explicit mapping Key (max 32 bytes, minimal representation)
         * @param Value the mapping Value (max 32 bytes, minimal representation)
         */
		public static LambdaMappingEntry OfKey(byte[] key, byte[] value) 
        {
            return new LambdaMappingEntry(key, null, value);
        }
        /**
         * Create a new mapping entry with a PreImage Key.
         *
         * @param PreImage the PreImage bytes for the mapping Key
         * @param Value the mapping Value (max 32 bytes, minimal representation)
         */
        public static LambdaMappingEntry WithPreimage(byte[] preimage, byte[] value) 
        {
            return new LambdaMappingEntry(null, preimage, value);
        }

        /**
         * Convert this mapping entry to a protobuf message.
         *
         * @return the protobuf LambdaMappingEntry
         */
        public Proto.LambdaMappingEntry ToProtobuf() 
        {
            return new Proto.LambdaMappingEntry
            {
                Key = ByteString.CopyFrom(_Key),
                Preimage = ByteString.CopyFrom(_PreImage),
                Value = ByteString.CopyFrom(_Value),
            };
        }

        /**
         * Create a LambdaMappingEntry from a protobuf message.
         *
         * @param proto the protobuf LambdaMappingEntry
         * @return a new LambdaMappingEntry instance
         */
        public static LambdaMappingEntry FromProtobuf(Proto.LambdaMappingEntry proto) 
        {
            return proto.EntryKeyCase switch 
            {
                Proto.LambdaMappingEntry.EntryKeyOneofCase.Key => OfKey(proto.Key.ToByteArray(), proto.Value.ToByteArray()),
                Proto.LambdaMappingEntry.EntryKeyOneofCase.Preimage => OfKey(proto.Preimage.ToByteArray(), proto.Value.ToByteArray()),
				Proto.LambdaMappingEntry.EntryKeyOneofCase.None or _ => throw new ArgumentException("LambdaMappingEntry must have either Key or PreImage set"),
            };
        }

        public override bool Equals(object? obj) 
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            LambdaMappingEntry that = (LambdaMappingEntry) obj;

            return 
                Equals(Key, that.Key) &&
                Equals(PreImage, that.PreImage) && 
                Equals(Value, that.Value);
        }
        public override int GetHashCode() 
        {
            return HashCode.Combine(Key, PreImage, Value);
        }
    }
}