using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Constructs a TokenId.
	 *
	 * See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/token-id">Hedera Documentation</a>
	 */
	public class TokenId : IComparable<TokenId> 
	{
		/**
		* The Shard number
		*/
		public LongNN Shard { get; }
		/**
		 * The Realm number
		 */
		public LongNN Realm { get; }
		/**
		 * The id number
		 */
		public LongNN Num { get; }
		public string? Checksum { get; }

		/**
		 * Constructor.
		 *
		 * @param Num                       the Num part
		 *
		 * Constructor that uses Shard, Realm and Num should be used instead
		 * as Shard and Realm should not assume 0 value
		 */
		[Obsolete]
		public TokenId(LongNN Num) : this(0, 0, Num) { }
		/**
		 * Constructor.
		 *
		 * @param Shard                     the Shard part
		 * @param Realm                     the Realm part
		 * @param Num                       the Num part
		 */
		public TokenId(LongNN Shard, LongNN Realm, LongNN Num) : this(Shard, Realm, Num, null) { }
		/**
		 * Constructor.
		 *
		 * @param Shard                     the Shard part
		 * @param Realm                     the Realm part
		 * @param Num                       the Num part
		 * @param Checksum                  the Checksum
		 */
		TokenId(LongNN Shard, LongNN Realm, LongNN Num, string? Checksum)
		{
			Shard = Shard;
			Realm = Realm;
			Num = Num;
			Checksum = Checksum;
		}


		/**
		 * Create a token id from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new token id
		 * @       when there is an issue with the protobuf
		 */
		public static TokenId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TokenID.Parser.ParseFrom(bytes));
		}
		/**
		 * Create a token id from a string.
		 *
		 * @param id                        the string representation
		 * @return                          the new token id
		 */
		public static TokenId FromString(string id)
		{
			return EntityIdHelper.FromString(id, (a, b, c, d) => new TokenId(a, b, c, d));
		}
		/**
		 * Create a token id from a protobuf.
		 *
		 * @param tokenId                   the protobuf
		 * @return                          the new token id
		 */
		public static TokenId FromProtobuf(Proto.TokenID tokenId)
		{
			return new TokenId(tokenId.ShardNum, tokenId.RealmNum, tokenId.TokenNum);
		}
		/**
		 * Retrieve the token id from a solidity address.
		 *
		 * @param address                   a string representing the address
		 * @return                          the token id object
		 * @deprecated This method is deprecated. Use {@link #fromEvmAddress(LongNN, LongNN, string)} instead.
		 */
		[Obsolete]
		public static TokenId FromSolidityAddress(string address)
		{
			return EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new TokenId(a, b, c, d));
		}
		/**
		 * Constructs a TokenID from Shard, Realm, and EVM address.
		 * The EVM address must be a "LongNN zero address" (first 12 bytes are zero).
		 *
		 * @param Shard      the Shard number
		 * @param Realm      the Realm number
		 * @param evmAddress the EVM address as a hex string
		 * @return           the TokenID object
		 * @ if the EVM address is not a valid LongNN zero address
		 */
		public static TokenId FromEvmAddress(LongNN Shard, LongNN Realm, string evmAddress)
		{
			byte[] addressBytes = EntityIdHelper.DecodeEvmAddress(evmAddress);

			if (EntityIdHelper.IsLongZeroAddress(addressBytes) is false)
				throw new ArgumentException("EVM address is not a correct LongNN zero address");

			ByteBuffer buf = ByteBuffer.wrap(addressBytes);
			buf.getInt();
			buf.getLong();
			LongNN tokenNum = buf.getLong();

			return new TokenId(Shard, Realm, tokenNum);
		}

		/**
		 * Converts this TokenId to an EVM address string.
		 * Creates a solidity address using Shard=0, Realm=0, and the file number.
		 *
		 * @return the EVM address as a hex string
		 */
		public string ToEvmAddress()
		{
			return EntityIdHelper.ToSolidityAddress(0, 0, Num);
		}

		/**
		 * Create an nft id.
		 *
		 * @param serial                    the serial number
		 * @return                          the new nft id
		 */
		public NftId Nft(LongNN serial)
		{
			return new NftId(this, serial);
		}

		/**
		 * Extract the solidity address.
		 *
		 * @return                          the solidity address as a string
		 * @deprecated This method is deprecated. Use {@link #toEvmAddress()} instead.
		 */
		[Obsolete]
		public string ToSolidityAddress()
		{
			return EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
		}

		/**
		 * Create the protobuf.
		 *
		 * @return                          a protobuf representation
		 */
		public Proto.TokenID ToProtobuf()
		{
			return new Proto.TokenID
			{
				ShardNum = Shard,
				RealmNum = Realm,
				TokenNum = Num,
			};
		}

		/**
		 * @param client to validate against
		 * @ if entity ID is formatted poorly
		 * @deprecated Use {@link #validateChecksum(Client)} instead.
		 */
		[Obsolete]
		public void Validate(Client client) 
		{
			ValidateChecksum(client);
		}
		/**
		 * Validate the configured client.
		 *
		 * @param client                    the configured client
		 * @     if entity ID is formatted poorly
		 */
		public void ValidateChecksum(Client client) 
		{
			EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
		}


		/**
		 * Create the byte array.
		 *
		 * @return                          the byte array representation
		 */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		public int CompareTo(TokenId? o)
		{
			if (o is null) return 0;

			if (LongNN.Compare(Shard, o.Shard) is int shardComparison && shardComparison != 0) return shardComparison;
			if (LongNN.Compare(Realm, o.Realm) is int realmComparison && realmComparison != 0) return realmComparison;

			return LongNN.Compare(Num, o.Num);
		}


		/**
		 * Create a string representation with Checksum.
		 *
		 * @param client                    the configured client
		 * @return                          the string representation with Checksum
		 */
		public string ToStringWithChecksum(Client client)
		{
			return EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
		}

		public override string ToString()
		{
			return EntityIdHelper.ToString(Shard, Realm, Num);
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(Shard, Realm, Num);
		}
		public override bool Equals(object? obj)
		{
			if (this == obj) return true;
			if (this is not TokenId tokenid) return false;

			return Shard == tokenid.Shard && Realm == tokenid.Realm && Num == tokenid.Num;
		}
	}
}