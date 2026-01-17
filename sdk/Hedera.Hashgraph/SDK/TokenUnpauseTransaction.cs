using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Resume transaction activity for a token.
 *
 * This transaction MUST be signed by the Token `pause_key`.<br/>
 * The `token` identified MUST exist, and MUST NOT be deleted.<br/>
 * The `token` identified MAY not be paused; if the token is not paused,
 * this transaction SHALL have no effect.
 * The `token` identified MUST have a `pause_key` set, the `pause_key` MUST be
 * a valid `Key`, and the `pause_key` MUST NOT be an empty `KeyList`.<br/>
 * An `unpaused` token MAY be transferred or otherwise modified.
 *
 * ### Block Stream Effects
 * None
 */
	public class TokenUnpauseTransaction extends Transaction<TokenUnpauseTransaction> {
		@Nullable

	private TokenId tokenId = null;

	/**
     * Constructor
     */
	public TokenUnpauseTransaction() { }

	/**
     * Constructor.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction)
     *            records
     * @       when there is an issue with the protobuf
     */
	TokenUnpauseTransaction(
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
	TokenUnpauseTransaction(Proto.TransactionBody txBody)
	{
		super(txBody);
		initFromTransactionBody();
	}

	/**
     * Extract the token id.
     *
     * @return                          the token id
     */
	@Nullable
	public TokenId getTokenId()
	{
		return tokenId;
	}

	/**
     * A token identifier.
     * <p>
     * The identified token SHALL be "unpaused". Subsequent transactions
     * involving that token MAY succeed.
     *
     * @param tokenId                   the token id
     * @return {@code this}
     */
	public TokenUnpauseTransaction setTokenId(TokenId tokenId)
	{
		Objects.requireNonNull(tokenId);
		requireNotFrozen();
		this.tokenId = tokenId;
		return this;
	}

	/**
     * Initialize from the transaction body.
     */
	void initFromTransactionBody()
	{
		var body = sourceTransactionBody.getTokenUnpause();
		if (body.hasToken())
		{
			tokenId = TokenId.FromProtobuf(body.getToken());
		}
	}

	/**
     * Build the transaction body.
     *
     * @return {@link
     *         Proto.TokenUnpauseTransactionBody}
     */
	TokenUnpauseTransactionBody.Builder build()
	{
		var builder = TokenUnpauseTransactionBody.newBuilder();
		if (tokenId != null)
		{
			builder.setToken(tokenId.ToProtobuf());
		}

		return builder;
	}

	@Override
	MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
		return TokenServiceGrpc.getUnpauseTokenMethod();
	}

	@Override
	void onFreeze(TransactionBody.Builder bodyBuilder)
	{
		bodyBuilder.setTokenUnpause(build());
	}

	@Override
	void onScheduled(SchedulableTransactionBody.Builder scheduled)
	{
		scheduled.setTokenUnpause(build());
	}

	@Override
	void validateChecksums(Client client) 
	{
        if (tokenId != null) {
			tokenId.validateChecksum(client);
		}
	}
}

}