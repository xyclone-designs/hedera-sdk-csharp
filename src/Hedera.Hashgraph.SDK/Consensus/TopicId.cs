// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;
using System.IO;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicId.cs.xml" path='docs/member[@name="T:TopicId"]/*' />
    public sealed class TopicId : Reference.Consensus.ITopicId<TopicId>, IComparable<TopicId>
    {
        private string? Checksum { get; }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="F:TopicId.Shard"]/*' />
        public long Shard { get; }
        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.#ctor(System.Int64)"]/*' />
        public long Realm { get; }
        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.#ctor(System.Int64)_2"]/*' />
        public long Num { get; }
        

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.#ctor(System.Int64)_3"]/*' />
        public TopicId(long num) : this(0, 0, num) { }
        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.#ctor(System.Int64,System.Int64,System.Int64)"]/*' />
        public TopicId(long shard, long realm, long num) : this(shard, realm, num, null) { }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.TopicId(System.Int64,System.Int64,System.Int64,System.String)"]/*' />
        TopicId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
        }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.FromString(System.String)"]/*' />
        public static TopicId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new TopicId (a, b, c, d));
        }
        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.FromBytes(System.Byte[])"]/*' />
        public static TopicId FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.Services.TopicID.Parser.ParseFrom(bytes));
        }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.FromSolidityAddress(System.String)"]/*' />
        public static TopicId FromSolidityAddress(string address)
        {
            return Utils.EntityIdHelper.FromSolidityAddress(address, (a, b, c, d) => new TopicId (a, b, c, d));
        }
        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.FromProtobuf(Proto.Services.TopicId)"]/*' />
        public static TopicId FromProtobuf(Proto.Services.TopicID topicId)
        {
            return new TopicId(topicId.ShardNum, topicId.RealmNum, topicId.TopicNum);
        }
        

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.ToSolidityAddress"]/*' />
        public string ToSolidityAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(Shard, Realm, Num);
        }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.FromEvmAddress(System.Int64,System.Int64,System.String)"]/*' />
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

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.ToEvmAddress"]/*' />
        public string ToEvmAddress()
        {
            return Utils.EntityIdHelper.ToSolidityAddress(0, 0, Num);
        }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.ToProtobuf"]/*' />
        public Proto.Services.TopicID ToProtobuf()
        {
            return new Proto.Services.TopicID
            {
				ShardNum = Shard,
				RealmNum = Realm,
				TopicNum = Num
            };
        }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.Validate(Client)"]/*' />
        public void Validate(Client client)
        {
            ValidateChecksum(client);
        }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.ValidateChecksum(Client)"]/*' />
        public void ValidateChecksum(Client client)
        {
            Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
        }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.ToBytes"]/*' />
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override string ToString()
        {
            return Utils.EntityIdHelper.ToString(Shard, Realm, Num);
        }

        /// <include file="TopicId.cs.xml" path='docs/member[@name="M:TopicId.Tostringwithchecksum(Client)"]/*' />
        public string Tostringwithchecksum(Client client)
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
