// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Mirror.Api.Proto;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Topic message records.
    /// </summary>
    public sealed class TopicMessage
    {
        /// <summary>
        /// The consensus timestamp of the message in seconds.nanoseconds
        /// </summary>
        public readonly Timestamp ConsensusTimestamp;
        /// <summary>
        /// The content of the message
        /// </summary>
        public readonly byte[] Contents;
        /// <summary>
        /// The new running hash of the topic that received the message
        /// </summary>
        public readonly byte[] RunningHash;
        /// <summary>
        /// The sequence number of the message relative to all other messages
        /// for the same topic
        /// </summary>
        public readonly ulong SequenceNumber;
        /// <summary>
        /// Array of topic message chunks.
        /// </summary>
        public readonly TopicMessageChunk[] Chunks;
        /// <summary>
        /// The transaction id
        /// </summary>
        public readonly TransactionId TransactionId;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lastConsensusTimestamp">the last consensus time</param>
        /// <param name="message">the message</param>
        /// <param name="lastRunningHash">the last running hash</param>
        /// <param name="lastSequenceNumber">the last sequence number</param>
        /// <param name="chunks">the array of chunks</param>
        /// <param name="transactionId">the transaction id</param>
        public TopicMessage(Timestamp lastConsensusTimestamp, byte[] message, byte[] lastRunningHash, ulong lastSequenceNumber, TopicMessageChunk[] chunks, TransactionId transactionId)
        {
            ConsensusTimestamp = lastConsensusTimestamp;
            Contents = message;
            RunningHash = lastRunningHash;
            SequenceNumber = lastSequenceNumber;
            Chunks = chunks;
            TransactionId = transactionId;
        }

        /// <summary>
        /// Create a new topic message from a response protobuf.
        /// </summary>
        /// <param name="response">the protobuf response</param>
        /// <returns>                         the new topic message</returns>
        public static TopicMessage OfSingle(ConsensusTopicResponse response)
        {
            return new TopicMessage(Utils.TimestampConverter.FromProtobuf(
                response.ConsensusTimestamp), 
                response.Message.ToByteArray(), 
                response.RunningHash.ToByteArray(), 
                response.SequenceNumber, 
                [new(response)], 
                response.ChunkInfo?.InitialTransactionID is Proto.TransactionID protoTransactionID ? TransactionId.FromProtobuf(protoTransactionID) : null);
        }
        /// <summary>
        /// Create a new topic message from a list of response's protobuf.
        /// </summary>
        /// <param name="responses">the protobuf response</param>
        /// <returns>                         the new topic message</returns>
        public static TopicMessage OfMany(IList<ConsensusTopicResponse> responses)
        {
            // response should be in the order of oldest to newest (not chunk order)
            var chunks = new TopicMessageChunk[responses.Count];
            TransactionId? transactionId = null;
            var contents = new ByteString[responses.Count];
            long totalSize = 0;
            foreach (ConsensusTopicResponse r in responses)
            {
                if (transactionId == null && r.ChunkInfo.InitialTransactionID is not null)
                {
                    transactionId = TransactionId.FromProtobuf(r.ChunkInfo.InitialTransactionID);
                }

                int index = r.ChunkInfo.Number - 1;
                chunks[index] = new TopicMessageChunk(r);
                contents[index] = r.Message;
                totalSize += r.Message.Length;
            }

            var wholeMessage = ByteBuffer.Allocate((int)totalSize);
            foreach (var content in contents)
            {
                wholeMessage.Put(content.AsReadOnlyByteBuffer());
            }

            var lastReceived = responses[responses.Count - 1];
            return new TopicMessage(
                Utils.TimestampConverter.FromProtobuf(lastReceived.ConsensusTimestamp), 
                wholeMessage.Array(), 
                lastReceived.RunningHash.ToByteArray(),
                lastReceived.SequenceNumber, 
                chunks, 
                transactionId);
        }
    }
}