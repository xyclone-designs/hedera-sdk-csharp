// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;
using System.IO;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Constructs a TokenId.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/token-id">Hedera Documentation</a>
    /// </summary>
    public class TokenId : IComparable<TokenId>
    {
        /// <summary>
        /// The shard number
        /// </summary>
        public readonly long shard;
        /// <summary>
        /// The realm number
        /// </summary>
        public readonly long realm;
        /// <summary>
        /// The id number
        /// </summary>
        public readonly long num;
        private readonly string checksum;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="num">the num part
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</param>
        public TokenId(long num) : this(0, 0, num) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        public TokenId(long shard, long realm, long num) : this(shard, realm, num, null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        /// <param name="checksum">the checksum</param>
        TokenId(long shard, long realm, long num, string checksum)
        {
            shard = shard;
            realm = realm;
            num = num;
            checksum = checksum;
        }

        /// <summary>
        /// Create a token id from a string.
        /// </summary>
        /// <param name="id">the string representation</param>
        /// <returns>                         the new token id</returns>
        public static TokenId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new TokenId(a, b, c, d));
        }
		/// <summary>
		/// Create a token id from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new token id</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static TokenId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TokenID.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a token id from a protobuf.
		/// </summary>
		/// <param name="tokenId">the protobuf</param>
		/// <returns>                         the new token id</returns>
		public static TokenId FromProtobuf(Proto.TokenID tokenId)
        {
            return new TokenId(tokenId.ShardNum, tokenId.RealmNum, tokenId.TokenNum);
        }
        /// <summary>
        /// Retrieve the token id from a solidity address.
        /// </summary>
        /// <param name="address">a string representing the address</param>
        /// <returns>                         the token id object</returns>
        /// <remarks>@deprecatedThis method is deprecated. Use {@link #fromEvmAddress(long, long, String)} instead.</remarks>
        public static TokenId FromSolidityAddress(string address)
        {
            return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new TokenId(a, b, c, d));
        }
        /// <summary>
        /// Constructs a TokenID from shard, realm, and EVM address.
        /// The EVM address must be a "long zero address" (first 12 bytes are zero).
        /// </summary>
        /// <param name="shard">the shard number</param>
        /// <param name="realm">the realm number</param>
        /// <param name="evmAddress">the EVM address as a hex string</param>
        /// <returns>          the TokenID object</returns>
        /// <exception cref="IllegalArgumentException">if the EVM address is not a valid long zero address</exception>
        public static TokenId FromEvmAddress(long shard, long realm, string evmAddress)
        {
            byte[] addressBytes = Utils.EntityIdHelper.DecodeEvmAddress(evmAddress);

            if (!Utils.EntityIdHelper.IsLongZeroAddress(addressBytes))
				throw new ArgumentException("EVM address is not a correct long zero address");

			using MemoryStream ms = new(addressBytes);
			using BinaryReader reader = new(ms);

			reader.ReadInt32();
			reader.ReadInt64();

			long tokenNum = reader.ReadInt64();

			return new TokenId(shard, realm, tokenNum);
		}        

        /// <summary>
        /// Create an nft id.
        /// </summary>
        /// <param name="serial">the serial number</param>
        /// <returns>                         the new nft id</returns>
        public virtual NftId Nft(long serial)
        {
            return new NftId(this, serial);
        }

		/// <summary>
		/// Converts this TokenId to an EVM address string.
		/// Creates a solidity address using shard=0, realm=0, and the file number.
		/// </summary>
		/// <returns>the EVM address as a hex string</returns>
		public virtual string ToEvmAddress()
		{
			return Utils.EntityIdHelper.ToSolidityAddress(0, 0, num);
		}
		/// <summary>
		/// Extract the solidity address.
		/// </summary>
		/// <returns>                         the solidity address as a string</returns>
		/// <remarks>@deprecatedThis method is deprecated. Use {@link #toEvmAddress()} instead.</remarks>
		public virtual string ToSolidityAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(shard, realm, num);
        }
        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         a protobuf representation</returns>
        public virtual Proto.TokenID ToProtobuf()
        {
            return new Proto.TokenID
            {
				ShardNum = shard,
				RealmNum = realm,
				TokenNum = num,
			};
        }

		/// <summary>
		/// Extract the checksum.
		/// </summary>
		/// <returns>                         the checksum</returns>
		public virtual string GetChecksum()
		{
			return checksum;
		}
		/// <summary>
		/// </summary>
		/// <param name="client">to validate against</param>
		/// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
		/// <remarks>@deprecatedUse {@link #validateChecksum(Client)} instead.</remarks>
		public virtual void Validate(Client client)
        {
            ValidateChecksum(client);
        }
        /// <summary>
        /// Validate the configured client.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        public virtual void ValidateChecksum(Client client)
        {
            Utils.EntityIdHelper.Validate(shard, realm, num, client, checksum);
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override string ToString()
        {
            return Utils.EntityIdHelper.ToString(shard, realm, num);
        }

        /// <summary>
        /// Create a string representation with checksum.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <returns>                         the string representation with checksum</returns>
        public virtual string ToStringWithChecksum(Client client)
        {
            return Utils.EntityIdHelper.ToStringWithChecksum(shard, realm, num, client, checksum);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(shard, realm, num);
        }
        public override bool Equals(object? o)
        {
            if (this == o)
				return true;

			if (o is not TokenId otherId)
				return false;

            return shard == otherId.shard && realm == otherId.realm && num == otherId.num;
        }

        public virtual int CompareTo(TokenId? o)
        {
            int shardComparison = shard.CompareTo(o?.shard);
            
            if (shardComparison != 0)
                return shardComparison;

            int realmComparison = realm.CompareTo(o?.realm);
            
            if (realmComparison != 0)
				return realmComparison;

			return num.CompareTo(o?.num);
        }
    }
}