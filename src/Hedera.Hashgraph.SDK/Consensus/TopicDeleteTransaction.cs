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
		/// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="M:TopicDeleteTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TopicDeleteTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="M:TopicDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TopicDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
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

			TopicId = TopicId.FromProtobuf(body.TopicId);
		}

        /// <include file="TopicDeleteTransaction.cs.xml" path='docs/member[@name="M:TopicDeleteTransaction.ToProtobuf"]/*' />
        public Proto.Services.ConsensusDeleteTopicTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ConsensusDeleteTopicTransactionBody();

            if (TopicId != null)
				builder.TopicId = TopicId.ToProtobuf();

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TopicId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.ConsensusDeleteTopic = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.ConsensusDeleteTopic = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.ConsensusService.ConsensusServiceClient.deleteTopic);

			return Proto.Services.ConsensusService.Descriptor.FindMethodByName(methodname);
		}
		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}
