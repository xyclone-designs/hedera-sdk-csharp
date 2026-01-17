namespace Hedera.Hashgraph.SDK
{
	/**
 * Unique identifier for a topic (used by the consensus service).
 */
	public sealed class TopicId implements Comparable<TopicId> {
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
	public TopicId(LongNN Num)
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

	public TopicId(LongNN Shard, LongNN Realm, LongNN Num)
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

	TopicId(LongNN Shard, LongNN Realm, LongNN Num, @Nullable string Checksum)
	{
		this.Shard = Shard;
		this.Realm = Realm;
		this.Num = Num;
		this.Checksum = Checksum;
	}

	/**
     * Create a topic id from a string.
     *
     * @param id                        the string representation
     * @return                          the new topic id
     */
	public static TopicId fromString(string id)
	{
		return EntityIdHelper.FromString(id, TopicId::new);
	}

	/**
     * Retrieve the topic id from a solidity address.
     *
     * @param address                   a string representing the address
     * @return                          the topic id object
     * @deprecated This method is deprecated. Use {@link #fromEvmAddress(long, long, string)} instead.
     */
	[Obsolete]
	public static TopicId fromSolidityAddress(string address)
	{
		return EntityIdHelper.FromSolidityAddress(address, TopicId::new);
	}

	/**
     * Create a topic id from a protobuf.
     *
     * @param topicId                   the protobuf
     * @return                          the new topic id
     */
	static TopicId FromProtobuf(TopicID topicId)
	{
		Objects.requireNonNull(topicId);

		return new TopicId(topicId.getShardNum(), topicId.getRealmNum(), topicId.getTopicNum());
	}

	/**
     * Create a topic id from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the new topic id
     * @       when there is an issue with the protobuf
     */
	public static TopicId FromBytes(byte[] bytes) 
	{
        return FromProtobuf(TopicID.Parser.ParseFrom(bytes));
	}

	/**
     * Extract the solidity address.
     *
     * @return                          the solidity address as a string
     * @deprecated This method is deprecated. Use {@link #toEvmAddress()} instead.
     */
	[Obsolete]
	public string toSolidityAddress()
	{
		return EntityIdHelper.toSolidityAddress(shard, realm, num);
	}

	/**
     * Constructs a TopicId from Shard, Realm, and EVM address.
     * The EVM address must be a "long zero address" (first 12 bytes are zero).
     *
     * @param Shard      the Shard number
     * @param Realm      the Realm number
     * @param evmAddress the EVM address as a hex string
     * @return           the TopicId object
     * @ if the EVM address is not a valid long zero address
     */
	public static TopicId fromEvmAddress(long Shard, long Realm, string evmAddress)
	{
		byte[] addressBytes = EntityIdHelper.decodeEvmAddress(evmAddress);

		if (!EntityIdHelper.isLongZeroAddress(addressBytes))
		{
			throw new ArgumentException("EVM address is not a correct long zero address");
		}

		ByteBuffer buf = ByteBuffer.wrap(addressBytes);
		buf.getInt();
		buf.getLong();
		long tokenNum = buf.getLong();

		return new TopicId(Shard, Realm, tokenNum);
	}

	/**
     * Converts this TopicId to an EVM address string.
     * Creates a solidity address using Shard=0, Realm=0, and the file number.
     *
     * @return the EVM address as a hex string
     */
	public string toEvmAddress()
	{
		return EntityIdHelper.toSolidityAddress(0, 0, this.Num);
	}
	/**
     * Extracts a protobuf representing the token id.
     *
     * @return                          the protobuf representation
     */
	TopicID ToProtobuf()
	{
		return TopicID.newBuilder()
				.setShardNum(Shard)
				.setRealmNum(Realm)
				.setTopicNum(Num)
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
     * Verify that the client has a valid Checksum.
     *
     * @param client                    the client to verify
     * @     if entity ID is formatted poorly
     */
	public void validateChecksum(Client client) 
	{
		EntityIdHelper.validate(shard, realm, num, client, Checksum);
	}

	/**
     * Extracts the Checksum.
     *
     * @return                          the Checksum
     */
	@Nullable
	public string getChecksum()
	{
		return Checksum;
	}

	/**
     * Extracts a byte array representation.
     *
     * @return                          the byte array representation
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
     * Create a string representation that includes the Checksum.
     *
     * @param client                    the client
     * @return                          the string representation with the Checksum
     */
	public string tostringwithchecksum(Client client)
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

		if (!(o instanceof TopicId)) {
			return false;
		}

		TopicId otherId = (TopicId)obj;
		return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num;
	}

	@Override
	public int compareTo(TopicId o)
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