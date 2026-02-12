// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Grpc.Core;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Queries;

using System;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Retrieve the latest state of a topic.
    /// <p>
    /// This method is unrestricted and allowed on any topic by any payer account.
    /// </summary>
    public sealed class TopicInfoQuery : Query<TopicInfo, TopicInfoQuery>
    {
        public TopicId? TopicId { get; set; }

		public override void ValidateChecksums(Client client)
        {
			TopicId?.ValidateChecksum(client);
		}
		public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
			queryBuilder.ConsensusGetTopicInfo = new Proto.ConsensusGetTopicInfoQuery
			{
				Header = header,
			};

			if (TopicId != null)
				queryBuilder.ConsensusGetTopicInfo.TopicID = TopicId.ToProtobuf();
		}
		public override ResponseStatus MapResponseStatus(Proto.Response response)
		{
			throw new NotImplementedException();
		}
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.ConsensusGetTopicInfo.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.ConsensusGetTopicInfo.Header;
        }
        public override TopicInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
			return TopicInfo.FromProtobuf(response.ConsensusGetTopicInfo);
		}
        public override Method<Proto.Query, Proto.Response> GetMethod()
        {
			throw new NotImplementedException();
        }
        public override MethodDescriptor GetMethodDescriptor()
        {
            throw new NotImplementedException();
        }
    }
}