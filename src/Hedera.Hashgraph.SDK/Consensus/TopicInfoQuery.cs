// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Grpc.Core;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Queries;

using System;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicInfoQuery.cs.xml" path='docs/member[@name="T:TopicInfoQuery"]/*' />
    public sealed class TopicInfoQuery : Query<TopicInfo, TopicInfoQuery>
    {
        public TopicId? TopicId { get; set; }

		public override void ValidateChecksums(Client client)
        {
			TopicId?.ValidateChecksum(client);
		}
		public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
        {
			queryBuilder.ConsensusGetTopicInfo = new Proto.Services.ConsensusGetTopicInfoQuery
			{
				Header = header,
			};

			if (TopicId != null)
				queryBuilder.ConsensusGetTopicInfo.TopicId = TopicId.ToProtobuf();
		}
		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
		{
			throw new NotImplementedException();
		}
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.ConsensusGetTopicInfo.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.ConsensusGetTopicInfo.Header;
        }
        public override TopicInfo MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
        {
			return TopicInfo.FromProtobuf(response.ConsensusGetTopicInfo);
		}
        public override Method<Proto.Services.Query, Proto.Services.Response> GetMethod()
        {
			throw new NotImplementedException();
        }
        public override MethodDescriptor GetMethodDescriptor()
        {
            throw new NotImplementedException();
        }
    }
}
