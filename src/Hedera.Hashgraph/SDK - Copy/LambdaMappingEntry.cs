// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Represents an entry in a Solidity mapping.
    /// <p>
    /// This class is used to specify updates to Solidity mapping entries in a
    /// lambda EVM hook's storage. It supports both explicit key bytes and
    /// preimage-based keys for variable-length mapping keys.
    /// </summary>
    public class LambdaMappingEntry
    {
        private readonly byte[]? _Key;
        private readonly byte[]? _Preimage;
        private readonly byte[] _Value;
        /// <summary>
        /// Create a new mapping entry with an explicit key.
        /// </summary>
        /// <param name="key">the explicit mapping key (max 32 bytes, minimal representation)</param>
        /// <param name="value">the mapping value (max 32 bytes, minimal representation)</param>
        public static LambdaMappingEntry OfKey(byte[] key, byte[] value)
        {
            return new LambdaMappingEntry(key, null, value);
        }

        /// <summary>
        /// Create a new mapping entry with a preimage key.
        /// </summary>
        /// <param name="preimage">the preimage bytes for the mapping key</param>
        /// <param name="value">the mapping value (max 32 bytes, minimal representation)</param>
        public static LambdaMappingEntry WithPreimage(byte[] preimage, byte[] value)
        {
            return new LambdaMappingEntry(null, preimage, value);
        }

        private LambdaMappingEntry(byte[]? key, byte[]? preimage, byte[] value)
		{
			_Value = value.CopyArray();
			_Key = key?.CopyArray();
            _Preimage = preimage?.CopyArray();
        }

        /// <summary>
        /// Check if this entry uses an explicit key.
        /// </summary>
        /// <returns>true if using explicit key, false if using preimage</returns>
        public virtual bool HasExplicitKey
        {
			get => Key != null;
        }

        /// <summary>
        /// Check if this entry uses a preimage key.
        /// </summary>
        /// <returns>true if using preimage, false if using explicit key</returns>
        public virtual bool HasPreimageKey
        {
			get => _Preimage != null;
        }

        /// <summary>
        /// Get the explicit key if this entry uses one.
        /// </summary>
        /// <returns>a copy of the key bytes, or null if using preimage</returns>
        public virtual byte[]? Key
        {
			get => _Key?.CopyArray();
        }

        /// <summary>
        /// Get the preimage if this entry uses one.
        /// </summary>
        /// <returns>a copy of the preimage bytes, or null if using explicit key</returns>
        public virtual byte[]? Preimage
        {
			get => _Preimage?.CopyArray();
        }

        /// <summary>
        /// Get the mapping value.
        /// </summary>
        /// <returns>a copy of the value bytes</returns>
        public virtual byte[] Value
        {
            get => _Value.CopyArray();
        }

        /// <summary>
        /// Convert this mapping entry to a protobuf message.
        /// </summary>
        /// <returns>the protobuf LambdaMappingEntry</returns>
        public virtual Proto.LambdaMappingEntry ToProtobuf()
        {
			Proto.LambdaMappingEntry proto = new ();

            if (Key != null)
				proto.Key = ByteString.CopyFrom(Key);
            else if (_Preimage != null)
				proto.Preimage = ByteString.CopyFrom(_Preimage);

            if (Value.Length > 0)
				proto.Value = ByteString.CopyFrom(Value);

            return proto;
        }

        /// <summary>
        /// Create a LambdaMappingEntry from a protobuf message.
        /// </summary>
        /// <param name="proto">the protobuf LambdaMappingEntry</param>
        /// <returns>a new LambdaMappingEntry instance</returns>
        public static LambdaMappingEntry FromProtobuf(Proto.LambdaMappingEntry proto)
        {
            return proto.EntryKeyCase switch
            {
                Key => LambdaMappingEntry.OfKey(proto.GetKey().ToByteArray(), proto.GetValue().ToByteArray()),
                Preimage => LambdaMappingEntry.WithPreimage(proto.GetPreimage().ToByteArray(), proto.GetValue().ToByteArray()),
                NotSet => new ArgumentException("LambdaMappingEntry must have either key or preimage set")};
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            
            LambdaMappingEntry that = (LambdaMappingEntry)o;

            return Equals(Key, that.Key) && Equals(_Preimage, that._Preimage) && Equals(Value, that.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HashCode.Combine(Key), HashCode.Combine(_Preimage), HashCode.Combine(Value));
        }
    }
}