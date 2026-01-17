namespace Hedera.Hashgraph.SDK
{
	/**
	 * Information about the versions of protobuf and hedera.
	 */
	public class NetworkVersionInfoQuery : Query<NetworkVersionInfo, NetworkVersionInfoQuery> 
	{
		public NetworkVersionInfoQuery() { }

		public override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
		{
			queryBuilder.setNetworkGetVersionInfo(
					NetworkGetVersionInfoQuery.newBuilder().setHeader(header));
		}

		
		public override Proto.ResponseHeader MapResponseHeader(Response response)
		{
			return response.getNetworkGetVersionInfo().getHeader();
		}


		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.getNetworkGetVersionInfo().getHeader();
		}


		public override void ValidateChecksums(Client client) 
		{
			// do nothing
		}
		public override Proto.NetworkVersionInfo MapResponse(Response response, AccountId nodeId, Proto.Query request)
		{
			return NetworkVersionInfo.FromProtobuf(response.getNetworkGetVersionInfo());
		}

		@Override
		MethodDescriptor<Proto.Query, Response> getMethodDescriptor() {
			return NetworkServiceGrpc.getGetVersionInfoMethod();
		}
	}
}