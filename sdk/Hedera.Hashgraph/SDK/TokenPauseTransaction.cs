namespace Hedera.Hashgraph.SDK
{
	/**
 * Pause transaction activity for a token.
 *
 * This transaction MUST be signed by the Token `pause_key`.<br/>
 * The `token` identified MUST exist, and MUST NOT be deleted.<br/>
 * The `token` identified MAY be paused; if the token is already paused,
 * this transaction SHALL have no effect.
 * The `token` identified MUST have a `pause_key` set, the `pause_key` MUST be
 * a valid `Key`, and the `pause_key` MUST NOT be an empty `KeyList`.<br/>
 * A `paused` token SHALL NOT be transferred or otherwise modified except to
 * "up-pause" the token with `unpauseToken` or in a `rejectToken` transaction.
 *
 * ### Block Stream Effects
 * None
 */
public class TokenPauseTransaction extends Transaction<TokenPauseTransaction> {
    @Nullable
    private TokenId tokenId = null;

    /**
     * Constructor.
     */
    public TokenPauseTransaction() {}

    /**
     * Constructor.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction)
     *            records
     * @       when there is an issue with the protobuf
     */
    TokenPauseTransaction(
            LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs)
             {
        super(txs);
        initFromTransactionBody();
    }

    /**
     * Constructor.
     *
     * @param txBody protobuf TransactionBody
     */
    TokenPauseTransaction(Proto.TransactionBody txBody) {
        super(txBody);
        initFromTransactionBody();
    }

    /**
     * Extract the token id.
     *
     * @return                          the token id
     */
    @Nullable
    public TokenId getTokenId() {
        return tokenId;
    }

    /**
     * A token identifier.
     * <p>
     * The identified token SHALL be paused. Subsequent transactions
     * involving that token SHALL fail until the token is "unpaused".
     *
     * @param tokenId                   the token id
     * @return {@code this}
     */
    public TokenPauseTransaction setTokenId(TokenId tokenId) {
        Objects.requireNonNull(tokenId);
        requireNotFrozen();
        this.tokenId = tokenId;
        return this;
    }

    void initFromTransactionBody() {
        var body = sourceTransactionBody.getTokenPause();
        if (body.hasToken()) {
            tokenId = TokenId.FromProtobuf(body.getToken());
        }
    }

    /**
     * Build the transaction body.
     *
     * @return {@link
     *         Proto.TokenPauseTransactionBody}
     */
    TokenPauseTransactionBody.Builder build() {
        var builder = TokenPauseTransactionBody.newBuilder();
        if (tokenId != null) {
            builder.setToken(tokenId.ToProtobuf());
        }

        return builder;
    }

    @Override
    MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
        return TokenServiceGrpc.getPauseTokenMethod();
    }

    @Override
    void onFreeze(TransactionBody.Builder bodyBuilder) {
        bodyBuilder.setTokenPause(build());
    }

    @Override
    void onScheduled(SchedulableTransactionBody.Builder scheduled) {
        scheduled.setTokenPause(build());
    }

    @Override
    void validateChecksums(Client client)  {
        if (tokenId != null) {
            tokenId.validateChecksum(client);
        }
    }
}

}