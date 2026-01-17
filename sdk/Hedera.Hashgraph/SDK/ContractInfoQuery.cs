namespace Hedera.Hashgraph.SDK
{
	/**
 * Get information about a smart contract instance.
 * <p>
 * This includes the account that it uses, the file containing its bytecode,
 * and the time when it will expire.
 */
	public sealed class ContractInfoQuery extends Query<ContractInfo, ContractInfoQuery> {
		@Nullable

	private ContractId contractId = null;

	/**
     * Constructor.
     */
	public ContractInfoQuery() { }

	/**
     * Extract the contract id.
     *
     * @return                          the contract id
     */
	@Nullable
	public ContractId getContractId()
	{
		return contractId;
	}

	/**
     * Sets the contract ID for which information is requested.
     *
     * @param contractId The ContractId to be set
     * @return {@code this}
     */
	public ContractInfoQuery setContractId(ContractId contractId)
	{
		Objects.requireNonNull(contractId);
		this.contractId = contractId;
		return this;
	}

	@Override
	public Task<Hbar> GetCostAsync(Client client)
	{
		// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
		// if you set that as the query payment; 25 tinybar seems to be enough to get
		// `CONTRACT_DELETED` back instead.
		return super.GetCostAsync(client).thenApply((cost)->Hbar.FromTinybars(Math.max(cost.toTinybars(), 25)));
	}

	@Override
	void validateChecksums(Client client) 
	{
        if (contractId != null) {
			contractId.validateChecksum(client);
		}
	}

	@Override
	void onMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
	{
		var builder = ContractGetInfoQuery.newBuilder();
		if (contractId != null)
		{
			builder.setContractID(contractId.ToProtobuf());
		}

		queryBuilder.setContractGetInfo(builder.setHeader(header));
	}

	@Override
	ResponseHeader mapResponseHeader(Response response)
	{
		return response.getContractGetInfo().getHeader();
	}

	@Override
	QueryHeader mapRequestHeader(Proto.Query request)
	{
		return request.getContractGetInfo().getHeader();
	}

	@Override
	ContractInfo mapResponse(Response response, AccountId nodeId, Proto.Query request)
	{
		return ContractInfo.FromProtobuf(response.getContractGetInfo().getContractInfo());
	}

	@Override
	MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
		return SmartContractServiceGrpc.getGetContractInfoMethod();
	}
}

}