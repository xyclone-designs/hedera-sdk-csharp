// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Submit a message for consensus.
    /// <p>
    /// Valid and authorized messages on valid topics will be ordered by the consensus service, gossipped to the
    /// mirror net, and published (in order) to all subscribers (from the mirror net) on this topic.
    /// <p>
    /// The submitKey (if any) must sign this transaction.
    /// <p>
    /// On success, the resulting TransactionReceipt contains the topic's updated topicSequenceNumber and
    /// topicRunningHash.
    /// </summary>
    public sealed class TopicMessageSubmitTransaction : ChunkedTransaction<TopicMessageSubmitTransaction>
    {
		private IList<CustomFixedFee> _CustomFees = [];

		/// <summary>
		/// Constructor.
		/// </summary>
		public TopicMessageSubmitTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TopicMessageSubmitTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TopicMessageSubmitTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Assign the topic id.
        /// </summary>
        /// <param name="topicId">the topic id</param>
        /// <returns>{@code this}</returns>
        public TopicId? TopicId
        {
            get; set { RequireNotFrozen(); field = value; }
        }
        /// <summary>
        /// Extract the message.
        /// </summary>
        /// <returns>                         the message</returns>
        public ByteString Message
        {
            get; set { RequireNotFrozen(); field = value; }

        } = ByteString.Empty;
		/// <summary>
		/// The maximum custom fee that the user is willing to pay for the message. If left empty, the user is willing to pay any custom fee.
		/// If used with a transaction type that does not support custom fee limits, the transaction will fail.
		/// </summary>
		public IList<CustomFixedFee> CustomFees { get { RequireNotFrozen(); return _CustomFees; } set { RequireNotFrozen(); _CustomFees = value; } }
		public IReadOnlyList<CustomFixedFee> CustomFees_Read { get => _CustomFees.AsReadOnly(); }

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
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

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ConsensusSubmitMessageTransactionBody}</returns>
        public Proto.ConsensusSubmitMessageTransactionBody Build()
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
            bodyBuilder.ConsensusSubmitMessage = Build();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			scheduled.ConsensusSubmitMessage = Build();
			scheduled.ConsensusSubmitMessage.Message = Data;
		}
		public override void OnFreezeChunk(Proto.TransactionBody body, Proto.TransactionID initialTransactionId, int startIndex, int endIndex, int chunk, int total)
        {
            if (total == 1)
            {
                body.ConsensusSubmitMessage = Build();
                body.ConsensusSubmitMessage.Message = Data.Copy(startIndex, endIndex);
            }
            else
            {
                body.ConsensusSubmitMessage = Build();
				body.ConsensusSubmitMessage.Message = Data.Copy(startIndex, endIndex);
                body.ConsensusSubmitMessage.ChunkInfo = new Proto.ConsensusMessageChunkInfo
                {
					InitialTransactionID = initialTransactionId,
					Number = chunk + 1,
					Total = total,
				};
            }
        }

        public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return ConsensusServiceGrpc.GetSubmitMessageMethod();
		}

        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }

        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}