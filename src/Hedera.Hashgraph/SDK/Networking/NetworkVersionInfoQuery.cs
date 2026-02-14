// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Queries;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <summary>
    /// Information about the versions of protobuf and hedera.
    /// </summary>
    public class NetworkVersionInfoQuery : Query<NetworkVersionInfo, NetworkVersionInfoQuery>
    {
		public override void ValidateChecksums(Client client) { }
		public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            queryBuilder.NetworkGetVersionInfo = new Proto.NetworkGetVersionInfoQuery
            {
				Header = header
			};
        }
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.NetworkGetVersionInfo.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.NetworkGetVersionInfo.Header;
		}
		public override NetworkVersionInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
		{
			return NetworkVersionInfo.FromProtobuf(response.NetworkGetVersionInfo);
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.NetworkService.NetworkServiceClient.getVersionInfo);

			return Proto.NetworkService.Descriptor.FindMethodByName(methodname);
		}
	}
}