// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

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
        TopicMessageSubmitTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
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
            Objects.RequireNonNull(topicId);
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
            Objects.RequireNonNull(customFeeLimits);
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
            Objects.RequireNonNull(customFeeLimit);
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
            var body = sourceTransactionBody.GetConsensusSubmitMessage();
            if (body.HasTopicID())
            {
                topicId = TopicId.FromProtobuf(body.GetTopicID());
            }

            if (!innerSignedTransactions.IsEmpty())
            {
                try
                {
                    for (var i = 0; i < innerSignedTransactions.Count; i += nodeAccountIds.IsEmpty() ? 1 : nodeAccountIds.Count)
                    {
                        data = data.Concat(TransactionBody.ParseFrom(innerSignedTransactions[i].GetBodyBytes()).GetConsensusSubmitMessage().GetMessage());
                    }
                }
                catch (InvalidProtocolBufferException exc)
                {
                    throw new ArgumentException(exc.GetMessage());
                }
            }
            else
            {
                data = body.GetMessage();
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ConsensusSubmitMessageTransactionBody}</returns>
        ConsensusSubmitMessageTransactionBody.Builder Build()
        {
            var builder = ConsensusSubmitMessageTransactionBody.NewBuilder();
            if (topicId != null)
            {
                builder.SetTopicID(topicId.ToProtobuf());
            }

            builder.SetMessage(data);
            return builder;
        }

        override void ValidateChecksums(Client client)
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

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetConsensusSubmitMessage(Build());
        }

        override void OnFreezeChunk(TransactionBody.Builder body, TransactionID initialTransactionId, int startIndex, int endIndex, int chunk, int total)
        {
            if (total == 1)
            {
                body.SetConsensusSubmitMessage(Build().SetMessage(data.Substring(startIndex, endIndex)));
            }
            else
            {
                body.SetConsensusSubmitMessage(Build().SetMessage(data.Substring(startIndex, endIndex)).SetChunkInfo(ConsensusMessageChunkInfo.NewBuilder().SetInitialTransactionID(Objects.RequireNonNull(initialTransactionId)).SetNumber(chunk + 1).SetTotal(total)));
            }
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetConsensusSubmitMessage(Build().SetMessage(data));
        }
    }
}