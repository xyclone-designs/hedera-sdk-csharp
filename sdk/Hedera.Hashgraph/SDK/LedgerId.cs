using Google.Protobuf;

using Org.BouncyCastle.Utilities.Encoders;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Internal utility class for ledger id manipulation.
     */
    public class LedgerId 
    {
        private readonly byte[] IdBytes;
        public static readonly LedgerId Mainnet = new ([0]);
        public static readonly LedgerId Testnet = new ([1]);
        public static readonly LedgerId Previewnet = new ([2]);

        /**
         * Constructor.
         *
         * @param idBytes                   the id (0=mainnet, 1=testnet, 2=previewnet, ...)
         */
        private LedgerId(byte[] idBytes) 
        {
            this.IdBytes = idBytes;
        }

        /**
         * Assign the ledger id via a string name or Hex encoded string.
         *
         * @param string                    the string containing the ledger id
         * @return                          the ledger id
         */
        public static LedgerId FromString(string str) {
            return str switch
            {
                "mainnet" => Mainnet,
                "testnet" => Testnet,
                "previewnet" => Previewnet,

                _ => new LedgerId(Hex.Decode(str)),
            };
        }
        /**
         * Create a ledger id from a byte array.
         *
         * @param bytes                     the byte array
         * @return                          the ledger id
         */
        public static LedgerId FromBytes(byte[] bytes) 
        {
            return new LedgerId(bytes);
        }
        /**
         * Create a ledger id from a string.
         *
         * @param byteString                the string
         * @return                          the ledger id
         */
        public static LedgerId FromByteString(ByteString byteString) 
        {
            return FromBytes(byteString.ToByteArray());
        }

		/**
         * Are we one of the three standard networks?
         *
         * @return                          Is it one of the three standard networks
         */
		bool IsKnownNetwork()
		{
			return IsMainnet() || IsTestnet() || IsPreviewnet();
		}
		/**
         * Are we on Mionnet?
         *
         * @return                          Is it mainnet
         */
		public bool IsMainnet() 
        {
            return Equals(Mainnet);
        }
        /**
         * Are we on Testnet?
         *
         * @return                          Is it testnet
         */
        public bool IsTestnet() 
        {
            return Equals(Testnet);
        }
        /**
         * Are we on Previewnet?
         *
         * @return                          Is it previewnet
         */
        public bool IsPreviewnet() 
        {
            return Equals(Previewnet);
        }
        /**
         * Create the byte array.
         *
         * @return                          the byte array representation
         */
        public byte[] ToBytes() 
        {
            byte[] _new = [];

            Array.Copy(IdBytes, _new, IdBytes.Length);

            return _new;
        }

        /**
         * Extract the byte string representation.
         *
         * @return                          the byte string representation
         */
        public ByteString ToByteString() 
        {
            return ByteString.CopyFrom(IdBytes);
        }

        /**
         * Extract the network name.
         *
         * @return                          the network name
         */
        [Obsolete]
        public NetworkName ToNetworkName() {
            if (IsMainnet()) return NetworkName.Mainnet;
            if (IsTestnet()) return NetworkName.Testnet;
            if (IsPreviewnet()) return NetworkName.PreviewNet;
            else return NetworkName.Other;
        }

        public override bool Equals(object? obj) {
            if (this == obj) return true;
            if (obj is not LedgerId ledgerid) return false;

            return Equals(IdBytes, ledgerid.IdBytes);
        }
        public override int GetHashCode() 
        {
            return HashCode.Combine(IdBytes);
        }
        /**
         * Extract the string representation.
         *
         * @return                          the string representation
         */
        public override string ToString() 
        {
            return true switch
            {
                true when IsMainnet() => "mainnet",
                true when IsTestnet() => "testnet",
                true when IsPreviewnet() => "previewnet",
                _ => Hex.ToHexString(IdBytes)
			};
        }
    }

}