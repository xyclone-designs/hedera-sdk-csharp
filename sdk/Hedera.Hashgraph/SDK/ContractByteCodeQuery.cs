using System.Diagnostics.Contracts;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Get the bytecode for a smart contract instance.
 */
	public sealed class ContractByteCodeQuery extends Query<ByteString, ContractByteCodeQuery> {
		@Nullable

	private ContractId contractId = null;

	/**
     * Constructor.
     */
	public ContractByteCodeQuery() { }

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
	public ContractByteCodeQuery setContractId(ContractId contractId)
	{
		Objects.requireNonNull(contractId);
		this.contractId = contractId;
		return this;
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
		var builder = ContractGetBytecodeQuery.newBuilder();
		if (contractId != null)
		{
			builder.setContractID(contractId.ToProtobuf());
		}

		queryBuilder.setContractGetBytecode(builder.setHeader(header));
	}

	@Override
	ResponseHeader mapResponseHeader(Response response)
	{
		return response.getContractGetBytecodeResponse().getHeader();
	}

	@Override
	QueryHeader mapRequestHeader(Proto.Query request)
	{
		return request.getContractGetBytecode().getHeader();
	}

	@Override
	ByteString mapResponse(Response response, AccountId nodeId, Proto.Query request)
	{
		return response.getContractGetBytecodeResponse().getBytecode();
	}

	@Override
	MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
		return SmartContractServiceGrpc.getContractGetBytecodeMethod();
	}
}

}