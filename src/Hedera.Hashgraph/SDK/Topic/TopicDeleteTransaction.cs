// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Delete a topic.
    /// <p>
    /// No more transactions or queries on the topic will succeed.
    /// <p>
    /// If an {@code adminKey} is set, this transaction must be signed by that key.
    /// If there is no {@code adminKey}, this transaction will fail with {@link Status#UNAUTHORIZED}.
    /// </summary>
    public sealed class TopicDeleteTransaction : Transaction<TopicDeleteTransaction>
    {
		/// <summary>
		/// Constructor.
		/// </summary>
		public TopicDeleteTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		public TopicDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public TopicDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Set the topic ID to delete.
        /// </summary>
        /// <param name="topicId">The TopicId to be set</param>
        /// <returns>{@code this}</returns>
        public TopicId? TopicId
        {
            get; set { RequireNotFrozen(); field = value; }
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ConsensusDeleteTopic;

			TopicId = TopicId.FromProtobuf(body.TopicID);
		}

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ConsensusDeleteTopicTransactionBody}</returns>
        public Proto.ConsensusDeleteTopicTransactionBody ToProtobuf()
        {
            var builder = new Proto.ConsensusDeleteTopicTransactionBody();

            if (TopicId != null)
				builder.TopicID = TopicId.ToProtobuf();

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TopicId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ConsensusDeleteTopic = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ConsensusDeleteTopic = ToProtobuf();
        }
		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return ConsensusServiceGrpc.GetDeleteTopicMethod();
		}

        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new System.NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new System.NotImplementedException();
        }
    }
}