// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Proto;
using Proto.Mirror;
using Java.Time;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
        private static readonly long testSequenceNumber = 7;
        private static readonly TransactionId testTransactionId = new TransactionId(new AccountId(0, 0, 1), testTimestamp);
        public virtual void ConstructWithArgs()
        {
            var consensusTopicResponse = Proto.ConsensusTopicResponse.NewBuilder().SetConsensusTimestamp(Timestamp.NewBuilder().SetSeconds(testTimestamp.GetEpochSecond())).SetMessage(ByteString.CopyFrom(testContents)).SetRunningHash(ByteString.CopyFrom(testRunningHash)).SetSequenceNumber(testSequenceNumber).SetChunkInfo(ConsensusMessageChunkInfo.NewBuilder().SetInitialTransactionID(testTransactionId.ToProtobuf()).Build()).Build();
            TopicMessageChunk topicMessageChunk = new TopicMessageChunk(consensusTopicResponse);
            Assert.Equal(topicMessageChunk.consensusTimestamp, testTimestamp);
            Assert.Equal(topicMessageChunk.contentSize, testContents.Length);
            Assert.Equal(topicMessageChunk.runningHash, testRunningHash);
            Assert.Equal(topicMessageChunk.sequenceNumber, testSequenceNumber);
        }
    }
}