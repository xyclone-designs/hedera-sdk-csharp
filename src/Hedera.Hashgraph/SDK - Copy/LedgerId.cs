// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Org.BouncyCastle.Utilities.Encoders;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Internal utility class for ledger id manipulation.
    /// </summary>
    public class LedgerId
    {
        private readonly byte[] IdBytes;
        /// <summary>
        /// The mainnet ledger id
        /// </summary>
        public static readonly LedgerId MAINNET = new ([ 0 ]);
        /// <summary>
        /// The testnet ledger id
        /// </summary>
        public static readonly LedgerId TESTNET = new ([ 1 ]);
        /// <summary>
        /// The previewnet ledger id
        /// </summary>
        public static readonly LedgerId PREVIEWNET = new ([ 2 ]);
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="idBytes">the id (0=mainnet, 1=testnet, 2=previewnet, ...)</param>
        public LedgerId(byte[] idBytes)
        {
            IdBytes = idBytes;
        }

        /// <summary>
        /// Assign the ledger id via a string name or Hex encoded String.
        /// </summary>
        /// <param name="string">the string containing the ledger id</param>
        /// <returns>                         the ledger id</returns>
        public static LedgerId FromString(string @string)
        {
            switch (@string)
            {
                case "mainnet":
                    return MAINNET;
                case "testnet":
                    return TESTNET;
                case "previewnet":
                    return PREVIEWNET;
                default:
                    return new LedgerId(Hex.Decode(@string));
                    
            }
        }
        /// <summary>
        /// Create a ledger id from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the ledger id</returns>
        public static LedgerId FromBytes(byte[] bytes)
        {
            return new LedgerId(bytes);
        }
        /// <summary>
        /// Create a ledger id from a string.
        /// </summary>
        /// <param name="byteString">the string</param>
        /// <returns>                         the ledger id</returns>
        public static LedgerId FromByteString(ByteString byteString)
        {
            return FromBytes(byteString.ToByteArray());
        }

        /// <summary>
        /// Create a ledger id from a network name.
        /// </summary>
        /// <param name="networkName">the network name</param>
        /// <returns>                         the ledger id</returns>
        public static LedgerId FromNetworkName(NetworkName networkName)
        {
            return networkName switch
            {
                NetworkName.MainNet => MAINNET,
                NetworkName.TestNet => TESTNET,
                NetworkName.PreviewNet => PREVIEWNET,

                _ => throw new ArgumentException("networkName must be MAINNET, TESTNET, or PREVIEWNET"),
            };
        }

        /// <summary>
        /// Are we on Mionnet?
        /// </summary>
        /// <returns>                         is it mainnet</returns>
        public virtual bool IsMainnet { get => Equals(MAINNET); }

        /// <summary>
        /// Are we on Testnet?
        /// </summary>
        /// <returns>                         is it testnet</returns>
        public virtual bool IsTestnet { get => Equals(TESTNET); }

        /// <summary>
        /// Are we on Previewnet?
        /// </summary>
        /// <returns>                         is it previewnet</returns>
        public virtual bool IsPreviewnet { get => Equals(PREVIEWNET); }

        /// <summary>
        /// Are we one of the three standard networks?
        /// </summary>
        /// <returns>                         is it one of the three standard networks</returns>
        public virtual bool IsKnownNetwork { get => IsMainnet || IsTestnet || IsPreviewnet; }

        /// <summary>
        /// Extract the string representation.
        /// </summary>
        /// <returns>                         the string representation</returns>
        public override string ToString()
        {
            return true switch
            {
				true when IsMainnet => "mainnet",
				true when IsTestnet => "testnet",
				true when IsPreviewnet => "previewnet",

				_ => Hex.ToHexString(IdBytes)
            };
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return IdBytes.CopyArray(IdBytes.Length);
        }

        /// <summary>
        /// Extract the byte string representation.
        /// </summary>
        /// <returns>                         the byte string representation</returns>
        public virtual ByteString ToByteString()
        {
            return ByteString.CopyFrom(IdBytes);
        }

        /// <summary>
        /// Extract the network name.
        /// </summary>
        /// <returns>                         the network name</returns>
        public virtual NetworkName ToNetworkName()
        {
			return true switch
			{
				true when IsMainnet => NetworkName.MainNet,
				true when IsTestnet => NetworkName.TestNet,
				true when IsPreviewnet => NetworkName.PreviewNet,

				_ => NetworkName.Other
			};
        }

        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is LedgerId))
            {
                return false;
            }

            LedgerId otherId = (LedgerId)o;

            return Equals(IdBytes, otherId.IdBytes);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IdBytes);
        }
    }
}