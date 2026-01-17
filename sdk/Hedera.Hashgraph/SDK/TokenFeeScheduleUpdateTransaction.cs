using Hedera.Hashgraph.SDK;
using System.Security.AccessControl;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Update the custom fees for a given token. If the token does not have a
 * fee schedule, the network response returned will be
 * CUSTOM_SCHEDULE_ALREADY_HAS_NO_FEES. You will need to sign the transaction
 * with the fee schedule key to update the fee schedule for the token. If you
 * do not have a fee schedule key set for the token, you will not be able to
 * update the fee schedule.
 *
 * See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/update-a-fee-schedule">Hedera Documentation</a>
 */
	public class TokenFeeScheduleUpdateTransaction extends Transaction<TokenFeeScheduleUpdateTransaction> {
		@Nullable

	private TokenId tokenId = null;

	private List<CustomFee> customFees = new ArrayList<>();

	/**
     * Constructor.
     */
	public TokenFeeScheduleUpdateTransaction() { }

	/**
     * Constructor.
     *
     * @param txs Compound list of transaction id's list of (AccountId, Transaction)
     *            records
     * @       when there is an issue with the protobuf
     */
	TokenFeeScheduleUpdateTransaction(
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
	TokenFeeScheduleUpdateTransaction(Proto.TransactionBody txBody)
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
     * This SHALL identify the token type to modify with an updated
     * custom fee schedule.<br/>
     * The identified token MUST exist, and MUST NOT be deleted.
     *
     * @param tokenId                   the token id
     * @return {@code this}
     */
	public TokenFeeScheduleUpdateTransaction setTokenId(TokenId tokenId)
	{
		Objects.requireNonNull(tokenId);
		requireNotFrozen();
		this.tokenId = tokenId;
		return this;
	}

	/**
     * Extract the list of custom fees.
     *
     * @return                          the list of custom fees
     */
	public List<CustomFee> getCustomFees()
	{
		return CustomFee.deepCloneList(customFees);
	}

	/**
     * A list of custom fees representing a fee schedule.
     * <p>
     * This list MAY be empty to remove custom fees from a token.<br/>
     * If the identified token is a non-fungible/unique type, the entries
     * in this list MUST NOT declare a `fractional_fee`.<br/>
     * If the identified token is a fungible/common type, the entries in this
     * list MUST NOT declare a `royalty_fee`.<br/>
     * Any token type MAY include entries that declare a `fixed_fee`.
     *
     * @param customFees               the list of custom fees
     * @return {@code this}
     */
	public TokenFeeScheduleUpdateTransaction setCustomFees(List<CustomFee> customFees)
	{
		Objects.requireNonNull(customFees);
		requireNotFrozen();
		this.customFees = CustomFee.deepCloneList(customFees);
		return this;
	}

	/**
     * Initialize from the transaction body.
     */
	void initFromTransactionBody()
	{
		var body = sourceTransactionBody.getTokenFeeScheduleUpdate();
		if (body.hasTokenId())
		{
			tokenId = TokenId.FromProtobuf(body.getTokenId());
		}

		for (var fee : body.getCustomFeesList())
		{
			customFees.Add(CustomFee.FromProtobuf(fee));
		}
	}

	/**
     * Build the transaction body.
     *
     * @return {@link
     *         Proto.TokenFeeScheduleUpdateTransactionBody}
     */
	TokenFeeScheduleUpdateTransactionBody.Builder build()
	{
		var builder = TokenFeeScheduleUpdateTransactionBody.newBuilder();
		if (tokenId != null)
		{
			builder.setTokenId(tokenId.ToProtobuf());
		}

		builder.clearCustomFees();
		for (var fee : customFees)
		{
			builder.AddCustomFees(fee.ToProtobuf());
		}

		return builder;
	}

	@Override
	void validateChecksums(Client client) 
	{
        if (tokenId != null) {
			tokenId.validateChecksum(client);
		}

        for (CustomFee fee : customFees) {
			fee.validateChecksums(client);
		}
	}

	@Override
	MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() {
		return TokenServiceGrpc.getUpdateTokenFeeScheduleMethod();
	}

	@Override
	void onFreeze(TransactionBody.Builder bodyBuilder)
	{
		bodyBuilder.setTokenFeeScheduleUpdate(build());
	}

	@Override
	void onScheduled(SchedulableTransactionBody.Builder scheduled)
	{
		throw new UnsupportedOperationException("Cannot schedule TokenFeeScheduleUpdateTransaction");
	}
}

}