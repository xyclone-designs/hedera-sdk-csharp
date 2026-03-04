// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Org.BouncyCastle.Utilities.Encoders;

using System;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <include file="LedgerId.cs.xml" path='docs/member[@name="T:LedgerId"]/*' />
    public class LedgerId(byte[] idBytes)
    {
        private readonly byte[] IdBytes = idBytes;
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="T:LedgerId_2"]/*' />
        public static readonly LedgerId MAINNET = new ([ 0 ]);
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="T:LedgerId_3"]/*' />
        public static readonly LedgerId TESTNET = new ([ 1 ]);
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="T:LedgerId_4"]/*' />
        public static readonly LedgerId PREVIEWNET = new ([ 2 ]);

        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.FromString(string @)"]/*' />
        public static LedgerId FromString(string @string)
        {
            return @string switch
            {
                "mainnet" => MAINNET,
                "testnet" => TESTNET,
                "previewnet" => PREVIEWNET,

                _ => new LedgerId(Hex.Decode(@string)),
            };
        }
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.FromBytes(System.Byte[])"]/*' />
        public static LedgerId FromBytes(byte[] bytes)
        {
            return new LedgerId(bytes);
        }
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.FromByteString(ByteString)"]/*' />
        public static LedgerId FromByteString(ByteString byteString)
        {
            return FromBytes(byteString.ToByteArray());
        }
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.FromNetworkName(NetworkName)"]/*' />
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

        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.Equals(MAINNET)"]/*' />
        public virtual bool IsMainnet { get => Equals(MAINNET); }
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.Equals(TESTNET)"]/*' />
        public virtual bool IsTestnet { get => Equals(TESTNET); }
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.Equals(PREVIEWNET)"]/*' />
        public virtual bool IsPreviewnet { get => Equals(PREVIEWNET); }
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="P:LedgerId.IsKnownNetwork"]/*' />
        public virtual bool IsKnownNetwork { get => IsMainnet || IsTestnet || IsPreviewnet; }

        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.ToBytes"]/*' />
        public virtual byte[] ToBytes()
        {
            return IdBytes.CopyArray(IdBytes.Length);
        }
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.ToByteString"]/*' />
        public virtual ByteString ToByteString()
        {
            return ByteString.CopyFrom(IdBytes);
        }
        /// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.ToNetworkName"]/*' />
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

        public override int GetHashCode()
        {
            return HashCode.Combine(IdBytes);
        }
		/// <include file="LedgerId.cs.xml" path='docs/member[@name="M:LedgerId.ToString"]/*' />
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
		public override bool Equals(object? o)
		{
			if (this == o)
				return true;

			if (o is not LedgerId otherId)
				return false;

			return Equals(IdBytes, otherId.IdBytes);
		}
	}
}