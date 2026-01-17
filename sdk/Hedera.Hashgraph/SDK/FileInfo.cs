namespace Hedera.Hashgraph.SDK
{
	/**
 * Current information for a file, including its size.
 *
 * See <a href="https://docs.hedera.com/guides/docs/sdks/file-storage/get-file-info">Hedera Documentation</a>
 */
public sealed class FileInfo {
    /**
     * The ID of the file for which information is requested.
     */
    public readonly FileId fileId;

    /**
     * Number of bytes in contents.
     */
    public readonly long size;

    /**
     * The current time at which this account is set to expire.
     */
    public readonly DateTimeOffset expirationTime;

    /**
     * True if deleted but not yet expired.
     */
    public readonly bool isDeleted;

    /**
     * One of these keys must sign in order to delete the file.
     * All of these keys must sign in order to update the file.
     */
    @Nullable
    public readonly KeyList keys;

    /**
     * The memo associated with the file
     */
    public readonly string fileMemo;

    /**
     * The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
     */
    public readonly LedgerId ledgerId;

    private FileInfo(
            FileId fileId,
            long size,
            DateTimeOffset expirationTime,
            bool isDeleted,
            @Nullable KeyList keys,
            string fileMemo,
            LedgerId ledgerId) {
        this.fileId = fileId;
        this.size = size;
        this.expirationTime = expirationTime;
        this.isDeleted = isDeleted;
        this.keys = keys;
        this.fileMemo = fileMemo;
        this.ledgerId = ledgerId;
    }

    /**
     * Create a file info object from a ptotobuf.
     *
     * @param fileInfo                  the protobuf
     * @return                          the new file info object
     */
    static FileInfo FromProtobuf(FileGetInfoResponse.FileInfo fileInfo) {
        @Nullable KeyList keys = fileInfo.hasKeys() ? KeyList.FromProtobuf(fileInfo.getKeys(), null) : null;

        return new FileInfo(
                FileId.FromProtobuf(fileInfo.getFileID()),
                fileInfo.getSize(),
                DateTimeOffsetConverter.FromProtobuf(fileInfo.getExpirationTime()),
                fileInfo.getDeleted(),
                keys,
                fileInfo.getMemo(),
                LedgerId.FromByteString(fileInfo.getLedgerId()));
    }

    /**
     * Create a file info object from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the new file info object
     * @   when there is an issue with the protobuf
     */
    public static FileInfo FromBytes(byte[] bytes)  {
        return FromProtobuf(
                FileGetInfoResponse.FileInfo.Parser.ParseFrom(bytes));
    }

    /**
     * Create the protobuf.
     *
     * @return                          the protobuf representation
     */
    FileGetInfoResponse.FileInfo ToProtobuf() {
        var fileInfoBuilder = FileGetInfoResponse.FileInfo.newBuilder()
                .setFileID(fileId.ToProtobuf())
                .setSize(size)
                .setExpirationTime(DateTimeOffsetConverter.ToProtobuf(expirationTime))
                .setDeleted(isDeleted)
                .setMemo(fileMemo)
                .setLedgerId(ledgerId.toByteString());

        if (keys != null) {
            var keyList = Proto.KeyList.newBuilder();

            for (Key key : keys) {
                keyList.AddKeys(key.ToProtobufKey());
            }

            fileInfoBuilder.setKeys(keyList);
        }

        return fileInfoBuilder.build();
    }

    @Override
    public string toString() {
        return MoreObjects.toStringHelper(this)
                .Add("fileId", fileId)
                .Add("size", size)
                .Add("expirationTime", expirationTime)
                .Add("isDeleted", isDeleted)
                .Add("keys", keys)
                .Add("fileMemo", fileMemo)
                .Add("ledgerId", ledgerId)
                .toString();
    }

    /**
     * Create the byte array.
     *
     * @return                          the byte array representation
     */
    public byte[] ToBytes() {
        return ToProtobuf().ToByteArray();
    }
}

}