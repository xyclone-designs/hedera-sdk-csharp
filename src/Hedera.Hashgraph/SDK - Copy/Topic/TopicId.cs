// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;
using System.IO;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Unique identifier for a topic (used by the consensus service).
    /// </summary>
    public sealed class TopicId : IComparable<TopicId>
    {
        /// <summary>
        /// The shard number
        /// </summary>
        public readonly long Shard;
        /// <summary>
        /// The realm number
        /// </summary>
        public readonly long Realm;
        /// <summary>
        /// The id number
        /// </summary>
        public readonly long Num;
        private readonly string Checksum;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="num">the num part
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</param>
        public TopicId(long num) : this(0, 0, num)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        public TopicId(long shard, long realm, long num) : this(shard, realm, num, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        /// <param name="checksum">the checksum</param>
        TopicId(long shard, long realm, long num, string checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
        }

        /// <summary>
        /// Create a topic id from a string.
        /// </summary>
        /// <param name="id">the string representation</param>
        /// <returns>                         the new topic id</returns>
        public static TopicId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new TopicId (a, b, c, d));
        }

        /// <summary>
        /// Retrieve the topic id from a solidity address.
        /// </summary>
        /// <param name="address">a string representing the address</param>
        /// <returns>                         the topic id object</returns>
        /// <remarks>@deprecatedThis method is deprecated. Use {@link #fromEvmAddress(long, long, String)} instead.</remarks>
        public static TopicId FromSolidityAddress(string address)
        {
            return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new TopicId (a, b, c, d));
        }

        /// <summary>
        /// Create a topic id from a protobuf.
        /// </summary>
        /// <param name="topicId">the protobuf</param>
        /// <returns>                         the new topic id</returns>
        public static TopicId FromProtobuf(Proto.TopicID topicId)
        {
            return new TopicId(topicId.ShardNum, topicId.RealmNum, topicId.TopicNum);
        }

        /// <summary>
        /// Create a topic id from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new topic id</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TopicId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.TopicID.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Extract the solidity address.
        /// </summary>
        /// <returns>                         the solidity address as a string</returns>
        /// <remarks>@deprecatedThis method is deprecated. Use {@link #toEvmAddress()} instead.</remarks>
        public string ToSolidityAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
        }

        /// <summary>
        /// Constructs a TopicId from shard, realm, and EVM address.
        /// The EVM address must be a "long zero address" (first 12 bytes are zero).
        /// </summary>
        /// <param name="shard">the shard number</param>
        /// <param name="realm">the realm number</param>
        /// <param name="evmAddress">the EVM address as a hex string</param>
        /// <returns>          the TopicId object</returns>
        /// <exception cref="IllegalArgumentException">if the EVM address is not a valid long zero address</exception>
        public static TopicId FromEvmAddress(long shard, long realm, string evmAddress)
        {
            byte[] addressBytes = Utils.EntityIdHelper.DecodeEvmAddress(evmAddress);
            if (!Utils.EntityIdHelper.IsLongZeroAddress(addressBytes))
            {
                throw new ArgumentException("EVM address is not a correct long zero address");
            }

			using MemoryStream ms = new(addressBytes);
			using BinaryReader reader = new(ms);

			reader.ReadInt32();
			reader.ReadInt64();

			long tokenNum = reader.ReadInt64();

			return new TopicId(shard, realm, tokenNum);
		}

        /// <summary>
        /// Converts this TopicId to an EVM address string.
        /// Creates a solidity address using shard=0, realm=0, and the file number.
        /// </summary>
        /// <returns>the EVM address as a hex string</returns>
        public string ToEvmAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(0, 0, Num);
        }

        /// <summary>
        /// Extracts a protobuf representing the token id.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        public Proto.TopicID ToProtobuf()
        {
            return new Proto.TopicID
            {
				ShardNum = Shard,
				RealmNum = Realm,
				TopicNum = Num
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="client">to validate against</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        /// <remarks>@deprecatedUse {@link #validateChecksum(Client)} instead.</remarks>
        public void Validate(Client client)
        {
            ValidateChecksum(client);
        }

        /// <summary>
        /// Verify that the client has a valid checksum.
        /// </summary>
        /// <param name="client">the client to verify</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        public void ValidateChecksum(Client client)
        {
            Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, checksum);
        }

        /// <summary>
        /// Extracts the checksum.
        /// </summary>
        /// <returns>                         the checksum</returns>
        public string GetChecksum()
        {
            return checksum;
        }

        /// <summary>
        /// Extracts a byte array representation.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override string ToString()
        {
            return Utils.EntityIdHelper.ToString(Shard, Realm, Num);
        }

        /// <summary>
        /// Create a string representation that includes the checksum.
        /// </summary>
        /// <param name="client">the client</param>
        /// <returns>                         the string representation with the checksum</returns>
        public string Tostringwithchecksum(Client client)
        {
            return Utils.EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, checksum);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Shard, Realm, Num);
        }

        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is TopicId))
            {
                return false;
            }

            TopicId otherId = (TopicId)o;
            return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num;
        }

        public int CompareTo(TopicId? o)
        {
            int shardComparison = Shard.CompareTo(o?.Shard);
            if (shardComparison != 0)
            {
                return shardComparison;
            }

            int realmComparison = Realm.CompareTo(o?.Realm);
            if (realmComparison != 0)
            {
                return realmComparison;
            }

            return Num.CompareTo(o?.Num);
        }
    }
}