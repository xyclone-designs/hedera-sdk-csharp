using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * A chunk of the topic message.
     */
    sealed class TopicMessageChunk 
    {
		public long ContentSize { get; }
		public byte[] RunningHash { get; }
		public long SequenceNumber { get; }
		public DateTimeOffset ConsensusTimestamp { get; }

		/**
         * Create a topic message chunk from a protobuf.
         *
         * @param response                  the protobuf
         */
		public TopicMessageChunk(ConsensusTopicResponse response) 
        {
            ConsensusTimestamp = DateTimeOffsetConverter.FromProtobuf(response.getConsensusTimestamp());
            ContentSize = response.getMessage().size();
            RunningHash = response.getRunningHash().ToByteArray();
            SequenceNumber = response.getSequenceNumber();
        }
    }

}