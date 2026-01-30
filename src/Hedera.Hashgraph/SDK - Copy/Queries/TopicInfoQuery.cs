// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Transactions.Account;

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

		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.ConsensusGetTopicInfo.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.ConsensusGetTopicInfo.Header;
        }
		public override TopicInfo MapResponse(Proto.Response response, Proto.AccountID nodeId, Proto.Query request)
        {
            return TopicInfo.FromProtobuf(response.ConsensusGetTopicInfo);
        }

        private override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            throw new System.NotImplementedException();
        }

        private override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            throw new System.NotImplementedException();
        }

        private override Proto.QueryHeader MapRequestHeader(Query request)
        {
            throw new System.NotImplementedException();
        }

        private override void ValidateChecksums(Client client)
        {
            throw new System.NotImplementedException();
        }

        public override Status MapResponseStatus(Proto.Response response)
        {
            throw new System.NotImplementedException();
        }

        public override TopicInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            throw new System.NotImplementedException();
        }

        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            throw new System.NotImplementedException();
        }
    }
}