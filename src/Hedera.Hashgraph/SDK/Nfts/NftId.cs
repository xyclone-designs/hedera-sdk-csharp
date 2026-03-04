// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Token;

using System;

namespace Hedera.Hashgraph.SDK.Nfts
{
	/// <include file="NftId.cs.xml" path='docs/member[@name="T:NftId"]/*' />
	public class NftId : IComparable<NftId>
    {
        /// <include file="NftId.cs.xml" path='docs/member[@name="M:NftId.#ctor(TokenId,System.Int64)"]/*' />
        public readonly TokenId TokenId;
        /// <include file="NftId.cs.xml" path='docs/member[@name="M:NftId.#ctor(TokenId,System.Int64)_2"]/*' />
        public readonly long Serial;
        /// <include file="NftId.cs.xml" path='docs/member[@name="M:NftId.#ctor(TokenId,System.Int64)_3"]/*' />
        public NftId(TokenId tokenId, long serial)
        {
            TokenId = tokenId;
            Serial = serial;
        }

		/// <include file="NftId.cs.xml" path='docs/member[@name="M:NftId.FromBytes(System.Byte[])"]/*' />
		public static NftId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.NftID.Parser.ParseFrom(bytes));
		}
		/// <include file="NftId.cs.xml" path='docs/member[@name="M:NftId.FromString(System.String)"]/*' />
		public static NftId FromString(string id)
        {
            var parts = id.Split("[/@]");
            if (parts.Length != 2)
            {
                throw new ArgumentException("Expecting {shardNum}.{realmNum}.{idNum}-{checksum}/{serialNum}");
            }

            return new NftId(TokenId.FromString(parts[0]), long.Parse(parts[1]));
        }
		/// <include file="NftId.cs.xml" path='docs/member[@name="M:NftId.FromProtobuf(Proto.NftID)"]/*' />
		public static NftId FromProtobuf(Proto.NftID nftId)
        {
            return new NftId(TokenId.FromProtobuf(nftId.TokenID), nftId.SerialNumber);
        }

        /// <include file="NftId.cs.xml" path='docs/member[@name="M:NftId.ToProtobuf"]/*' />
        public virtual Proto.NftID ToProtobuf()
        {
            return new Proto.NftID
            {
                TokenID = TokenId.ToProtobuf(),
                SerialNumber = Serial,
            };
        }

        /// <include file="NftId.cs.xml" path='docs/member[@name="M:NftId.ToBytes"]/*' />
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <include file="NftId.cs.xml" path='docs/member[@name="M:NftId.ToStringWithChecksum(Client)"]/*' />
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
            return HashCode.Combine(TokenId.Shard, TokenId.Realm, TokenId.Num, Serial);
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
			int tokenComparison = TokenId.CompareTo(o?.TokenId);

			if (tokenComparison != 0)
			{
				return tokenComparison;
			}

			return Serial.CompareTo(o?.Serial);
		}
	}
}