// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Queries;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <include file="NetworkVersionInfoQuery.cs.xml" path='docs/member[@name="T:NetworkVersionInfoQuery"]/*' />
    public class NetworkVersionInfoQuery : Query<NetworkVersionInfo, NetworkVersionInfoQuery>
    {
		public override void ValidateChecksums(Client client) { }
		public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
        {
            queryBuilder.NetworkGetVersionInfo = new Proto.Services.NetworkGetVersionInfoQuery
            {
				Header = header
			};
        }
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.NetworkGetVersionInfo.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.NetworkGetVersionInfo.Header;
		}
		public override NetworkVersionInfo MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
		{
			return NetworkVersionInfo.FromProtobuf(response.NetworkGetVersionInfo);
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.NetworkService.NetworkServiceClient.getVersionInfo);

			return Proto.Services.NetworkService.Descriptor.FindMethodByName(methodname);
		}
	}
}
