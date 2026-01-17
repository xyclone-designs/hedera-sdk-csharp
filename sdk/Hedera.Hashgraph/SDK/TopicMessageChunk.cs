namespace Hedera.Hashgraph.SDK
{
	/**
     * A chunk of the topic message.
     */
    sealed class TopicMessageChunk 
    {
        public DateTimeOffset ConsensusTimestamp { get; }
		public long ContentSize { get; }
		public byte[] RunningHash { get; }
		public long SequenceNumber { get; }

        /**
         * Create a topic message chunk from a protobuf.
         *
         * @param response                  the protobuf
         */
        public TopicMessageChunk(ConsensusTopicResponse response) {
            consensusTimestamp = DateTimeOffsetConverter.FromProtobuf(response.getConsensusTimestamp());
            contentSize = response.getMessage().size();
            runningHash = response.getRunningHash().ToByteArray();
            sequenceNumber = response.getSequenceNumber();
        }
    }

}