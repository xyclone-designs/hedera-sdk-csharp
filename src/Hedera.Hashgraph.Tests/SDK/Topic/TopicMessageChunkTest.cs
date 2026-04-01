// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Topic;

using Google.Protobuf;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    public class TopicMessageChunkTest
    {
        private static readonly DateTimeOffset testTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private static readonly byte[] testContents = new byte[]
        {
            0x01,
            0x02,
            0x03
        };
        private static readonly byte[] testRunningHash = new byte[]
        {
            0x04,
            0x05,
            0x06
        };
        private static readonly ulong testSequenceNumber = 7;
        private static readonly TransactionId testTransactionId = new TransactionId(new AccountId(0, 0, 1), testTimestamp);

        [Fact]
        public virtual void ConstructWithArgs()
        {
            var consensusTopicResponse = new Proto.ConsensusTopicResponse
            {
                ConsensusTimestamp = new Proto.Timestamp{ Seconds = testTimestamp.ToUnixTimeSeconds() },
                Message = ByteString.CopyFrom(testContents),
                RunningHash = ByteString.CopyFrom(testRunningHash),
                SequenceNumber = testSequenceNumber,
                ChunkInfo = new Proto.ConsensusMessageChunkInfo { InitialTransactionID = testTransactionId.ToProtobuf() },
            };

            TopicMessageChunk topicMessageChunk = new (consensusTopicResponse);

            Assert.Equal(topicMessageChunk.ConsensusTimestamp, testTimestamp);
            Assert.Equal(topicMessageChunk.ContentSize, testContents.Length);
            Assert.Equal(topicMessageChunk.RunningHash, testRunningHash);
            Assert.Equal(topicMessageChunk.SequenceNumber, testSequenceNumber);
        }
    }
}