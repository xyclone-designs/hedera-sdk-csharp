// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
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
        /// The Shard Number
        /// </summary>
        public readonly long Shard;
        /// <summary>
        /// The Realm Number
        /// </summary>
        public readonly long Realm;
        /// <summary>
        /// The id Number
        /// </summary>
        public readonly long Num;
        private readonly string? Checksum;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Num">the Num part
        /// 
        /// Constructor that uses Shard, Realm and Num should be used instead
        /// as Shard and Realm should not assume 0 value</param>
        public TokenId(long num) : this(0, 0, num) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Shard">the Shard part</param>
        /// <param name="Realm">the Realm part</param>
        /// <param name="Num">the Num part</param>
        public TokenId(long shard, long realm, long num) : this(shard, realm, num, null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Shard">the Shard part</param>
        /// <param name="Realm">the Realm part</param>
        /// <param name="Num">the Num part</param>
        /// <param name="Checksum">the Checksum</param>
        TokenId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
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
        /// Constructs a TokenID from Shard, Realm, and EVM address.
        /// The EVM address must be a "long zero address" (first 12 bytes are zero).
        /// </summary>
        /// <param name="Shard">the Shard Number</param>
        /// <param name="Realm">the Realm Number</param>
        /// <param name="evmAddress">the EVM address as a hex string</param>
        /// <returns>          the TokenID object</returns>
        /// <exception cref="IllegalArgumentException">if the EVM address is not a valid long zero address</exception>
        public static TokenId FromEvmAddress(long Shard, long Realm, string evmAddress)
        {
            byte[] addressBytes = Utils.EntityIdHelper.DecodeEvmAddress(evmAddress);

            if (!Utils.EntityIdHelper.IsLongZeroAddress(addressBytes))
				throw new ArgumentException("EVM address is not a correct long zero address");

			using MemoryStream ms = new(addressBytes);
			using BinaryReader reader = new(ms);

			reader.ReadInt32();
			reader.ReadInt64();

			long tokenNum = reader.ReadInt64();

			return new TokenId(Shard, Realm, tokenNum);
		}        

        /// <summary>
        /// Create an nft id.
        /// </summary>
        /// <param name="serial">the serial Number</param>
        /// <returns>                         the new nft id</returns>
        public virtual NftId Nft(long serial)
        {
            return new NftId(this, serial);
        }

		/// <summary>
		/// Converts this TokenId to an EVM address string.
		/// Creates a solidity address using Shard=0, Realm=0, and the file Number.
		/// </summary>
		/// <returns>the EVM address as a hex string</returns>
		public virtual string ToEvmAddress()
		{
			return Utils.EntityIdHelper.ToSolidityAddress(0, 0, Num);
		}
		/// <summary>
		/// Extract the solidity address.
		/// </summary>
		/// <returns>                         the solidity address as a string</returns>
		/// <remarks>@deprecatedThis method is deprecated. Use {@link #toEvmAddress()} instead.</remarks>
		public virtual string ToSolidityAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
        }
        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         a protobuf representation</returns>
        public virtual Proto.TokenID ToProtobuf()
        {
            return new Proto.TokenID
            {
				ShardNum = Shard,
				RealmNum = Realm,
				TokenNum = Num,
			};
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
            Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
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
            return Utils.EntityIdHelper.ToString(Shard, Realm, Num);
        }

        /// <summary>
        /// Create a string representation with Checksum.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <returns>                         the string representation with Checksum</returns>
        public virtual string ToStringWithChecksum(Client client)
        {
            return Utils.EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Shard, Realm, Num);
        }
        public override bool Equals(object? o)
        {
            if (this == o)
				return true;

			if (o is not TokenId otherId)
				return false;

            return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num;
        }

        public virtual int CompareTo(TokenId? o)
        {
            int ShardComparison = Shard.CompareTo(o?.Shard);
            
            if (ShardComparison != 0)
                return ShardComparison;

            int RealmComparison = Realm.CompareTo(o?.Realm);
            
            if (RealmComparison != 0)
				return RealmComparison;

			return Num.CompareTo(o?.Num);
        }
    }
}