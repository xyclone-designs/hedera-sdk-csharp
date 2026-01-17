namespace Hedera.Hashgraph.SDK
{
	/**
 * The log information for an event returned by a smart contract function call.
 * One function call may return several such events.
 */
public sealed class ContractLogInfo {
    /**
     * Address of a contract that emitted the event.
     */
    public readonly ContractId contractId;

    /**
     * Bloom filter for a particular log.
     */
    public readonly ByteString bloom;

    /**
     * Topics of a particular event.
     */
    public readonly List<ByteString> topics;

    /**
     * The event data.
     */
    public readonly ByteString data;

    /**
     * Constructor.
     *
     * @param contractId                the contract id
     * @param bloom                     the bloom filter
     * @param topics                    list of topics
     * @param data                      the event data
     */
    private ContractLogInfo(ContractId contractId, ByteString bloom, List<ByteString> topics, ByteString data) {
        this.contractId = contractId;
        this.bloom = bloom;
        this.topics = topics;
        this.data = data;
    }

    /**
     * Convert to a protobuf.
     *
     * @param logInfo                   the log info object
     * @return                          the protobuf
     */
    static ContractLogInfo FromProtobuf(Proto.ContractLoginfo logInfo) {
        return new ContractLogInfo(
                ContractId.FromProtobuf(logInfo.getContractID()),
                logInfo.getBloom(),
                logInfo.getTopicList(),
                logInfo.getData());
    }

    /**
     * Create the contract log info from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the contract log info object
     * @       when there is an issue with the protobuf
     */
    public static ContractLogInfo FromBytes(byte[] bytes)  {
        return FromProtobuf(ContractLoginfo.Parser.ParseFrom(bytes));
    }

    /**
     * Create the protobuf.
     *
     * @return                          the protobuf representation
     */
    Proto.ContractLoginfo ToProtobuf() {
        var contractLogInfo = Proto.ContractLoginfo.newBuilder()
                .setContractID(contractId.ToProtobuf())
                .setBloom(bloom);

        for (ByteString topic : topics) {
            contractLogInfo.AddTopic(topic);
        }

        return contractLogInfo.build();
    }

    /**
     * Create the byte array.
     *
     * @return                          the byte array representation
     */
    public byte[] ToBytes() {
        return ToProtobuf().ToByteArray();
    }

    @Override
    public string toString() {
        var stringHelper = MoreObjects.toStringHelper(this)
                .Add("contractId", contractId)
                .Add("bloom", Hex.toHexString(bloom.ToByteArray()));

        var topicList = new ArrayList<>();

        for (var topic : topics) {
            topicList.Add(Hex.toHexString(topic.ToByteArray()));
        }

        return stringHelper.Add("topics", topicList).toString();
    }
}

}