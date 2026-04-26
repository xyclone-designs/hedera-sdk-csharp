// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptocurrency;

using Google.Protobuf;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    public class TopicMessageTest
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
        private static readonly TransactionId testTransactionId = new (new AccountId(0, 0, 1), testTimestamp);

        [Fact]
        public virtual void ConstructWithArgs()
        {
            var consensusTopicResponse = new Proto.Mirror.ConsensusTopicResponse
            {
                Message = ByteString.CopyFrom(testContents),
                RunningHash = ByteString.CopyFrom(testRunningHash),
                SequenceNumber = testSequenceNumber,
                ChunkInfo = new Proto.Services.ConsensusMessageChunkInfo { InitialTransactionId = testTransactionId.ToProtobuf() },
                ConsensusTimestamp = new Proto.Services.Timestamp
                {
                    Seconds = testTimestamp.ToUnixTimeSeconds()
                }
            };

            TopicMessageChunk topicMessageChunk = new (new Proto.Mirror.ConsensusTopicResponse
            {
                Message = ByteString.CopyFrom(testContents),
                RunningHash = ByteString.CopyFrom(testRunningHash),
                SequenceNumber = testSequenceNumber,
                ConsensusTimestamp = new Proto.Services.Timestamp
                {
                    Seconds = testTimestamp.ToUnixTimeSeconds()
                }
            });
            TopicMessageChunk[] topicMessageChunkArr = new[]
            {
                topicMessageChunk,
                topicMessageChunk,
                topicMessageChunk
            };
            TopicMessage topicMessage = new (testTimestamp, testContents, testRunningHash, testSequenceNumber, topicMessageChunkArr, testTransactionId);
            
            Assert.Equal(topicMessage.ConsensusTimestamp, testTimestamp);
            Assert.Equal(topicMessage.Contents, testContents);
            Assert.Equal(topicMessage.RunningHash, testRunningHash);
            Assert.Equal(topicMessage.SequenceNumber, testSequenceNumber);
            Assert.Equal(topicMessage.TransactionId, testTransactionId);
        }
        [Fact]
        public virtual void OfSingle()
        {
            var consensusTopicResponse = new Proto.Mirror.ConsensusTopicResponse
            {
                Message = ByteString.CopyFrom(testContents),
                RunningHash = ByteString.CopyFrom(testRunningHash),
                SequenceNumber = testSequenceNumber,
                ChunkInfo = new Proto.Services.ConsensusMessageChunkInfo { InitialTransactionId = testTransactionId.ToProtobuf() },
                ConsensusTimestamp = new Proto.Services.Timestamp
                {
                    Seconds = testTimestamp.ToUnixTimeSeconds()
                }
            };
            
            TopicMessage topicMessage = TopicMessage.OfSingle(consensusTopicResponse);
            
            Assert.Equal(topicMessage.ConsensusTimestamp, testTimestamp);
            Assert.Equal(topicMessage.Contents, testContents);
            Assert.Equal(topicMessage.RunningHash, testRunningHash);
            Assert.Equal(topicMessage.SequenceNumber, testSequenceNumber);
            Assert.Single(topicMessage.Chunks);
            Assert.Equal(topicMessage.TransactionId, testTransactionId);
        }
        [Fact]
        public virtual void OfMany()
        {
            var consensusTopicResponse1 = new Proto.Mirror.ConsensusTopicResponse
            {
                Message = ByteString.CopyFrom(testContents),
                RunningHash = ByteString.CopyFrom(testRunningHash),
                SequenceNumber = testSequenceNumber,
                ChunkInfo = new Proto.Services.ConsensusMessageChunkInfo 
                {
                    InitialTransactionId = testTransactionId.ToProtobuf(),
                    Number = 1,
                    Total = 2,
                },
                ConsensusTimestamp = new Proto.Services.Timestamp
                {
                    Seconds = testTimestamp.ToUnixTimeSeconds()
                }
            };
            var consensusTopicResponse2 = new Proto.Mirror.ConsensusTopicResponse
            {
                Message = ByteString.CopyFrom(testContents),
                RunningHash = ByteString.CopyFrom(testRunningHash),
                SequenceNumber = testSequenceNumber + 1,
                ChunkInfo = new Proto.Services.ConsensusMessageChunkInfo 
                {
                    InitialTransactionId = testTransactionId.ToProtobuf(),
                    Number = 2,
                    Total = 2,
                },
                ConsensusTimestamp = new Proto.Services.Timestamp
                {
                    Seconds = testTimestamp.ToUnixTimeSeconds() + 1
                }
            };
            
            TopicMessage topicMessage = TopicMessage.OfMany([consensusTopicResponse1, consensusTopicResponse2]);
            byte[] totalContents = new byte[testContents.Length * 2];
            
            Array.Copy(testContents, 0, totalContents, 0, testContents.Length);
            Array.Copy(testContents, 0, totalContents, testContents.Length, testContents.Length);
            Assert.Equal(topicMessage.ConsensusTimestamp, testTimestamp.AddSeconds(1));
            Assert.Equal(topicMessage.Contents, totalContents);
            Assert.Equal(topicMessage.RunningHash, testRunningHash);
            Assert.Equal(topicMessage.SequenceNumber, testSequenceNumber + 1);
            Assert.Equal(topicMessage.TransactionId, testTransactionId);
        }
    }
}