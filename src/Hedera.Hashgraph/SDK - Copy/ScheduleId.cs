// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// The entity ID of a schedule transaction.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/schedule-transaction/schedule-id">Hedera Documentation</a>
    /// </summary>
    public sealed class ScheduleId : IComparable<ScheduleId>
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
        private readonly string? Checksum;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="num">the num part
        /// 
        /// Constructor that uses shard, realm and num should be used instead
        /// as shard and realm should not assume 0 value</param>
        public ScheduleId(long num) : this(0, 0, num) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        public ScheduleId(long shard, long realm, long num) : this(shard, realm, num, null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        /// <param name="checksum">the checksum</param>
        private ScheduleId(long shard, long realm, long num, string? checksum)
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            Checksum = checksum;
        }

        /// <summary>
        /// Create a schedule id from a string.
        /// </summary>
        /// <param name="id">the string representing the schedule id</param>
        /// <returns>                         the new schedule id</returns>
        public static ScheduleId FromString(string id)
        {
            return Utils.EntityIdHelper.FromString(id, (a, b, c, d) => new ScheduleId(a, b, c, d));
        }
		/// <summary>
		/// Create a schedule id from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new schedule id</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static ScheduleId FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.ScheduleID.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a schedule id from a protobuf.
		/// </summary>
		/// <param name="scheduleId">the protobuf</param>
		/// <returns>                         the new schedule id</returns>
		public static ScheduleId FromProtobuf(Proto.ScheduleID scheduleId)
        {
            return new ScheduleId(scheduleId.ShardNum, scheduleId.RealmNum, scheduleId.ScheduleNum);
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representing the schedule id</returns>
        public Proto.ScheduleID ToProtobuf()
        {
            return new Proto.ScheduleID
            {
				ShardNum = Shard,
				RealmNum = Realm,
				ScheduleNum = Num,
			};
        }

		/// <summary>
		/// Extract the checksum.
		/// </summary>
		/// <returns>                         the checksum</returns>
		public string GetChecksum()
		{
			return Checksum;
		}
		public int CompareTo(ScheduleId? o)
		{
			int shardComparison = Shard.CompareTo(o.Shard);
			if (shardComparison != 0)
			{
				return shardComparison;
			}

			int realmComparison = Realm.CompareTo(o.Realm);
			if (realmComparison != 0)
			{
				return realmComparison;
			}

			return Num.CompareTo(o.Num);
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
        /// Validate the configured client.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        public void ValidateChecksum(Client client)
        {
            Utils.EntityIdHelper.Validate(Shard, Realm, Num, client, Checksum);
        }
		/// <summary>
		/// Convert the schedule id into a string with checksum.
		/// </summary>
		/// <param name="client">the configured client</param>
		/// <returns>                         the string representation</returns>
		public string ToStringWithChecksum(Client client)
		{
			return Utils.EntityIdHelper.ToStringWithChecksum(Shard, Realm, Num, client, Checksum);
		}

		/// <summary>
		/// Create the byte array.
		/// </summary>
		/// <returns>                         byte array representation</returns>
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