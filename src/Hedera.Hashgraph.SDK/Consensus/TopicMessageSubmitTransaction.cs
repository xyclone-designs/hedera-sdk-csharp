// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicMessageSubmitTransaction.cs.xml" path='docs/member[@name="T:TopicMessageSubmitTransaction"]/*' />
    public sealed class TopicMessageSubmitTransaction : ChunkedTransaction<TopicMessageSubmitTransaction>
    {
		/// <include file="TopicMessageSubmitTransaction.cs.xml" path='docs/member[@name="M:TopicMessageSubmitTransaction.#ctor"]/*' />
		public TopicMessageSubmitTransaction() { }
		/// <include file="TopicMessageSubmitTransaction.cs.xml" path='docs/member[@name="M:TopicMessageSubmitTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TopicMessageSubmitTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TopicMessageSubmitTransaction.cs.xml" path='docs/member[@name="M:TopicMessageSubmitTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TopicMessageSubmitTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TopicMessageSubmitTransaction.cs.xml" path='docs/member[@name="M:TopicMessageSubmitTransaction.RequireNotFrozen"]/*' />
        public TopicId? TopicId
        {
            get; set { RequireNotFrozen(); field = value; }
        }
        /// <include file="TopicMessageSubmitTransaction.cs.xml" path='docs/member[@name="M:TopicMessageSubmitTransaction.RequireNotFrozen_2"]/*' />
        public ByteString Message
        {
            get; set { RequireNotFrozen(); field = value; }

        } = ByteString.Empty;
		/// <include file="TopicMessageSubmitTransaction.cs.xml" path='docs/member[@name="M:TopicMessageSubmitTransaction.InitFromTransactionBody"]/*' />
		public IList<CustomFeeLimit> CustomFeeLimits
		{
			init; get => field ??= new ListGuarded<CustomFeeLimit>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}

		/// <include file="TopicMessageSubmitTransaction.cs.xml" path='docs/member[@name="M:TopicMessageSubmitTransaction.InitFromTransactionBody_2"]/*' />
		void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ConsensusSubmitMessage;

            if (body.TopicID is not null)
				TopicId = TopicId.FromProtobuf(body.TopicID);

			if (InnerSignedTransactions.Count != 0)
            {
                try
                {
                    for (var i = 0; i < InnerSignedTransactions.Count; i += NodeAccountIds.Count == 0 ? 1 : NodeAccountIds.Count)
                    {
						Data = Data.Concat(Proto.TransactionBody.Parser.ParseFrom(InnerSignedTransactions[i].BodyBytes).ConsensusSubmitMessage.Message);
                    }
                }
                catch (InvalidProtocolBufferException exc)
                {
                    throw new ArgumentException(exc.Message);
                }
            }
            else
            {
				Data = body.Message;
            }
        }

        /// <include file="TopicMessageSubmitTransaction.cs.xml" path='docs/member[@name="M:TopicMessageSubmitTransaction.ToProtobuf"]/*' />
        public Proto.ConsensusSubmitMessageTransactionBody ToProtobuf()
        {
            var builder = new Proto.ConsensusSubmitMessageTransactionBody();

            if (TopicId != null)
				builder.TopicID = TopicId.ToProtobuf();

			builder.Message = Data;
            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            TopicId?.ValidateChecksum(client);
        }
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ConsensusSubmitMessage = ToProtobuf();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			scheduled.ConsensusSubmitMessage = ToProtobuf();
			scheduled.ConsensusSubmitMessage.Message = Data;
		}
        public override void OnFreezeChunk(Proto.TransactionBody body, Proto.TransactionID? initialTransactionId, int startIndex, int endIndex, int chunk, int total)
        {
            if (total == 1)
            {
                body.ConsensusSubmitMessage = ToProtobuf();
                body.ConsensusSubmitMessage.Message = Data.Copy(startIndex, endIndex);
            }
            else
            {
                body.ConsensusSubmitMessage = ToProtobuf();
				body.ConsensusSubmitMessage.Message = Data.Copy(startIndex, endIndex);
                body.ConsensusSubmitMessage.ChunkInfo = new Proto.ConsensusMessageChunkInfo
                {
					InitialTransactionID = initialTransactionId,
					Number = chunk + 1,
					Total = total,
				};
            }
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.ConsensusService.ConsensusServiceClient.submitMessage);

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