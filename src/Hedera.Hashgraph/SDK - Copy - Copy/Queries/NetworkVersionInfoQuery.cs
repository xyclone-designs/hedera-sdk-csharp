// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions.Account;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Information about the versions of protobuf and hedera.
    /// </summary>
    public class NetworkVersionInfoQuery : Query<NetworkVersionInfo, NetworkVersionInfoQuery>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public NetworkVersionInfoQuery(){}

		public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            queryBuilder.NetworkGetVersionInfo = new Proto.NetworkGetVersionInfoQuery
            {
				Header = header
			};
        }

		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.NetworkGetVersionInfo.Header;
        }

		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.NetworkGetVersionInfo.Header;
        }

        public override void ValidateChecksums(Client client) { }

		public override NetworkVersionInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return NetworkVersionInfo.FromProtobuf(response.NetworkGetVersionInfo);
        }

		public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return NetworkServiceGrpc.GetGetVersionInfoMethod();
        }
    }
}