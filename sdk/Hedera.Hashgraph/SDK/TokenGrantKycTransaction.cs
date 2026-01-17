namespace Hedera.Hashgraph.SDK
{
	/**
 * Grant "Know Your Customer"(KYC) for one account for a single token.
 *
 * This transaction MUST be signed by the `kyc_key` for the token.<br/>
 * The identified token MUST have a `kyc_key` set to a valid `Key` value.<br/>
 * The token `kyc_key` MUST NOT be an empty `KeyList`.<br/>
 * The identified token MUST exist and MUST NOT be deleted.<br/>
 * The identified account MUST exist and MUST NOT be deleted.<br/>
 * The identified account MUST have an association to the identified token.<br/>
 * On success the association between the identified account and the identified
 * token SHALL be marked as "KYC granted".
 *
 * ### Block Stream Effects
 * None
 */
	public class TokenGrantKycTransaction : Transaction<TokenGrantKycTransaction> 
	{
		@Nullable
		private TokenId tokenId = null;
		@Nullable
		private AccountId accountId = null;

		/**
		 * Configure.
		 */
		public TokenGrantKycTransaction() { }

		/**
		 * Constructor.
		 *
		 * @param txs Compound list of transaction id's list of (AccountId, Transaction)
		 *            records
		 * @       when there is an issue with the protobuf
		 */
		TokenGrantKycTransaction(
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
		TokenGrantKycTransaction(Proto.TransactionBody txBody)
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
		 * The identified token SHALL grant "KYC" for the account
		 * identified by the `account` field.<br/>
		 * The identified token MUST be associated to the account identified
		 * by the `account` field.
		 *
		 * @param tokenId                   the token id
		 * @return {@code this}
		 */
		public TokenGrantKycTransaction setTokenId(TokenId tokenId)
		{
			Objects.requireNonNull(tokenId);
			requireNotFrozen();
			this.tokenId = tokenId;
			return this;
		}

		/**
		 * Extract the account id.
		 *
		 * @return                          the account id
		 */
		@Nullable
		public AccountId getAccountId()
		{
			return accountId;
		}

		/**
		 * An account identifier.
		 * <p>
		 * The token identified by the `token` field SHALL grant "KYC" for the
		 * identified account.<br/>
		 * This account MUST be associated to the token identified
		 * by the `token` field.
		 *
		 * @param accountId                 the account id
		 * @return {@code this}
		 */
		public TokenGrantKycTransaction setAccountId(AccountId accountId)
		{
			Objects.requireNonNull(accountId);
			requireNotFrozen();
			this.accountId = accountId;
			return this;
		}

		/**
		 * Initialize from the transaction body.
		 */
		void initFromTransactionBody()
		{
			var body = sourceTransactionBody.getTokenGrantKyc();
			if (body.hasToken())
			{
				tokenId = TokenId.FromProtobuf(body.getToken());
			}

			if (body.hasAccount())
			{
				accountId = AccountId.FromProtobuf(body.getAccount());
			}
		}

		/**
		 * Build the transaction body.
		 *
		 * @return {@link
		 *         Proto.TokenGrantKycTransactionBody}
		 */
		TokenGrantKycTransactionBody.Builder build()
		{
			var builder = TokenGrantKycTransactionBody.newBuilder();
			if (tokenId != null)
			{
				builder.setToken(tokenId.ToProtobuf());
			}

			if (accountId != null)
			{
				builder.setAccount(accountId.ToProtobuf());
			}

			return builder;
		}

		public override void validateChecksums(Client client) 
		{
			if (tokenId != null) {
				tokenId.validateChecksum(client);
			}

			if (accountId != null) {
				accountId.validateChecksum(client);
			}
		}

		public override MethodDescriptor<Proto.Transaction, TransactionResponse> getMethodDescriptor() 
		{
			return TokenServiceGrpc.getFreezeTokenAccountMethod();
		}

		public override void onFreeze(TransactionBody.Builder bodyBuilder)
		{
			bodyBuilder.setTokenGrantKyc(build());
		}
		public override void onScheduled(SchedulableTransactionBody.Builder scheduled)
		{
			scheduled.setTokenGrantKyc(build());
		}
	}

}