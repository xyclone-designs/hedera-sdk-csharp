using Google.Protobuf;
using Org.BouncyCastle.Utilities.Encoders;
using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * The ID for a cryptocurrency account on Hedera.
     */
    public sealed class EvmAddress : Key 
    {
        private readonly byte[] Bytes = [];

        /**
         * Constructor
         *
         * @param bytes the byte array representation of the address
         */
        public EvmAddress(byte[] bytes) 
        {
            Array.Copy(bytes, Bytes, bytes.Length);
        }

        /**
         * Convert a string to an ethereum address.
         *
         * @param evmAddress                      the string
         * @return                          the ethereum address
         */
        public static EvmAddress FromString(string evmAddress) {
            string address = evmAddress.StartsWith("0x") ? evmAddress[2..] : evmAddress;
            
            if (address.Length != 40)
				throw new ArgumentException("Invalid EvmAddress: " + evmAddress);

			return new EvmAddress(Hex.Decode(address));
			
        }
        public static EvmAddress? FromAliasBytes(ByteString aliasBytes) 
        {
            if (!aliasBytes.IsEmpty && aliasBytes.Length == 20)
				return new EvmAddress(aliasBytes.ToByteArray());

			return null;
        }
        /**
         * Convert a byte array to an ethereum address.
         *
         * @param bytes                     the byte array
         * @return                          the ethereum address
         */
        public new static EvmAddress FromBytes(byte[] bytes) 
        {
            return new EvmAddress(bytes);
        }
        public new byte[] ToBytes() 
        {
            byte[] copy = [];
            Bytes.CopyTo(copy);
            return copy;
        }

        public override string ToString() 
        {
            return Hex.ToHexString(Bytes);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Bytes);
        }
        public override bool Equals(object? o) {
            if (this == o) return true;
            if (o is not EvmAddress evmaddress) return false;

            return Equals(Bytes, evmaddress.Bytes);
        }
		public override Proto.Key ToProtobufKey()
		{
			throw new NotImplementedException("ToProtobufKey() not implemented for EvmAddress");
		}
	}
}