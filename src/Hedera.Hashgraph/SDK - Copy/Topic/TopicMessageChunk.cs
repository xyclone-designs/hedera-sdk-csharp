// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Mirror.Api.Proto;

using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// A chunk of the topic message.
    /// </summary>
    public sealed class TopicMessageChunk
    {
        public readonly Timestamp consensusTimestamp;
        public readonly long contentSize;
        public readonly byte[] runningHash;
        public readonly ulong sequenceNumber;
        /// <summary>
        /// Create a topic message chunk from a protobuf.
        /// </summary>
        /// <param name="response">the protobuf</param>
        public TopicMessageChunk(ConsensusTopicResponse response)
        {
            consensusTimestamp = Utils.TimestampConverter.FromProtobuf(response.ConsensusTimestamp);
            contentSize = response.Message.Length;
            runningHash = response.RunningHash.ToByteArray();
            sequenceNumber = response.SequenceNumber;
        }
    }
}