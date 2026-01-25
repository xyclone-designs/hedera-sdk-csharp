// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Token;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// The (non-fungible) token of which this NFT is an instance
    /// </summary>
    public class NftId : IComparable<NftId>
    {
        /// <summary>
        /// The (non-fungible) token of which this NFT is an instance
        /// </summary>
        public readonly TokenId TokenId;
        /// <summary>
        /// The unique identifier of this instance
        /// </summary>
        public readonly long Serial;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="serial">the serial number</param>
        public NftId(TokenId tokenId, long serial)
        {
            TokenId = tokenId;
            Serial = serial;
        }

		/// <summary>
		/// Create a new nft id from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new nft id</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static NftId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.NftID.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a new nft id from a string.
		/// </summary>
		/// <param name="id">the string representation</param>
		/// <returns>                         the new nft id</returns>
		public static NftId FromString(string id)
        {
            var parts = id.Split("[/@]");
            if (parts.Length != 2)
            {
                throw new ArgumentException("Expecting {shardNum}.{realmNum}.{idNum}-{checksum}/{serialNum}");
            }

            return new NftId(TokenId.FromString(parts[0]), long.Parse(parts[1]));
        }
		/// <summary>
		/// Create a new ntf id from a protobuf.
		/// </summary>
		/// <param name="nftId">the protobuf representation</param>
		/// <returns>                         the new nft id</returns>
		public static NftId FromProtobuf(Proto.NftID nftId)
        {
            return new NftId(TokenId.FromProtobuf(nftId.TokenID), nftId.SerialNumber);
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         a protobuf representation</returns>
        public virtual Proto.NftID ToProtobuf()
        {
            return new Proto.NftID
            {
                TokenID = TokenId.ToProtobuf(),
                SerialNumber = Serial,
            };
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         a byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <summary>
		/// Generate a string representation with checksum.
		/// </summary>
		/// <param name="client">the configured client</param>
		/// <returns>                         the string representation with checksum</returns>
		public virtual string ToStringWithChecksum(Client client)
		{
			return TokenId.ToStringWithChecksum(client) + "/" + Serial;
		}

		public override string ToString()
        {
            return TokenId.ToString() + "/" + Serial;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(TokenId.shard, TokenId.realm, TokenId.num, Serial);
        }
        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is NftId))
            {
                return false;
            }

            NftId otherId = (NftId)o;
            return TokenId.Equals(otherId.TokenId) && Serial == otherId.Serial;
        }

		public virtual int CompareTo(NftId? o)
		{
			int tokenComparison = TokenId.CompareTo(o.TokenId);
			if (tokenComparison != 0)
			{
				return tokenComparison;
			}

			return Serial.CompareTo(o.Serial);
		}
	}
}