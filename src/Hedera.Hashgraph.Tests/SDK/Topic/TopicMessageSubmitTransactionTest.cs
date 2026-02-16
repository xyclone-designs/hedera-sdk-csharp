// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Protobuf;
using Proto;
using Io.Github.JsonSnapshot;
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
    public class TopicMessageSubmitTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TopicId testTopicId = new TopicId(0, 6, 9);
        private static readonly byte[] testMessageBytes = new[]
        {
            0x04,
            0x05,
            0x06
        };
        private static readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        private TopicMessageSubmitTransaction SpawnTestTransactionString()
        {
            return new TopicMessageSubmitTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetTopicId(testTopicId).SetMessage(new string (testMessageBytes)).Freeze().Sign(unusedPrivateKey);
        }

        private TopicMessageSubmitTransaction SpawnTestTransactionBytes()
        {
            return new TopicMessageSubmitTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetTopicId(testTopicId).SetMessage(testMessageBytes).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TopicMessageSubmitTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetConsensusSubmitMessage(ConsensusSubmitMessageTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TopicMessageSubmitTransaction>(tx);
        }

        public virtual void ConstructTopicMessageSubmitTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = ConsensusSubmitMessageTransactionBody.NewBuilder().SetTopicID(testTopicId.ToProtobuf()).SetMessage(ByteString.CopyFrom(testMessageBytes)).Build();
            var tx = TransactionBody.NewBuilder().SetConsensusSubmitMessage(transactionBody).Build();
            var topicSubmitMessageTransaction = new TopicMessageSubmitTransaction(tx);
            Assert.Equal(topicSubmitMessageTransaction.GetTopicId(), testTopicId);
        }

        public virtual void GetSetTopicId()
        {
            var topicSubmitMessageTransaction = new TopicMessageSubmitTransaction().SetTopicId(testTopicId);
            Assert.Equal(topicSubmitMessageTransaction.GetTopicId(), testTopicId);
        }

        public virtual void GetSetTopicIdFrozen()
        {
            var tx = SpawnTestTransactionString();
            Assert.Throws<InvalidOperationException>(() => tx.SetTopicId(testTopicId));
        }

        public virtual void GetSetMessage()
        {
            var topicSubmitMessageTransactionString = new TopicMessageSubmitTransaction().SetMessage(new string (testMessageBytes));
            var topicSubmitMessageTransactionBytes = new TopicMessageSubmitTransaction().SetMessage(testMessageBytes);
            Assert.Equal(topicSubmitMessageTransactionString.GetMessage().ToByteArray(), testMessageBytes);
            Assert.Equal(topicSubmitMessageTransactionBytes.GetMessage().ToByteArray(), testMessageBytes);
        }

        public virtual void GetSetMessageFrozen()
        {
            var topicSubmitMessageTransactionString = SpawnTestTransactionString();
            var topicSubmitMessageTransactionBytes = SpawnTestTransactionBytes();
            Assert.Throws<InvalidOperationException>(() => topicSubmitMessageTransactionString.SetMessage(testMessageBytes));
            Assert.Throws<InvalidOperationException>(() => topicSubmitMessageTransactionBytes.SetMessage(testMessageBytes));
        }

        public virtual void ShouldSetCustomFeeLimits()
        {
            var customFeeLimits = Arrays.AsList(new CustomFeeLimit().SetPayerId(new AccountId(0, 0, 1)).SetCustomFees(List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(new TokenId(0, 0, 1)))), new CustomFeeLimit().SetPayerId(new AccountId(0, 0, 2)).SetCustomFees(List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(new TokenId(0, 0, 2)))));
            var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction().SetCustomFeeLimits(customFeeLimits);
            Assert.Equal(topicMessageSubmitTransaction.GetCustomFeeLimits(), customFeeLimits);
        }

        public virtual void ShouldAddCustomFeeLimitToList()
        {
            var customFeeLimits = new List(List.Of(new CustomFeeLimit().SetPayerId(new AccountId(0, 0, 1)).SetCustomFees(List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(new TokenId(0, 0, 1)))), new CustomFeeLimit().SetPayerId(new AccountId(0, 0, 2)).SetCustomFees(List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(new TokenId(0, 0, 2))))));
            var customFeeLimitToBeAdded = new CustomFeeLimit().SetPayerId(new AccountId(0, 0, 3)).SetCustomFees(new List(List.Of(new CustomFixedFee().SetAmount(3).SetDenominatingTokenId(new TokenId(0, 0, 3)))));
            var expectedCustomFeeLimits = new List(customFeeLimits);
            expectedCustomFeeLimits.Add(customFeeLimitToBeAdded);
            var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction().SetCustomFeeLimits(customFeeLimits).AddCustomFeeLimit(customFeeLimitToBeAdded);
            Assert.Equal(topicMessageSubmitTransaction.GetCustomFeeLimits(), expectedCustomFeeLimits);
        }

        public virtual void ShouldAddCustomFeeLimitToEmptyList()
        {
            var customFeeLimitToBeAdded = new CustomFeeLimit().SetPayerId(new AccountId(0, 0, 3)).SetCustomFees(new List(List.Of(new CustomFixedFee().SetAmount(3).SetDenominatingTokenId(new TokenId(0, 0, 3)))));
            var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction().AddCustomFeeLimit(customFeeLimitToBeAdded);
            AssertThat(topicMessageSubmitTransaction.GetCustomFeeLimits()).ContainsExactly(customFeeLimitToBeAdded);
        }
    }
}