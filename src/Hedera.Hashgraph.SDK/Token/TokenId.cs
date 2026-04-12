// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Nfts;
using System;
using System.IO;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenId.cs.xml" path='docs/member[@name="T:TokenId"]/*' />
    public class TokenId : IComparable<TokenId>
    {
        /// <include file="TokenId.cs.xml" path='docs/member[@name="F:TokenId.Shard"]/*' />
        public readonly long Shard;
        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.#ctor(System.Int64)"]/*' />
        public readonly long Realm;
        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.#ctor(System.Int64)_2"]/*' />
        public readonly long Num;
        private readonly string? Checksum;

        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.#ctor(System.Int64)_3"]/*' />
        public TokenId(long num) : this(0, 0, num) { }

        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.#ctor(System.Int64,System.Int64,System.Int64)"]/*' />
        public TokenId(long shard, long realm, long num) : this(shard, realm, num, null) { }

        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.TokenId(System.Int64,System.Int64,System.Int64,System.String)"]/*' />
        TokenId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
        }

        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.FromString(System.String)"]/*' />
        public static TokenId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new TokenId(a, b, c, d));
        }
		/// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.FromBytes(System.Byte[])"]/*' />
		public static TokenId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.TokenID.Parser.ParseFrom(bytes));
		}
		/// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.FromProtobuf(Proto.Services.TokenID)"]/*' />
		public static TokenId FromProtobuf(Proto.Services.TokenID tokenId)
        {
            return new TokenId(tokenId.ShardNum, tokenId.RealmNum, tokenId.TokenNum);
        }
        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.FromSolidityAddress(System.String)"]/*' />
        public static TokenId FromSolidityAddress(string address)
        {
            return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new TokenId(a, b, c, d));
        }
        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.FromEvmAddress(System.Int64,System.Int64,System.String)"]/*' />
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

        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.Nft(System.Int64)"]/*' />
        public virtual NftId Nft(long serial)
        {
            return new NftId(this, serial);
        }

		/// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.ToEvmAddress"]/*' />
		public virtual string ToEvmAddress()
		{
			return Utils.EntityIdHelper.ToSolidityAddress(0, 0, Num);
		}
		/// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.ToSolidityAddress"]/*' />
		public virtual string ToSolidityAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
        }
        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenID ToProtobuf()
        {
            return new Proto.Services.TokenID
            {
				ShardNum = Shard,
				RealmNum = Realm,
				TokenNum = Num,
			};
        }

		/// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.Validate(Client)"]/*' />
		public virtual void Validate(Client client)
        {
            ValidateChecksum(client);
        }
        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.ValidateChecksum(Client)"]/*' />
        public virtual void ValidateChecksum(Client client)
        {
            Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
        }

        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.ToBytes"]/*' />
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override string ToString()
        {
            return Utils.EntityIdHelper.ToString(Shard, Realm, Num);
        }

        /// <include file="TokenId.cs.xml" path='docs/member[@name="M:TokenId.ToStringWithChecksum(Client)"]/*' />
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
