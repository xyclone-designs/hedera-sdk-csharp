// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Keys;

using Org.BouncyCastle.Utilities.Encoders;

using System;

namespace Hedera.Hashgraph.SDK.Ethereum
{
    /// <include file="EvmAddress.cs.xml" path='docs/member[@name="T:EvmAddress"]/*' />
    public sealed class EvmAddress : Key
    {
        private readonly byte[] bytes = [];

        /// <include file="EvmAddress.cs.xml" path='docs/member[@name="M:EvmAddress.#ctor(System.Byte[])"]/*' />
        public EvmAddress(byte[] bytes)
        {
            Array.Copy(bytes, bytes, bytes.Length);
        }

        /// <include file="EvmAddress.cs.xml" path='docs/member[@name="M:EvmAddress.FromString(System.String)"]/*' />
        public static EvmAddress FromString(string evmAddress)
        {
            string address = evmAddress.StartsWith("0x") ? evmAddress.Substring(2) : evmAddress;
            if (address.Length == 40)
            {
                return new EvmAddress(Hex.Decode(address));
            }

            throw new ArgumentException("Invalid EvmAddress: " + evmAddress);
        }
        public static EvmAddress? FromAliasBytes(ByteString aliasBytes)
        {
            if (aliasBytes.Length == 20)
				return new EvmAddress(aliasBytes.ToByteArray());

			return null;
        }

        /// <include file="EvmAddress.cs.xml" path='docs/member[@name="M:EvmAddress.FromBytes(System.Byte[])"]/*' />
        public new static EvmAddress FromBytes(byte[] bytes)
        {
            return new EvmAddress(bytes);
        }

        public override Proto.Services.Key ToProtobufKey()
        {
            throw new NotSupportedException("toProtobufKey() not implemented for EvmAddress");
        }
        public override byte[] ToBytes()
        {
            return bytes.CopyArray();
        }
        public override string ToString()
        {
            return Hex.ToHexString(bytes);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(bytes);
        }
        public override bool Equals(object? o)
        {
            if (this == o)
				return true;

			if (o is not EvmAddress other)
				return false;

            return Equals(bytes, other.bytes);
        }
    }
}
