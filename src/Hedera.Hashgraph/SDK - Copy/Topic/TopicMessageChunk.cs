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
        public readonly Timestamp ConsensusTimestamp;
        public readonly long ContentSize;
        public readonly byte[] RunningHash;
        public readonly ulong SequenceNumber;
        /// <summary>
        /// Create a topic message chunk from a protobuf.
        /// </summary>
        /// <param name="response">the protobuf</param>
        public TopicMessageChunk(ConsensusTopicResponse response)
        {
            ConsensusTimestamp = Utils.TimestampConverter.FromProtobuf(response.ConsensusTimestamp);
            ContentSize = response.Message.Length;
            RunningHash = response.RunningHash.ToByteArray();
            SequenceNumber = response.SequenceNumber;
        }
    }
}