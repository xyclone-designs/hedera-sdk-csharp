// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;

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
        public NetworkVersionInfoQuery()
        {
        }

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            queryBuilder.SetNetworkGetVersionInfo(NetworkGetVersionInfoQuery.NewBuilder().SetHeader(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetNetworkGetVersionInfo().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetNetworkGetVersionInfo().GetHeader();
        }

        override void ValidateChecksums(Client client)
        {
        }

        override NetworkVersionInfo MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            return NetworkVersionInfo.FromProtobuf(response.GetNetworkGetVersionInfo());
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return NetworkServiceGrpc.GetGetVersionInfoMethod();
        }
    }
}