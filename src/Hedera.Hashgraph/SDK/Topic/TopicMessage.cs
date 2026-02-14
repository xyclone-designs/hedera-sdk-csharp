// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Topic message records.
    /// </summary>
    public sealed class TopicMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lastConsensusTimestamp">the last consensus time</param>
        /// <param name="message">the message</param>
        /// <param name="lastRunningHash">the last running hash</param>
        /// <param name="lastSequenceNumber">the last sequence number</param>
        /// <param name="chunks">the array of chunks</param>
        /// <param name="transactionId">the transaction id</param>
        public TopicMessage(Timestamp lastConsensusTimestamp, byte[] message, byte[] lastRunningHash, ulong lastSequenceNumber, TopicMessageChunk[] chunks, TransactionId? transactionId)
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
        public static TopicMessage OfSingle(Proto.ConsensusTopicResponse response)
        {
            return new TopicMessage(
                Utils.TimestampConverter.FromProtobuf(response.ConsensusTimestamp), 
                response.Message.ToByteArray(), 
                response.RunningHash.ToByteArray(), 
                response.SequenceNumber, 
                [new(response)], 
                TransactionId.FromProtobuf(response.ChunkInfo.InitialTransactionID));
        }
        /// <summary>
        /// Create a new topic message from a list of response's protobuf.
        /// </summary>
        /// <param name="responses">the protobuf response</param>
        /// <returns>                         the new topic message</returns>
        public static TopicMessage OfMany(IList<Proto.ConsensusTopicResponse> responses)
        {
            // response should be in the order of oldest to newest (not chunk order)
            var chunks = new TopicMessageChunk[responses.Count];
            TransactionId? transactionId = null;
            var contents = new ByteString[responses.Count];
            long totalSize = 0;

            foreach (Proto.ConsensusTopicResponse r in responses)
            {
                transactionId ??= TransactionId.FromProtobuf(r.ChunkInfo.InitialTransactionID);

				int index = r.ChunkInfo.Number - 1;
                chunks[index] = new TopicMessageChunk(r);
                contents[index] = r.Message;
                totalSize += r.Message.Length;
            }

			byte[] wholeMessage = [.. contents.SelectMany(_ => _.ToByteArray())];
            var lastReceived = responses[responses.Count - 1];

            return new TopicMessage(
                Utils.TimestampConverter.FromProtobuf(lastReceived.ConsensusTimestamp), 
                wholeMessage, 
                lastReceived.RunningHash.ToByteArray(),
                lastReceived.SequenceNumber, 
                chunks, 
                transactionId);
        }

		/// <summary>
		/// The consensus timestamp of the message in seconds.nanoseconds
		/// </summary>
		public Timestamp ConsensusTimestamp { get; }
		/// <summary>
		/// The content of the message
		/// </summary>
		public byte[] Contents { get; }
		/// <summary>
		/// The new running hash of the topic that received the message
		/// </summary>
		public byte[] RunningHash { get; }
		/// <summary>
		/// The sequence number of the message relative to all other messages
		/// for the same topic
		/// </summary>
		public ulong SequenceNumber { get; }
		/// <summary>
		/// Array of topic message chunks.
		/// </summary>
		public TopicMessageChunk[] Chunks { get; }
		/// <summary>
		/// The transaction id
		/// </summary>
		public TransactionId? TransactionId { get; }
	}
}