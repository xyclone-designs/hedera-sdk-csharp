namespace Hedera.Hashgraph.SDK
{
	/**
 * Retrieve the latest state of a topic.
 * <p>
 * This method is unrestricted and allowed on any topic by any payer account.
 */
	public sealed class TopicInfoQuery extends Query<TopicInfo, TopicInfoQuery> {
		@Nullable
		TopicId topicId = null;

	/**
     * Constructor.
     */
	public TopicInfoQuery() { }

	/**
     * Extract the topic id.
     *
     * @return                          the topic id
     */
	@Nullable
	public TopicId getTopicId()
	{
		return topicId;
	}

	/**
     * Set the topic to retrieve info about (the parameters and running state of).
     *
     * @param topicId The TopicId to be set
     * @return {@code this}
     */
	public TopicInfoQuery setTopicId(TopicId topicId)
	{
		Objects.requireNonNull(topicId);
		this.topicId = topicId;
		return this;
	}

	@Override
	void validateChecksums(Client client) 
	{
        if (topicId != null) {
			topicId.validateChecksum(client);
		}
	}

	@Override
	void onMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
	{
		var builder = ConsensusGetTopicInfoQuery.newBuilder();
		if (topicId != null)
		{
			builder.setTopicID(topicId.ToProtobuf());
		}

		queryBuilder.setConsensusGetTopicInfo(builder.setHeader(header));
	}

	@Override
	ResponseHeader mapResponseHeader(Response response)
	{
		return response.getConsensusGetTopicInfo().getHeader();
	}

	@Override
	QueryHeader mapRequestHeader(Proto.Query request)
	{
		return request.getConsensusGetTopicInfo().getHeader();
	}

	@Override
	TopicInfo mapResponse(Response response, AccountId nodeId, Proto.Query request)
	{
		return TopicInfo.FromProtobuf(response.getConsensusGetTopicInfo());
	}

	@Override
	MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
		return ConsensusServiceGrpc.getGetTopicInfoMethod();
	}
}

}