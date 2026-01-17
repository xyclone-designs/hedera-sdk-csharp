namespace Hedera.Hashgraph.SDK
{
	/**
 * Initializes the TokenInfoQuery object.
 */
	public class TokenInfoQuery extends Query<TokenInfo, TokenInfoQuery> {
		@Nullable
		TokenId tokenId = null;

	/**
     * Constructor.
     */
	public TokenInfoQuery() { }

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
     * Sets the Token ID for which information is requested.
     *
     * @param tokenId                           The TokenId to be set
     * @return {@code this}
     */
	public TokenInfoQuery setTokenId(TokenId tokenId)
	{
		Objects.requireNonNull(tokenId);
		this.tokenId = tokenId;
		return this;
	}

	@Override
	void validateChecksums(Client client) 
	{
        if (tokenId != null) {
			tokenId.validateChecksum(client);
		}
	}

	@Override
	void onMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
	{
		var builder = TokenGetInfoQuery.newBuilder();
		if (tokenId != null)
		{
			builder.setToken(tokenId.ToProtobuf());
		}

		queryBuilder.setTokenGetInfo(builder.setHeader(header));
	}

	@Override
	ResponseHeader mapResponseHeader(Response response)
	{
		return response.getTokenGetInfo().getHeader();
	}

	@Override
	QueryHeader mapRequestHeader(Proto.Query request)
	{
		return request.getTokenGetInfo().getHeader();
	}

	@Override
	TokenInfo mapResponse(Response response, AccountId nodeId, Query request)
	{
		return TokenInfo.FromProtobuf(response.getTokenGetInfo());
	}

	@Override
	MethodDescriptor<Query, Response> getMethodDescriptor() {
		return TokenServiceGrpc.getGetTokenInfoMethod();
	}

	@Override
	public Task<Hbar> GetCostAsync(Client client)
	{
		// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
		// if you set that as the query payment; 25 tinybar seems to be enough to get
		// `Token_DELETED` back instead.
		return super.GetCostAsync(client).thenApply((cost)->Hbar.FromTinybars(Math.max(cost.toTinybars(), 25)));
	}
}

}