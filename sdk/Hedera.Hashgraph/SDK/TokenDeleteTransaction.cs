using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Deleting a token marks a token as deleted, though it will remain in the
 * ledger. The operation must be signed by the specified Admin Key of the
 * Token. If the Admin Key is not set, Transaction will result in
 * TOKEN_IS_IMMUTABlE. Once deleted update, mint, burn, wipe, freeze,
 * unfreeze, grant kyc, revoke kyc and token transfer transactions will
 * resolve to TOKEN_WAS_DELETED.
 *
 * See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/delete-a-token">Hedera Documentation</a>
 */
	public class TokenDeleteTransaction extends Transaction<TokenDeleteTransaction> {
		@Nullable

	private TokenId tokenId = null;

	/**
     * Constructor.
     */
	public TokenDeleteTransaction() { }

	/**
     * Constructor.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction)
     *            records
     * @       when there is an issue with the protobuf
     */
	TokenDeleteTransaction(
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
	TokenDeleteTransaction(Proto.TransactionBody txBody)
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
     * This SHALL identify the token type to delete.<br/>
     * The identified token MUST exist, and MUST NOT be deleted.
     *
     * @param tokenId                   the token id
     * @return {@code this}
     */
	public TokenDeleteTransaction setTokenId(TokenId tokenId)
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
		var body = sourceTransactionBody.getTokenDeletion();
		if (body.hasToken())
		{
			tokenId = TokenId.FromProtobuf(body.getToken());
		}
	}

	/**
     * Build the transaction body.
     *
     * @return {@link
     *         Proto.TokenDeleteTransactionBody}
     */
	TokenDeleteTransactionBody.Builder build()
	{
		var builder = TokenDeleteTransactionBody.newBuilder();
		if (tokenId != null)
		{
			builder.setToken(tokenId.ToProtobuf());
		}

		return builder;
	}

	@Override
	void validateChecksums(Client client) 
	{
        if (tokenId != null) {
			tokenId.validateChecksum(client);
		}
	}

	@Override
	MethodDescriptor<Transaction, TransactionResponse> getMethodDescriptor() {
		return TokenServiceGrpc.getDeleteTokenMethod();
	}

	@Override
	void onFreeze(TransactionBody.Builder bodyBuilder)
	{
		bodyBuilder.setTokenDeletion(build());
	}

	@Override
	void onScheduled(SchedulableTransactionBody.Builder scheduled)
	{
		scheduled.setTokenDeletion(build());
	}
}

}