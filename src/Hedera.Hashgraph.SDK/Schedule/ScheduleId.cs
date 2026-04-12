// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK.Schedule
{
    /// <include file="ScheduleId.cs.xml" path='docs/member[@name="T:ScheduleId"]/*' />
    public sealed class ScheduleId : IComparable<ScheduleId>
    {
        /// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.#ctor(System.Int64)"]/*' />
        public ScheduleId(long num) : this(0, 0, num) { }
        /// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.#ctor(System.Int64,System.Int64,System.Int64)"]/*' />
        public ScheduleId(long shard, long realm, long num) : this(shard, realm, num, null) { }
        /// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.#ctor(System.Int64,System.Int64,System.Int64,System.String)"]/*' />
        private ScheduleId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
        }

        /// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.FromString(System.String)"]/*' />
        public static ScheduleId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new ScheduleId(a, b, c, d));
        }
		/// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.FromBytes(System.Byte[])"]/*' />
		public static ScheduleId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.ScheduleID.Parser.ParseFrom(bytes));
		}
		/// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.FromProtobuf(Proto.Services.ScheduleID)"]/*' />
		public static ScheduleId FromProtobuf(Proto.Services.ScheduleID scheduleId)
        {
            return new ScheduleId(scheduleId.ShardNum, scheduleId.RealmNum, scheduleId.ScheduleNum);
        }

		/// <include file="ScheduleId.cs.xml" path='docs/member[@name="P:ScheduleId.Shard"]/*' />
		public long Shard { get; }
		/// <include file="ScheduleId.cs.xml" path='docs/member[@name="P:ScheduleId.Realm"]/*' />
		public long Realm { get; }
		/// <include file="ScheduleId.cs.xml" path='docs/member[@name="P:ScheduleId.Num"]/*' />
		public long Num { get; }
		public string? Checksum { get; }

		/// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.ToProtobuf"]/*' />
		public Proto.Services.ScheduleID ToProtobuf()
        {
            return new Proto.Services.ScheduleID
            {
				ShardNum = Shard,
				RealmNum = Realm,
				ScheduleNum = Num,
			};
        }

		public int CompareTo(ScheduleId? o)
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
		/// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.Validate(Client)"]/*' />
		public void Validate(Client client)
        {
            ValidateChecksum(client);
        }
        /// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.ValidateChecksum(Client)"]/*' />
        public void ValidateChecksum(Client client)
        {
            Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
        }
		/// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.ToStringWithChecksum(Client)"]/*' />
		public string ToStringWithChecksum(Client client)
		{
			return Utils.EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
		}

		/// <include file="ScheduleId.cs.xml" path='docs/member[@name="M:ScheduleId.ToBytes"]/*' />
		public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

		public override int GetHashCode()
		{
			return HashCode.Combine(Shard, Realm, Num);
		}
		public override string ToString()
        {
            return Utils.EntityIdHelper.ToString(Shard, Realm, Num);
        }
        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is ScheduleId))
            {
                return false;
            }

            ScheduleId otherId = (ScheduleId)o;
            return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num;
        }		
	}
}
