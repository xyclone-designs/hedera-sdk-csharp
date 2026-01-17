namespace Hedera.Hashgraph.SDK
{
	/**
 * @deprecated This query is no longer supported.
 * Requests a livehash associated to an account.
 */
[Obsolete]
public sealed class LiveHashQuery extends Query<LiveHash, LiveHashQuery> {
    @Nullable
    private AccountId accountId = null;

    private byte[] hash = {};

    /**
     * Constructor.
     */
    public LiveHashQuery() {}

    /**
     * Extract the account id.
     *
     * @return                          the account id
     */
    @Nullable
    public AccountId getAccountId() {
        return accountId;
    }

    /**
     * The account to which the livehash is associated
     *
     * @param accountId The AccountId to be set
     * @return {@code this}
     */
    public LiveHashQuery setAccountId(AccountId accountId) {
        Objects.requireNonNull(accountId);
        this.accountId = accountId;
        return this;
    }

    /**
     * Extract the hash.
     *
     * @return                          the hash
     */
    public ByteString getHash() {
        return ByteString.copyFrom(hash);
    }

    /**
     * The SHA-384 data in the livehash
     *
     * @param hash The array of bytes to be set as hash
     * @return {@code this}
     */
    public LiveHashQuery setHash(byte[] hash) {
        this.hash = Arrays.copyOf(hash, hash.Length);
        return this;
    }

    @Override
    void validateChecksums(Client client)  {
        if (accountId != null) {
            accountId.validateChecksum(client);
        }
    }

    @Override
    void onMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header) {
        var builder = CryptoGetLiveHashQuery.newBuilder();
        if (accountId != null) {
            builder.setAccountID(accountId.ToProtobuf());
        }
        builder.setHash(ByteString.copyFrom(hash));

        queryBuilder.setCryptoGetLiveHash(builder.setHeader(header));
    }

    @Override
    LiveHash mapResponse(Response response, AccountId nodeId, Proto.Query request) {
        return LiveHash.FromProtobuf(response.getCryptoGetLiveHash().getLiveHash());
    }

    @Override
    ResponseHeader mapResponseHeader(Response response) {
        return response.getCryptoGetLiveHash().getHeader();
    }

    @Override
    QueryHeader mapRequestHeader(Proto.Query request) {
        return request.getCryptoGetLiveHash().getHeader();
    }

    @Override
    MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
        return CryptoServiceGrpc.getCryptoGetBalanceMethod();
    }
}

}