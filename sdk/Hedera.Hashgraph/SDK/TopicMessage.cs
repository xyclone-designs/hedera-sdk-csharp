namespace Hedera.Hashgraph.SDK
{
	/**
	 * Topic message records.
	 */
	public sealed class TopicMessage
	{
		/**
		 * Constructor.
		 *
		 * @param lastConsensusTimestamp    the last consensus time
		 * @param message                   the message
		 * @param lastRunningHash           the last running hash
		 * @param lastSequenceNumber        the last sequence number
		 * @param chunks                    the array of chunks
		 * @param transactionId             the transaction id
		 */
		TopicMessage(
			DateTimeOffset lastConsensusTimestamp,
			byte[] message,
			byte[] lastRunningHash,
			long lastSequenceNumber,
			TopicMessageChunk[]? chunks,
			TransactionId? transactionId)
		{
			ConsensusTimestamp = lastConsensusTimestamp;
			Contents = message;
			RunningHash = lastRunningHash;
			SequenceNumber = lastSequenceNumber;
			Chunks = chunks;
			TransactionId = transactionId;
		}

		public DateTimeOffset ConsensusTimestamp { get; }
		public byte[] Contents { get; }
		public byte[] RunningHash { get; }
		public long SequenceNumber { get; }
		public TopicMessageChunk[]? Chunks { get; }
		public TransactionId? TransactionId { get; }

		/**
		 * Create a new topic message from a response protobuf.
		 *
		 * @param response                  the protobuf response
		 * @return                          the new topic message
		 */
		static TopicMessage ofSingle(ConsensusTopicResponse response)
		{
			return new TopicMessage(
					DateTimeOffsetConverter.FromProtobuf(response.getConsensusTimestamp()),
					response.getMessage().ToByteArray(),
					response.getRunningHash().ToByteArray(),
					response.getSequenceNumber(),
					new TopicMessageChunk[] { new (response) },
					response.hasChunkInfo() && response.getChunkInfo().hasInitialTransactionID()
							? TransactionId.FromProtobuf(response.getChunkInfo().getInitialTransactionID())
							: null);
		}

		/**
		 * Create a new topic message from a list of response's protobuf.
		 *
		 * @param responses                 the protobuf response
		 * @return                          the new topic message
		 */
		static TopicMessage ofMany(List<ConsensusTopicResponse> responses)
		{
			// response should be in the order of oldest to newest (not chunk order)
			var chunks = new TopicMessageChunk[responses.size()];
			TransactionId transactionId = null;
			var contents = new ByteString[responses.size()];
			long totalSize = 0;

			for (ConsensusTopicResponse r : responses)
			{
				if (transactionId == null && r.getChunkInfo().hasInitialTransactionID())
				{
					transactionId = TransactionId.FromProtobuf(r.getChunkInfo().getInitialTransactionID());
				}

				int index = r.getChunkInfo().getNumber() - 1;

				chunks[index] = new TopicMessageChunk(r);
				contents[index] = r.getMessage();
				totalSize += r.getMessage().size();
			}

			var wholeMessage = ByteBuffer.allocate((int)totalSize);

			for (var content : contents)
			{
				wholeMessage.put(content.asReadOnlyByteBuffer());
			}

			var lastReceived = responses.get(responses.size() - 1);

			return new TopicMessage(
					DateTimeOffsetConverter.FromProtobuf(lastReceived.getConsensusTimestamp()),
					wholeMessage.array(),
					lastReceived.getRunningHash().ToByteArray(),
					lastReceived.getSequenceNumber(),
					chunks,
					transactionId);
		}

		@Override
		public string toString()
		{
			return MoreObjects.toStringHelper(this)
					.Add("consensusTimestamp", consensusTimestamp)
					.Add("contents", new string(contents, StandardCharsets.UTF_8))
					.Add("runningHash", runningHash)
					.Add("sequenceNumber", sequenceNumber)
					.toString();
		}
	}

}