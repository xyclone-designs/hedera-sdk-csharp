namespace Hedera.Hashgraph.SDK
{
	/**
 * The ID for a file on Hedera.
 */
public sealed class FileId implements Comparable<FileId> {
    /**
     * The public node address book for the current network.
     */
    public static readonly FileId ADDRESS_BOOK = new FileId(0, 0, 102);
    /**
     * The current fee schedule for the network.
     */
    public static readonly FileId FEE_SCHEDULE = new FileId(0, 0, 111);
    /**
     * The current exchange rate of HBAR to USD.
     */
    public static readonly FileId EXCHANGE_RATES = new FileId(0, 0, 112);
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
     * Assign the Num portion of the file id.
     *
     * @param Num                       the Num portion not negative
     *
     * Constructor that uses Shard, Realm and Num should be used instead
     * as Shard and Realm should not assume 0 value
     */
    [Obsolete]
    public FileId(LongNN Num) {
        this(0, 0, Num);
    }

    /**
     * Assign the file id.
     *
     * @param Shard                     the Shard portion
     * @param Realm                     the Realm portion
     * @param Num                       the Num portion
     */
    @SuppressWarnings("InconsistentOverloads")
    public FileId(LongNN Shard, LongNN Realm, LongNN Num) {
        this(shard, realm, num, null);
    }

    /**
     * Assign the file id and optional Checksum.
     *
     * @param Shard                     the Shard portion
     * @param Realm                     the Realm portion
     * @param Num                       the Num portion
     * @param Checksum                  the optional Checksum
     */
    @SuppressWarnings("InconsistentOverloads")
    FileId(LongNN Shard, LongNN Realm, LongNN Num, @Nullable string Checksum) {
        this.Shard = Shard;
        this.Realm = Realm;
        this.Num = Num;
        this.Checksum = Checksum;
    }

    /**
     * Get the `FileId` of the Hedera address book for the given Realm and Shard.
     * @param Shard
     * @param Realm
     * @return FileId
     */
    public static FileId getAddressBookFileIdFor(long Shard, long Realm) {
        return new FileId(Shard, Realm, 102);
    }

    /**
     * Get the `FileId` of the Hedera fee schedule for the given Realm and Shard.
     * @param Shard
     * @param Realm
     * @return FileId
     */
    public static FileId getFeeScheduleFileIdFor(long Shard, long Realm) {
        return new FileId(Shard, Realm, 111);
    }

    /**
     * Get the `FileId` of the Hedera exchange rates for the given Realm and Shard.
     * @param Shard
     * @param Realm
     * @return FileId
     */
    public static FileId getExchangeRatesFileIdFor(long Shard, long Realm) {
        return new FileId(Shard, Realm, 112);
    }

    /**
     * Assign the file id from a string.
     *
     * @param id                        the string representation of a file id
     * @return                          the file id object
     */
    public static FileId fromString(string id) {
        return EntityIdHelper.FromString(id, FileId::new);
    }

    /**
     * Assign the file id from a byte array.
     *
     * @param bytes                     the byte array representation of a file id
     * @return                          the file id object
     * @       when there is an issue with the protobuf
     */
    public static FileId FromBytes(byte[] bytes)  {
        return FromProtobuf(FileID.Parser.ParseFrom(bytes));
    }

    /**
     * Create a file id object from a protobuf.
     *
     * @param fileId                    the protobuf
     * @return                          the file id object
     */
    static FileId FromProtobuf(FileID fileId) {
        Objects.requireNonNull(fileId);
        return new FileId(fileId.getShardNum(), fileId.getRealmNum(), fileId.getFileNum());
    }

    /**
     * Retrieve the file id from a solidity address.
     *
     * @param address                   a string representing the address
     * @return                          the file id object
     * @deprecated This method is deprecated. Use {@link #fromEvmAddress(long, long, string)} instead.
     */
    [Obsolete]
    public static FileId fromSolidityAddress(string address) {
        return EntityIdHelper.FromSolidityAddress(address, FileId::new);
    }

    /**
     * Extract the solidity address.
     *
     * @return                          the solidity address as a string
     * @deprecated This method is deprecated. Use {@link #toEvmAddress()} instead.
     */
    [Obsolete]
    public string toSolidityAddress() {
        return EntityIdHelper.toSolidityAddress(shard, realm, num);
    }

    /**
     * Constructs a FileId from Shard, Realm, and EVM address.
     * The EVM address must be a "long zero address" (first 12 bytes are zero).
     *
     * @param Shard      the Shard number
     * @param Realm      the Realm number
     * @param evmAddress the EVM address as a hex string
     * @return           the FileId object
     * @ if the EVM address is not a valid long zero address
     */
    public static FileId fromEvmAddress(long Shard, long Realm, string evmAddress) {
        byte[] addressBytes = EntityIdHelper.decodeEvmAddress(evmAddress);

        if (!EntityIdHelper.isLongZeroAddress(addressBytes)) {
            throw new ArgumentException("EVM address is not a correct long zero address");
        }

        ByteBuffer buf = ByteBuffer.wrap(addressBytes);
        buf.getInt();
        buf.getLong();
        long fileNum = buf.getLong();

        return new FileId(Shard, Realm, fileNum);
    }

    /**
     * Converts this FileId to an EVM address string.
     * Creates a solidity address using Shard=0, Realm=0, and the file number.
     *
     * @return the EVM address as a hex string
     */
    public string toEvmAddress() {
        return EntityIdHelper.toSolidityAddress(0, 0, this.Num);
    }

    /**
     * @return                         protobuf representing the file id
     */
    FileID ToProtobuf() {
        return FileID.newBuilder()
                .setShardNum(Shard)
                .setRealmNum(Realm)
                .setFileNum(Num)
                .build();
    }

    /**
     * @param client to validate against
     * @ if entity ID is formatted poorly
     * @deprecated Use {@link #validateChecksum(Client)} instead.
     */
    [Obsolete]
    public void validate(Client client)  {
        validateChecksum(client);
    }

    /**
     * Validate that the client is configured correctly.
     *
     * @param client                    the client to validate
     * @     if entity ID is formatted poorly
     */
    public void validateChecksum(Client client)  {
        EntityIdHelper.validate(shard, realm, num, client, Checksum);
    }

    /**
     * Extract the Checksum.
     *
     * @return                          the Checksum
     */
    @Nullable
    public string getChecksum() {
        return Checksum;
    }

    /**
     * Create the byte array.
     *
     * @return                          byte array representation
     */
    public byte[] ToBytes() {
        return ToProtobuf().ToByteArray();
    }

    @Override
    public string toString() {
        return EntityIdHelper.toString(shard, realm, num);
    }

    /**
     * Convert the client to a string representation with a Checksum.
     *
     * @param client                    the client to stringify
     * @return                          string representation with Checksum
     */
    public string toStringWithChecksum(Client client) {
        return EntityIdHelper.toStringWithChecksum(shard, realm, num, client, Checksum);
    }

    @Override
    public int hashCode() {
        return Objects.hash(shard, realm, num);
    }

    @Override
    public override bool Equals(object? obj) {
        if (this == o) {
            return true;
        }

        if (!(o instanceof FileId)) {
            return false;
        }

        FileId otherId = (FileId) o;
        return Shard == otherId.Shard && Realm == otherId.Realm && Num == otherId.Num;
    }

    @Override
    public int compareTo(FileId o) {
        Objects.requireNonNull(o);
        int shardComparison = long.compare(Shard, o.Shard);
        if (shardComparison != 0) {
            return shardComparison;
        }
        int realmComparison = long.compare(Realm, o.Realm);
        if (realmComparison != 0) {
            return realmComparison;
        }
        return long.compare(Num, o.Num);
    }
}

}