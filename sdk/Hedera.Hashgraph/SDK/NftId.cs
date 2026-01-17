using Google.Protobuf;
using System;
using System.Runtime.InteropServices;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * The (non-fungible) token of which this NFT is an instance
	 */
	public class NftId : IComparable<NftId> 
	{
		/**
		 * The (non-fungible) token of which this NFT is an instance
		 */
		public readonly TokenId TokenId;
		/**
		 * The unique identifier of this instance
		 */
		public readonly LongNN Serial;

		/**
		 * Constructor.
		 *
		 * @param TokenId                   the token id
		 * @param Serial                    the Serial number
		 */
		public NftId(TokenId tokenid, LongNN serial)
		{
			TokenId = tokenid;
			Serial = serial;
		}

		/**
		 * Create a new nft id from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new nft id
		 * @       when there is an issue with the protobuf
		 */
		public static NftId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.NftID.Parser.ParseFrom(bytes));
		}
		/**
		 * Create a new nft id from a string.
		 *
		 * @param id                        the string representation
		 * @return                          the new nft id
		 */
		public static NftId FromString(string id)
		{
			var parts = id.Split("[/@]");

			if (parts.Length != 2)
				throw new ArgumentException("Expecting {shardNum}.{realmNum}.{idNum}-{Checksum}/{SerialNum}");

			return new NftId(TokenId.FromString(parts[0]), LongNN.Parse(parts[1]));
		}
		/**
		 * Create a new ntf id from a protobuf.
		 *
		 * @param nftId                     the protobuf representation
		 * @return                          the new nft id
		 */
		public static NftId FromProtobuf(Proto.NftID nftId)
		{
			return new NftId(TokenId.FromProtobuf(nftId.TokenID), nftId.SerialNumber);
		}

		public int CompareTo(NftId? o)
		{
			int tokenComparison = TokenId.CompareTo(o?.TokenId);

			if (tokenComparison != 0) return tokenComparison;

			return LongNN.Compare(Serial, o?.Serial);
		}

		/**
		 * Create the byte array.
		 *
		 * @return                          a byte array representation
		 */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/**
		 * Create the protobuf.
		 *
		 * @return                          a protobuf representation
		 */
		public Proto.NftID ToProtobuf()
		{
			return new Proto.NftID
			{
				SerialNumber = Serial,
				TokenID = TokenId.ToProtobuf(),
			};
		}
		/**
		 * Generate a string representation with Checksum.
		 *
		 * @param client                    the configured client
		 * @return                          the string representation with Checksum
		 */
		public string ToStringWithChecksum(Client client)
		{
			return TokenId.toStringWithChecksum(client) + "/" + Serial;
		}

		public override string ToString()
		{
			return TokenId.toString() + "/" + Serial;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(TokenId.Shard, TokenId.Realm, TokenId.Num, Serial);
		}
		public override bool Equals(object? obj)
		{
			if (this == obj) return true;
			if (this is not NftId nftid) return false;

			return TokenId.Equals(nftid.TokenId) && Serial == nftid.Serial;
		}
    }

}