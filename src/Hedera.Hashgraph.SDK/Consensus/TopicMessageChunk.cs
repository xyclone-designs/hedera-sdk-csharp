// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicMessageChunk.cs.xml" path='docs/member[@name="T:TopicMessageChunk"]/*' />
    /// <include file="TopicMessageChunk.cs.xml" path='docs/member[@name="M:TopicMessageChunk.#ctor(Proto.Mirror.ConsensusTopicResponse)"]/*' />
    public sealed class TopicMessageChunk(Proto.Mirror.ConsensusTopicResponse response)
    {
		public DateTimeOffset ConsensusTimestamp { get; } = response.ConsensusTimestamp.ToDateTimeOffset();
		public long ContentSize { get; } = response.Message.Length;
		public byte[] RunningHash { get; } = response.RunningHash.ToByteArray();
		public ulong SequenceNumber { get; } = response.SequenceNumber;
    }
}
