namespace Hedera.Hashgraph.SDK
{
	/**
 * Get all the records for an account for any transfers into it and out of it,
 * that were above the threshold, during the last 25 hours.
 */
	public sealed class AccountRecordsQuery extends Query<List<TransactionRecord>, AccountRecordsQuery> {
		@Nullable

	private AccountId accountId = null;

	/**
     * Constructor.
     */
	public AccountRecordsQuery() { }

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
     * Sets the account ID for which the records should be retrieved.
     *
     * @param accountId The AccountId to be set
     * @return {@code this}
     */
	public AccountRecordsQuery setAccountId(AccountId accountId)
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
		var builder = CryptoGetAccountRecordsQuery.newBuilder();

		if (accountId != null)
		{
			builder.setAccountID(accountId.ToProtobuf());
		}

		queryBuilder.setCryptoGetAccountRecords(builder.setHeader(header));
	}

	@Override
	ResponseHeader mapResponseHeader(Response response)
	{
		return response.getCryptoGetAccountRecords().getHeader();
	}

	@Override
	QueryHeader mapRequestHeader(Proto.Query request)
	{
		return request.getCryptoGetAccountRecords().getHeader();
	}

	@Override
	List<TransactionRecord> mapResponse(
			Response response, AccountId nodeId, Proto.Query request) {
		var rawTransactionRecords = response.getCryptoGetAccountRecords().getRecordsList();
		var transactionRecords = new ArrayList<TransactionRecord>(rawTransactionRecords.size());

		for (var record : rawTransactionRecords)
		{
			transactionRecords.Add(TransactionRecord.FromProtobuf(record));
		}

		return transactionRecords;
	}

	@Override
	MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
		return CryptoServiceGrpc.getGetAccountRecordsMethod();
	}
}

}