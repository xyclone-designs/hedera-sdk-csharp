// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="T:TopicDeleteTransaction"]/*' />
    public sealed class TopicDeleteTransaction : Transaction<TopicDeleteTransaction>
    {
		/// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="M:TopicDeleteTransaction.#ctor"]/*' />
		public TopicDeleteTransaction() { }
		/// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="M:TopicDeleteTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TopicDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="M:TopicDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TopicDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="M:TopicDeleteTransaction.RequireNotFrozen"]/*' />
        public TopicId? TopicId
        {
            get; set { RequireNotFrozen(); field = value; }
        }

        /// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="M:TopicDeleteTransaction.InitFromTransactionBody"]/*' />
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ConsensusDeleteTopic;

			TopicId = TopicId.FromProtobuf(body.TopicID);
		}

        /// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="M:TopicDeleteTransaction.ToProtobuf"]/*' />
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
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.ConsensusService.ConsensusServiceClient.deleteTopic);

			return Proto.ConsensusService.Descriptor.FindMethodByName(methodname);
		}
		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}