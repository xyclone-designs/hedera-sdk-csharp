// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Topic
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
        private TopicId topicId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TopicDeleteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TopicDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TopicDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the topic id.
        /// </summary>
        /// <returns>                         the topic id</returns>
        public TopicId GetTopicId()
        {
            return topicId;
        }

        /// <summary>
        /// Set the topic ID to delete.
        /// </summary>
        /// <param name="topicId">The TopicId to be set</param>
        /// <returns>{@code this}</returns>
        public TopicDeleteTransaction SetTopicId(TopicId topicId)
        {
            ArgumentNullException.ThrowIfNull(topicId);
            RequireNotFrozen();
            topicId = topicId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ConsensusDeleteTopic;
            if (body.TopicID is not null)
            {
                topicId = TopicId.FromProtobuf(body.TopicID);
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ConsensusDeleteTopicTransactionBody}</returns>
        Proto.ConsensusDeleteTopicTransactionBody Build()
        {
            var builder = new Proto.ConsensusDeleteTopicTransactionBody();
            if (topicId != null)
            {
                builder.TopicID = topicId.ToProtobuf();
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            if (topicId != null)
            {
                topicId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return ConsensusServiceGrpc.GetDeleteTopicMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ConsensusDeleteTopic = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ConsensusDeleteTopic = Build();
        }
    }
}