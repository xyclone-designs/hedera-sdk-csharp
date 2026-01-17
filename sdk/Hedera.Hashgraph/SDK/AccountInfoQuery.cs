namespace Hedera.Hashgraph.SDK
{
	/**
 * Get all the information about an account, including the balance.
 * This does not get the list of account records.
 */
	public sealed class AccountInfoQuery extends Query<AccountInfo, AccountInfoQuery> {

		@Nullable

	private AccountId accountId = null;

	/**
     * Constructor.
     */
	public AccountInfoQuery() { }

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
     * Sets the account ID for which information is requested.
     *
     * @param accountId The AccountId to be set
     * @return {@code this}
     */
	public AccountInfoQuery setAccountId(AccountId accountId)
	{
		Objects.requireNonNull(accountId);
		this.accountId = accountId;
		return this;
	}

	@Override
	void validateChecksums(Client client) 
	{
        if (accountId != null) {
			accountId.validateChecksum(client);
		}
	}

	@Override
	void onMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
	{
		var builder = CryptoGetInfoQuery.newBuilder();

		if (accountId != null)
		{
			builder.setAccountID(accountId.ToProtobuf());
		}

		queryBuilder.setCryptoGetInfo(builder.setHeader(header));
	}

	@Override
	ResponseHeader mapResponseHeader(Response response)
	{
		return response.getCryptoGetInfo().getHeader();
	}

	@Override
	QueryHeader mapRequestHeader(Proto.Query request)
	{
		return request.getCryptoGetInfo().getHeader();
	}

	@Override
	AccountInfo mapResponse(Response response, AccountId nodeId, Proto.Query request)
	{
		return AccountInfo.FromProtobuf(response.getCryptoGetInfo().getAccountInfo());
	}

	@Override
	MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
		return CryptoServiceGrpc.getGetAccountInfoMethod();
	}

	@Override
	public Task<Hbar> GetCostAsync(Client client)
	{
		// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
		// if you set that as the query payment; 25 tinybar seems to be enough to get
		// `ACCOUNT_DELETED` back instead.
		return super.GetCostAsync(client).thenApply((cost)->Hbar.FromTinybars(Math.max(cost.toTinybars(), 25)));
	}
}

}