using System;
using System.Runtime.InteropServices;

namespace Hedera.Hashgraph.SDK
{
	/**
 * The entity ID of a schedule transaction.
 *
 * See <a href="https://docs.hedera.com/guides/docs/sdks/schedule-transaction/schedule-id">Hedera Documentation</a>
 */
	public sealed class ScheduleId implements Comparable<ScheduleId> {
		/**
		 * The Shard number
		 */
		@Nonnegative

	public readonly long Shard;

	/**
     * The Realm number
     */
	@Nonnegative
	public readonly long Realm;

	/**
     * The id number
     */
	@Nonnegative
	public readonly long Num;

	@Nullable
	private readonly string Checksum;

    /**
     * Constructor.
     *
     * @param Num                       the Num part
     *
     * Constructor that uses Shard, Realm and Num should be used instead
     * as Shard and Realm should not assume 0 value
     */
    [Obsolete]
	public ScheduleId(LongNN Num)
	{
		this(0, 0, Num);
	}

	/**
     * Constructor.
     *
     * @param Shard                     the Shard part
     * @param Realm                     the Realm part
     * @param Num                       the Num part
     */
	@SuppressWarnings("InconsistentOverloads")

	public ScheduleId(LongNN Shard, LongNN Realm, LongNN Num)
	{
		this(shard, realm, num, null);
	}

	/**
     * Constructor.
     *
     * @param Shard                     the Shard part
     * @param Realm                     the Realm part
     * @param Num                       the Num part
     * @param Checksum                  the Checksum
     */
	@SuppressWarnings("InconsistentOverloads")

	ScheduleId(LongNN Shard, LongNN Realm, LongNN Num, @Nullable string Checksum)
	{
		this.Shard = Shard;
		this.Realm = Realm;
		this.Num = Num;
		this.Checksum = Checksum;
	}

	/**
     * Create a schedule id from a string.
     *
     * @param id                        the string representing the schedule id
     * @return                          the new schedule id
     */
	public static ScheduleId fromString(string id)
	{
		return EntityIdHelper.FromString(id, ScheduleId::new);
	}

	/**
     * Create a schedule id from a protobuf.
     *
     * @param scheduleId                the protobuf
     * @return                          the new schedule id
     */
	static ScheduleId FromProtobuf(ScheduleID scheduleId)
	{
		Objects.requireNonNull(scheduleId);
		return new ScheduleId(scheduleId.getShardNum(), scheduleId.getRealmNum(), scheduleId.getScheduleNum());
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
        return FromProtobuf(ScheduleID.Parser.ParseFrom(bytes));
	}

	/**
     * Create the protobuf.
     *
     * @return                          the protobuf representing the schedule id
     */
	ScheduleID ToProtobuf()
	{
		return ScheduleID.newBuilder()
				.setShardNum(Shard)
				.setRealmNum(Realm)
				.setScheduleNum(Num)
				.build();
	}

	/**
     * @param client to validate against
     * @ if entity ID is formatted poorly
     * @deprecated Use {@link #validateChecksum(Client)} instead.
     */
	[Obsolete]
	public void validate(Client client) 
	{
		validateChecksum(client);
	}

	/**
     * Validate the configured client.
     *
     * @param client                    the configured client
     * @     if entity ID is formatted poorly
     */
	public void validateChecksum(Client client) 
	{
		EntityIdHelper.validate(shard, realm, num, client, Checksum);
	}

	/**
     * Extract the Checksum.
     *
     * @return                          the Checksum
     */
	@Nullable
	public string getChecksum()
	{
		return Checksum;
	}

	/**
     * Create the byte array.
     *
     * @return                          byte array representation
     */
	public byte[] ToBytes()
	{
		return ToProtobuf().ToByteArray();
	}

	@Override
	public string toString()
	{
		return EntityIdHelper.toString(shard, realm, num);
	}

	/**
     * Convert the schedule id into a string with Checksum.
     *
     * @param client                    the configured client
     * @return                          the string representation
     */
	public string toStringWithChecksum(Client client)
	{
		return EntityIdHelper.toStringWithChecksum(shard, realm, num, client, Checksum);
	}

	@Override
	public int hashCode()
	{
		return Objects.hash(shard, realm, num);
	}

	@Override
	public override bool Equals(object? obj)
	{
		if (this == o)
		{
			return true;
		}

		if (!(o instanceof ScheduleId)) {
			return false;
		}

		ScheduleId otherId = (ScheduleId)obj;
		return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num;
	}

	@Override
	public int compareTo(ScheduleId o)
	{
		Objects.requireNonNull(o);
		int shardComparison = long.compare(Shard, o.Shard);
		if (shardComparison != 0)
		{
			return shardComparison;
		}
		int realmComparison = long.compare(Realm, o.Realm);
		if (realmComparison != 0)
		{
			return realmComparison;
		}
		return long.compare(Num, o.Num);
	}
}

}