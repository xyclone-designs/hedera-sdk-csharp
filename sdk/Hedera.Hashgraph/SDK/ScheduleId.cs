using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * The entity ID of a schedule transaction.
	 *
	 * See <a href="https://docs.hedera.com/guides/docs/sdks/schedule-transaction/schedule-id">Hedera Documentation</a>
	 */
	public sealed class ScheduleId : IComparable<ScheduleId> 
	{
		/**
		 * Constructor.
		 *
		 * @param Num                       the Num part
		 *
		 * Constructor that uses Shard, Realm and Num should be used instead
		 * as Shard and Realm should not assume 0 value
		 */
		[Obsolete]
		public ScheduleId(LongNN num) : this(0, 0, num) { }
		/**
		 * Constructor.
		 *
		 * @param Shard                     the Shard part
		 * @param Realm                     the Realm part
		 * @param Num                       the Num part
		 */
		public ScheduleId(LongNN shard, LongNN realm, LongNN num) : this(shard, realm, num, null) { }

		/**
		 * Constructor.
		 *
		 * @param Shard                     the Shard part
		 * @param Realm                     the Realm part
		 * @param Num                       the Num part
		 * @param Checksum                  the Checksum
		 */
		ScheduleId(LongNN shard, LongNN realm, LongNN num, string? checksum)
		{
			Shard = shard;
			Realm = realm;
			Num = num;
			Checksum = checksum;
		}

		/**
		 * Create a schedule id from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new schedule id
		 * @       when there is an issue with the protobuf
		 */
		public static ScheduleId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.ScheduleID.Parser.ParseFrom(bytes));
		}
		/**
		 * Create a schedule id from a string.
		 *
		 * @param id                        the string representing the schedule id
		 * @return                          the new schedule id
		 */
		public static ScheduleId FromString(string id)
		{
			return EntityIdHelper.FromString(id, (a, b, c, d) => new ScheduleId(a, b, c, d));
		}
		/**
		 * Create a schedule id from a protobuf.
		 *
		 * @param scheduleId                the protobuf
		 * @return                          the new schedule id
		 */
		public static ScheduleId FromProtobuf(Proto.ScheduleID scheduleId)
		{
			return new ScheduleId(scheduleId.ShardNum, scheduleId.RealmNum, scheduleId.ScheduleNum);
		}

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
		 * Create the byte array.
		 *
		 * @return                          byte array representation
		 */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/**
		 * Create the protobuf.
		 *
		 * @return                          the protobuf representing the schedule id
		 */
		public Proto.ScheduleID ToProtobuf()
		{
			return new Proto.ScheduleID
			{
				ShardNum = Shard,
				RealmNum = Realm,
				ScheduleNum = Num,
			};
		}
		public int CompareTo(ScheduleId? o)
		{
			if (o is null) return 1;

			if (Shard.CompareTo(o.Shard) is int shardComparison && shardComparison != 0)
				return shardComparison;

			if (Realm.CompareTo(o.Realm) is int realmComparison && realmComparison != 0)
				return realmComparison;

			return Num.CompareTo(o.Num);
		}
		/**
		 * Convert the schedule id into a string with Checksum.
		 *
		 * @param client                    the configured client
		 * @return                          the string representation
		 */
		public string ToStringWithChecksum(Client client)
		{
			return EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
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
			if (obj is not ScheduleId scheduleid) return false;

			return Shard == scheduleid.Shard && Realm == scheduleid.Realm && Num == scheduleid.Num;
		}
	}
}