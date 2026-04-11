// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicMessageChunk.cs.xml" path='docs/member[@name="T:TopicMessageChunk"]/*' />
    public sealed class TopicMessageChunk
    {
        /// <include file="TopicMessageChunk.cs.xml" path='docs/member[@name="M:TopicMessageChunk.#ctor(Proto.ConsensusTopicResponse)"]/*' />
        public TopicMessageChunk(Proto.ConsensusTopicResponse response)
        {
            ConsensusTimestamp = response.ConsensusTimestamp.ToDateTimeOffset();
            ContentSize = response.Message.Length;
            RunningHash = response.RunningHash.ToByteArray();
            SequenceNumber = response.SequenceNumber;
        }

		public DateTimeOffset ConsensusTimestamp { get; }
		public long ContentSize { get; }
		public byte[] RunningHash { get; }
		public ulong SequenceNumber { get; }
	}
}