// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.Proto.Services;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicMessage.cs.xml" path='docs/member[@name="T:TopicMessage"]/*' />
    public sealed class TopicMessage
    {
        /// <include file="TopicMessage.cs.xml" path='docs/member[@name="M:TopicMessage.#ctor(DateTimeOffset,System.Byte[],System.Byte[],System.UInt64,TopicMessageChunk[],TransactionId)"]/*' />
        public TopicMessage(DateTimeOffset lastConsensusTimestamp, byte[] message, byte[] lastRunningHash, ulong lastSequenceNumber, TopicMessageChunk[] chunks, TransactionId? transactionId)
        {
            ConsensusTimestamp = lastConsensusTimestamp;
            Contents = message;
            RunningHash = lastRunningHash;
            SequenceNumber = lastSequenceNumber;
            Chunks = chunks;
            TransactionId = transactionId;
        }

        /// <include file="TopicMessage.cs.xml" path='docs/member[@name="M:TopicMessage.OfSingle(Proto.Mirror.ConsensusTopicResponse)"]/*' />
        public static TopicMessage OfSingle(Proto.Mirror.ConsensusTopicResponse response)
        {
            return new TopicMessage(
                response.ConsensusTimestamp.ToDateTimeOffset(), 
                response.Message.ToByteArray(), 
                response.RunningHash.ToByteArray(), 
                response.SequenceNumber, 
                [new(response)], 
                TransactionId.FromProtobuf(response.ChunkInfo.InitialTransactionId));
        }
        /// <include file="TopicMessage.cs.xml" path='docs/member[@name="M:TopicMessage.OfMany(System.Collections.Generic.IList{Proto.Mirror.ConsensusTopicResponse})"]/*' />
        public static TopicMessage OfMany(IList<Proto.Mirror.ConsensusTopicResponse> responses)
        {
            // response should be in the order of oldest to newest (not chunk order)
            var chunks = new TopicMessageChunk[responses.Count];
            TransactionId? transactionId = null;
            var contents = new ByteString[responses.Count];
            long totalSize = 0;

            foreach (Proto.Mirror.ConsensusTopicResponse r in responses)
            {
                transactionId ??= TransactionId.FromProtobuf(r.ChunkInfo.InitialTransactionId);

				int index = r.ChunkInfo.Number - 1;
                chunks[index] = new TopicMessageChunk(r);
                contents[index] = r.Message;
                totalSize += r.Message.Length;
            }

			byte[] wholeMessage = [.. contents.SelectMany(_ => _.ToByteArray())];
            var lastReceived = responses[responses.Count - 1];

            return new TopicMessage(
                lastReceived.ConsensusTimestamp.ToDateTimeOffset(), 
                wholeMessage, 
                lastReceived.RunningHash.ToByteArray(),
                lastReceived.SequenceNumber, 
                chunks, 
                transactionId);
        }

		/// <include file="TopicMessage.cs.xml" path='docs/member[@name="P:TopicMessage.ConsensusTimestamp"]/*' />
		public DateTimeOffset ConsensusTimestamp { get; }
		/// <include file="TopicMessage.cs.xml" path='docs/member[@name="P:TopicMessage.Contents"]/*' />
		public byte[] Contents { get; }
		/// <include file="TopicMessage.cs.xml" path='docs/member[@name="P:TopicMessage.RunningHash"]/*' />
		public byte[] RunningHash { get; }
		/// <include file="TopicMessage.cs.xml" path='docs/member[@name="P:TopicMessage.SequenceNumber"]/*' />
		public ulong SequenceNumber { get; }
		/// <include file="TopicMessage.cs.xml" path='docs/member[@name="P:TopicMessage.Chunks"]/*' />
		public TopicMessageChunk[] Chunks { get; }
		/// <include file="TopicMessage.cs.xml" path='docs/member[@name="P:TopicMessage.TransactionId"]/*' />
		public TransactionId? TransactionId { get; }
	}
}
