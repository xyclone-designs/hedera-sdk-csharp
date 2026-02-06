// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Transactions.Topic
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
        private TopicId topicId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TopicMessageSubmitTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TopicMessageSubmitTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TopicMessageSubmitTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// Assign the topic id.
        /// </summary>
        /// <param name="topicId">the topic id</param>
        /// <returns>{@code this}</returns>
        public TopicMessageSubmitTransaction SetTopicId(TopicId topicId)
        {
            ArgumentNullException.ThrowIfNull(topicId);
            RequireNotFrozen();
            topicId = topicId;
            return this;
        }

        /// <summary>
        /// Extract the message.
        /// </summary>
        /// <returns>                         the message</returns>
        public ByteString GetMessage()
        {
            return GetData();
        }

        /// <summary>
        /// Assign the message from a byte string.
        /// </summary>
        /// <param name="message">the byte string</param>
        /// <returns>                         the message</returns>
        public TopicMessageSubmitTransaction SetMessage(ByteString message)
        {
            return SetData(message);
        }

        /// <summary>
        /// Assign the message from a byte array.
        /// </summary>
        /// <param name="message">the byte array</param>
        /// <returns>                         the message</returns>
        public TopicMessageSubmitTransaction SetMessage(byte[] message)
        {
            return SetData(message);
        }

        /// <summary>
        /// Assign the message from a string.
        /// </summary>
        /// <param name="message">the string</param>
        /// <returns>                         the message</returns>
        public TopicMessageSubmitTransaction SetMessage(string message)
        {
            return SetData(message);
        }

        /// <summary>
        /// Extract the custom fee limits of the transaction
        /// </summary>
        /// <returns>the custom fee limits of the transaction</returns>
        public IList<CustomFeeLimit> GetCustomFeeLimits()
        {
            return customFeeLimits;
        }

        /// <summary>
        /// The maximum custom fee that the user is willing to pay for the message. If left empty, the user is willing to pay any custom fee.
        /// If used with a transaction type that does not support custom fee limits, the transaction will fail.
        /// </summary>
        public TopicMessageSubmitTransaction SetCustomFeeLimits(IList<CustomFeeLimit> customFeeLimits)
        {
            ArgumentNullException.ThrowIfNull(customFeeLimits);
            RequireNotFrozen();
            customFeeLimits = customFeeLimits;
            return this;
        }

        /// <summary>
        /// Adds a custom fee limit
        /// </summary>
        /// <param name="customFeeLimit"></param>
        /// <returns>{@code this}</returns>
        public TopicMessageSubmitTransaction AddCustomFeeLimit(CustomFeeLimit customFeeLimit)
        {
            ArgumentNullException.ThrowIfNull(customFeeLimit);
            RequireNotFrozen();
            if (customFeeLimits != null)
            {
                customFeeLimits.Add(customFeeLimit);
            }

            return this;
        }

        /// <summary>
        /// Clears all custom fee limits.
        /// </summary>
        /// <returns>{@code this}</returns>
        public TopicMessageSubmitTransaction ClearCustomFeeLimits()
        {
            RequireNotFrozen();
            if (customFeeLimits != null)
            {
                customFeeLimits.Clear();
            }

            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ConsensusSubmitMessage;
            if (body.TopicID is not null)
            {
                topicId = TopicId.FromProtobuf(body.TopicID);
            }

            if (InnerSignedTransactions.Count != 0)
            {
                try
                {
                    for (var i = 0; i < InnerSignedTransactions.Count; i += nodeAccountIds.Length == 0 ? 1 : nodeAccountIds.Count)
                    {
                        data = data.Concat(Proto.TransactionBody.Parser.ParseFrom(InnerSignedTransactions[i].BodyBytes).ConsensusSubmitMessage.Message);
                    }
                }
                catch (InvalidProtocolBufferException exc)
                {
                    throw new ArgumentException(exc.Message);
                }
            }
            else
            {
                data = body.Message;
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ConsensusSubmitMessageTransactionBody}</returns>
        Proto.ConsensusSubmitMessageTransactionBody Build()
        {
            var builder = new Proto.ConsensusSubmitMessageTransactionBody();
            if (topicId != null)
            {
                builder.TopicID = topicId.ToProtobuf();
            }

            builder.Message = data;
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
            return ConsensusServiceGrpc.GetSubmitMessageMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ConsensusSubmitMessage = Build();
        }

        override void OnFreezeChunk(Proto.TransactionBody body, Proto.TransactionID initialTransactionId, int startIndex, int endIndex, int chunk, int total)
        {
            if (total == 1)
            {
                body.ConsensusSubmitMessage = Build();
                body.ConsensusSubmitMessage.Message = data[startIndex .. endIndex];
            }
            else
            {
                body.ConsensusSubmitMessage = Build();
				body.ConsensusSubmitMessage.Message = data[startIndex..endIndex];
                body.ConsensusSubmitMessage.ChunkInfo = new Proto.ConsensusMessageChunkInfo
                {
					InitialTransactionID = initialTransactionId,
					Number = chunk + 1,
					Total = total,
				};
            }
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ConsensusSubmitMessage = Build();
            scheduled.ConsensusSubmitMessage.Message = data;
        }
    }
}