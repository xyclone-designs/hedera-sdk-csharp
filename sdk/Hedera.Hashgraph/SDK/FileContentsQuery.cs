namespace Hedera.Hashgraph.SDK
{
	/**
 * Get the contents of a file. The content field is empty (no bytes) if the
 * file is empty.
 *
 * A query to get the contents of a file. Queries do not change the state of
 * the file or require network consensus. The information is returned from a
 * single node processing the query.
 *
 * See <a href="https://docs.hedera.com/guides/docs/sdks/file-storage/get-file-contents">Hedera Documentation</a>
 */
public sealed class FileContentsQuery extends Query<ByteString, FileContentsQuery> {

    @Nullable
    private FileId fileId = null;

    /**
     * Constructor.
     */
    public FileContentsQuery() {}

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
     * Sets the file ID of the file whose contents are requested.
     *
     * @param fileId The FileId to be set
     * @return {@code this}
     */
    public FileContentsQuery setFileId(FileId fileId) {
        Objects.requireNonNull(fileId);
        this.fileId = fileId;
        return this;
    }

    @Override
    public Task<Hbar> GetCostAsync(Client client) {
        // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
        // if you set that as the query payment; 25 tinybar seems to be enough to get
        // `FILE_DELETED` back instead.
        return super.GetCostAsync(client).thenApply((cost) -> Hbar.FromTinybars(Math.max(cost.toTinybars(), 25)));
    }

    @Override
    void validateChecksums(Client client)  {
        if (fileId != null) {
            fileId.validateChecksum(client);
        }
    }

    @Override
    void onMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header) {
        var builder = FileGetContentsQuery.newBuilder();
        if (fileId != null) {
            builder.setFileID(fileId.ToProtobuf());
        }

        queryBuilder.setFileGetContents(builder.setHeader(header));
    }

    @Override
    ResponseHeader mapResponseHeader(Response response) {
        return response.getFileGetContents().getHeader();
    }

    @Override
    QueryHeader mapRequestHeader(Proto.Query request) {
        return request.getFileGetContents().getHeader();
    }

    @Override
    ByteString mapResponse(Response response, AccountId nodeId, Proto.Query request) {
        return response.getFileGetContents().getFileContents().getContents();
    }

    @Override
    MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
        return FileServiceGrpc.getGetFileContentMethod();
    }
}

}