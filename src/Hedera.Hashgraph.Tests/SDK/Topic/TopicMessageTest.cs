// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Proto;
using Proto.Mirror;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
        private static readonly long testSequenceNumber = 7;
        private static readonly TransactionId testTransactionId = new TransactionId(new AccountId(0, 0, 1), testTimestamp);
        public virtual void ConstructWithArgs()
        {
            TopicMessageChunk topicMessageChunk = new TopicMessageChunk(ConsensusTopicResponse.NewBuilder().SetConsensusTimestamp(Timestamp.NewBuilder().SetSeconds(testTimestamp.GetEpochSecond())).SetRunningHash(ByteString.CopyFrom(testRunningHash)).SetSequenceNumber(testSequenceNumber).Build());
            TopicMessageChunk[] topicMessageChunkArr = new[]
            {
                topicMessageChunk,
                topicMessageChunk,
                topicMessageChunk
            };
            TopicMessage topicMessage = new TopicMessage(testTimestamp, testContents, testRunningHash, testSequenceNumber, topicMessageChunkArr, testTransactionId);
            Assert.Equal(topicMessage.consensusTimestamp, testTimestamp);
            Assert.Equal(topicMessage.contents, testContents);
            Assert.Equal(topicMessage.runningHash, testRunningHash);
            Assert.Equal(topicMessage.sequenceNumber, testSequenceNumber);
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            Assert.Equal(topicMessage.transactionId, testTransactionId);
        }

        public virtual void OfSingle()
        {
            var consensusTopicResponse = Proto.ConsensusTopicResponse.NewBuilder().SetConsensusTimestamp(Timestamp.NewBuilder().SetSeconds(testTimestamp.GetEpochSecond())).SetMessage(ByteString.CopyFrom(testContents)).SetRunningHash(ByteString.CopyFrom(testRunningHash)).SetSequenceNumber(testSequenceNumber).SetChunkInfo(ConsensusMessageChunkInfo.NewBuilder().SetInitialTransactionID(testTransactionId.ToProtobuf()).Build()).Build();
            TopicMessage topicMessage = TopicMessage.OfSingle(consensusTopicResponse);
            Assert.Equal(topicMessage.consensusTimestamp, testTimestamp);
            Assert.Equal(topicMessage.contents, testContents);
            Assert.Equal(topicMessage.runningHash, testRunningHash);
            Assert.Equal(topicMessage.sequenceNumber, testSequenceNumber);
            Assert.Single(topicMessage.chunks);
            Assert.Equal(topicMessage.transactionId, testTransactionId);
        }

        public virtual void OfMany()
        {
            var consensusTopicResponse1 = Proto.ConsensusTopicResponse.NewBuilder().SetConsensusTimestamp(Timestamp.NewBuilder().SetSeconds(testTimestamp.GetEpochSecond())).SetMessage(ByteString.CopyFrom(testContents)).SetRunningHash(ByteString.CopyFrom(testRunningHash)).SetSequenceNumber(testSequenceNumber).SetChunkInfo(ConsensusMessageChunkInfo.NewBuilder().SetInitialTransactionID(testTransactionId.ToProtobuf()).SetNumber(1).SetTotal(2).Build()).Build();
            var consensusTopicResponse2 = Proto.ConsensusTopicResponse.NewBuilder().SetConsensusTimestamp(Timestamp.NewBuilder().SetSeconds(testTimestamp.GetEpochSecond() + 1)).SetMessage(ByteString.CopyFrom(testContents)).SetRunningHash(ByteString.CopyFrom(testRunningHash)).SetSequenceNumber(testSequenceNumber + 1).SetChunkInfo(ConsensusMessageChunkInfo.NewBuilder().SetNumber(2).SetTotal(2).Build()).Build();
            TopicMessage topicMessage = TopicMessage.OfMany(List.Of(consensusTopicResponse1, consensusTopicResponse2));
            byte[] totalContents = new byte[testContents.Length * 2];
            Array.Copy(testContents, 0, totalContents, 0, testContents.Length);
            Array.Copy(testContents, 0, totalContents, testContents.Length, testContents.Length);
            Assert.Equal(topicMessage.consensusTimestamp, testTimestamp.PlusSeconds(1));
            Assert.Equal(topicMessage.contents, totalContents);
            Assert.Equal(topicMessage.runningHash, testRunningHash);
            Assert.Equal(topicMessage.sequenceNumber, testSequenceNumber + 1);
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            Assert.Equal(topicMessage.transactionId, testTransactionId);
        }
    }
}