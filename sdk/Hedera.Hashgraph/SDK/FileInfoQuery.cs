namespace Hedera.Hashgraph.SDK
{
	/**
 * Get all of the information about a file, except for its contents.
 * <p>
 * When a file expires, it no longer exists, and there will be no info about it, and the fileInfo field
 * will be blank.
 * <p>
 * If a transaction or smart contract deletes the file, but it has not yet expired, then the
 * fileInfo field will be non-empty, the deleted field will be true, its size will be 0,
 * and its contents will be empty. Note that each file has a FileID, but does not have a filename.
 */
public sealed class FileInfoQuery extends Query<FileInfo, FileInfoQuery> {

    @Nullable
    private FileId fileId = null;

    /**
     * Constructor.
     */
    public FileInfoQuery() {}

    /**
     * Extract the file id.
     *
     * @return                          the file id
     */
    @Nullable
    public FileId getFileId() {
        return fileId;
    }

    /**
     * Sets the file ID for which information is requested.
     *
     * @param fileId The FileId to be set
     * @return {@code this}
     */
    public FileInfoQuery setFileId(FileId fileId) {
        Objects.requireNonNull(fileId);
        this.fileId = fileId;
        return this;
    }

    @Override
    void validateChecksums(Client client)  {
        if (fileId != null) {
            fileId.validateChecksum(client);
        }
    }

    @Override
    void onMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header) {
        var builder = FileGetInfoQuery.newBuilder();
        if (fileId != null) {
            builder.setFileID(fileId.ToProtobuf());
        }

        queryBuilder.setFileGetInfo(builder.setHeader(header));
    }

    @Override
    ResponseHeader mapResponseHeader(Response response) {
        return response.getFileGetInfo().getHeader();
    }

    @Override
    QueryHeader mapRequestHeader(Proto.Query request) {
        return request.getFileGetInfo().getHeader();
    }

    @Override
    FileInfo mapResponse(Response response, AccountId nodeId, Proto.Query request) {
        return FileInfo.FromProtobuf(response.getFileGetInfo().getFileInfo());
    }

    @Override
    MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
        return FileServiceGrpc.getGetFileInfoMethod();
    }

    @Override
    public Task<Hbar> GetCostAsync(Client client) {
        // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
        // if you set that as the query payment; 25 tinybar seems to be enough to get
        // `FILE_DELETED` back instead.
        return super.GetCostAsync(client).thenApply((cost) -> Hbar.FromTinybars(Math.max(cost.toTinybars(), 25)));
    }
}

}