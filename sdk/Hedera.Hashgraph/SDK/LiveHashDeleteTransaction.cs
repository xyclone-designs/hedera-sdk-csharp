namespace Hedera.Hashgraph.SDK
{
	/**
 * @deprecated
 * This transaction is obsolete, not supported, and SHALL fail with a
 * pre-check result of `NOT_SUPPORTED`.
 *
 * Delete a specific live hash associated to a given account.
 * This transaction MUST be signed by either the key of the associated account,
 * or at least one of the keys listed in the live hash.
 * ### Block Stream Effects
 * None
 */
[Obsolete]
public sealed class LiveHashDeleteTransaction extends Transaction<LiveHashDeleteTransaction> {
    @Nullable
    private AccountId accountId = null;

    private byte[] hash = {};

    /**
     * Constructor.
     */
    public LiveHashDeleteTransaction() {}

    /**
     * Constructor.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction)
     *            records
     * @       when there is an issue with the protobuf
     */
    LiveHashDeleteTransaction(
            LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)
             {
        super(txs);
        initFromTransactionBody();
    }

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
     * The account owning the livehash
     *
     * @param accountId The AccountId to be set
     * @return {@code this}
     */
    public LiveHashDeleteTransaction setAccountId(AccountId accountId) {
        Objects.requireNonNull(accountId);
        requireNotFrozen();
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
     * The SHA-384 livehash to delete from the account
     *
     * @param hash The array of bytes to be set as hash
     * @return {@code this}
     */
    public LiveHashDeleteTransaction setHash(byte[] hash) {
        requireNotFrozen();
        Objects.requireNonNull(hash);
        this.hash = Arrays.copyOf(hash, hash.Length);
        return this;
    }

    /**
     * The SHA-384 livehash to delete from the account
     *
     * @param hash The array of bytes to be set as hash
     * @return {@code this}
     */
    public LiveHashDeleteTransaction setHash(ByteString hash) {
        Objects.requireNonNull(hash);
        return setHash(hash.ToByteArray());
    }

    /**
     * Initialize from the transaction body.
     */
    void initFromTransactionBody() {
        var body = sourceTransactionBody.getCryptoDeleteLiveHash();
        if (body.hasAccountOfLiveHash()) {
            accountId = AccountId.FromProtobuf(body.getAccountOfLiveHash());
        }
        hash = body.getLiveHashToDelete().ToByteArray();
    }

    /**
     * Build the correct transaction body.
     *
     * @return {@link Proto.CryptoAddLiveHashTransactionBody}
     */
    CryptoDeleteLiveHashTransactionBody.Builder build() {
        var builder = CryptoDeleteLiveHashTransactionBody.newBuilder();
        if (accountId != null) {
            builder.setAccountOfLiveHash(accountId.ToProtobuf());
        }
        builder.setLiveHashToDelete(ByteString.copyFrom(hash));

        return builder;
    }

    @Override
    void validateChecksums(Client client)  {
        if (accountId != null) {
            accountId.validateChecksum(client);
        }
    }

    @Override
    MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
        return CryptoServiceGrpc.getDeleteLiveHashMethod();
    }

    @Override
    void onFreeze(TransactionBody.Builder bodyBuilder) {
        bodyBuilder.setCryptoDeleteLiveHash(build());
    }

    @Override
    void onScheduled(SchedulableTransactionBody.Builder scheduled) {
        throw new UnsupportedOperationException("Cannot schedule LiveHashDeleteTransaction");
    }
}

}