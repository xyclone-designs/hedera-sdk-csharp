// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class TopicUpdateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PublicKey testAdminKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e11").GetPublicKey();
        private static readonly PublicKey testSubmitKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e12").GetPublicKey();
        private static readonly TopicId testTopicId = TopicId.FromString("0.0.5007");
        private static readonly string testTopicMemo = "test memo";
        private static readonly Duration testAutoRenewPeriod = Duration.OfHours(10);
        private static readonly Instant testExpirationTime = Instant.Now();
        private static readonly AccountId testAutoRenewAccountId = AccountId.FromString("8.8.8");
        private static readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void ClearShouldSerialize()
        {
            SnapshotMatcher.Expect(new TopicUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetTopicId(testTopicId).ClearAdminKey().ClearAutoRenewAccountId().ClearSubmitKey().ClearTopicMemo().Freeze().Sign(unusedPrivateKey).ToString()).ToMatchSnapshot();
        }

        virtual void SetShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TopicUpdateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TopicUpdateTransaction SpawnTestTransaction()
        {
            return new TopicUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetTopicId(testTopicId).SetAdminKey(testAdminKey).SetAutoRenewAccountId(testAutoRenewAccountId).SetAutoRenewPeriod(testAutoRenewPeriod).SetSubmitKey(testSubmitKey).SetTopicMemo(testTopicMemo).SetExpirationTime(validStart).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TopicUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetConsensusUpdateTopic(ConsensusUpdateTopicTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TopicUpdateTransaction>(tx);
        }

        virtual void ConstructTopicUpdateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = ConsensusUpdateTopicTransactionBody.NewBuilder().SetTopicID(testTopicId.ToProtobuf()).SetMemo(StringValue.NewBuilder().SetValue(testTopicMemo).Build()).SetExpirationTime(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond()).Build()).SetAdminKey(testAdminKey.ToProtobufKey()).SetSubmitKey(testSubmitKey.ToProtobufKey()).SetAutoRenewPeriod(com.hedera.hashgraph.sdk.proto.Duration.NewBuilder().SetSeconds(testAutoRenewPeriod.ToSeconds()).Build()).SetAutoRenewAccount(testAutoRenewAccountId.ToProtobuf()).Build();
            var tx = TransactionBody.NewBuilder().SetConsensusUpdateTopic(transactionBody).Build();
            var topicUpdateTransaction = new TopicUpdateTransaction(tx);
            Assert.Equal(topicUpdateTransaction.GetTopicId(), testTopicId);
            Assert.Equal(topicUpdateTransaction.GetTopicMemo(), testTopicMemo);
            Assert.Equal(topicUpdateTransaction.GetExpirationTime().GetEpochSecond(), testExpirationTime.GetEpochSecond());
            Assert.Equal(topicUpdateTransaction.GetAdminKey(), testAdminKey);
            Assert.Equal(topicUpdateTransaction.GetSubmitKey(), testSubmitKey);
            Assert.Equal(topicUpdateTransaction.GetAutoRenewPeriod().ToSeconds(), testAutoRenewPeriod.ToSeconds());
            Assert.Equal(topicUpdateTransaction.GetAutoRenewAccountId(), testAutoRenewAccountId);
        }

        // doesn't throw an exception as opposed to C++ sdk
        virtual void ConstructTopicUpdateTransactionFromWrongTransactionBodyProtobuf()
        {
            var transactionBody = CryptoDeleteTransactionBody.NewBuilder().Build();
            var tx = TransactionBody.NewBuilder().SetCryptoDelete(transactionBody).Build();
            new TopicUpdateTransaction(tx);
        }

        virtual void GetSetTopicId()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetTopicId(testTopicId);
            Assert.Equal(topicUpdateTransaction.GetTopicId(), testTopicId);
        }

        virtual void GetSetTopicIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTopicId(testTopicId));
        }

        virtual void GetSetTopicMemo()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetTopicMemo(testTopicMemo);
            Assert.Equal(topicUpdateTransaction.GetTopicMemo(), testTopicMemo);
        }

        virtual void GetSetTopicMemoFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTopicMemo(testTopicMemo));
        }

        virtual void ClearTopicMemo()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetTopicMemo(testTopicMemo);
            topicUpdateTransaction.ClearTopicMemo();
            Assert.Empty(topicUpdateTransaction.GetTopicMemo());
        }

        virtual void ClearTopicMemoFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.ClearTopicMemo());
        }

        virtual void GetSetExpirationTime()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetExpirationTime(testExpirationTime);
            Assert.Equal(topicUpdateTransaction.GetExpirationTime(), testExpirationTime);
        }

        virtual void GetSetExpirationTimeFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetExpirationTime(testExpirationTime));
        }

        virtual void GetSetAdminKey()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetAdminKey(testAdminKey);
            Assert.Equal(topicUpdateTransaction.GetAdminKey(), testAdminKey);
        }

        virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAdminKey(testAdminKey));
        }

        virtual void ClearAdminKey()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetAdminKey(testAdminKey);
            topicUpdateTransaction.ClearAdminKey();
            Assert.Equal(topicUpdateTransaction.GetAdminKey(), new KeyList());
        }

        virtual void ClearAdminKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.ClearAdminKey());
        }

        virtual void GetSetSubmitKey()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetSubmitKey(testSubmitKey);
            Assert.Equal(topicUpdateTransaction.GetSubmitKey(), testSubmitKey);
        }

        virtual void GetSetSubmitKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetSubmitKey(testSubmitKey));
        }

        virtual void ClearSubmitKey()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetSubmitKey(testSubmitKey);
            topicUpdateTransaction.ClearSubmitKey();
            Assert.Equal(topicUpdateTransaction.GetSubmitKey(), new KeyList());
        }

        virtual void ClearSubmitKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.ClearSubmitKey());
        }

        virtual void GetSetAutoRenewPeriod()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetAutoRenewPeriod(testAutoRenewPeriod);
            Assert.Equal(topicUpdateTransaction.GetAutoRenewPeriod(), testAutoRenewPeriod);
        }

        virtual void GetSetAutoRenewPeriodFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAutoRenewPeriod(testAutoRenewPeriod));
        }

        virtual void GetSetAutoRenewAccountId()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetAutoRenewAccountId(testAutoRenewAccountId);
            Assert.Equal(topicUpdateTransaction.GetAutoRenewAccountId(), testAutoRenewAccountId);
        }

        virtual void GetSetAutoRenewAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAutoRenewAccountId(testAutoRenewAccountId));
        }

        virtual void ClearAutoRenewAccountId()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction().SetAutoRenewAccountId(testAutoRenewAccountId);
            topicUpdateTransaction.ClearAutoRenewAccountId();
            Assert.Equal(topicUpdateTransaction.GetAutoRenewAccountId(), new AccountId(0, 0, 0));
        }

        virtual void ClearAutoRenewAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.ClearAutoRenewAccountId());
        }

        virtual void ShouldSetFeeScheduleKey()
        {
            PrivateKey feeScheduleKey = PrivateKey.GenerateECDSA();
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            topicUpdateTransaction.SetFeeScheduleKey(feeScheduleKey);
            Assert.Equal(topicUpdateTransaction.GetFeeScheduleKey().ToString(), feeScheduleKey.ToString());
        }

        virtual void ShouldSetFeeExemptKeys()
        {
            IList<PrivateKey> feeExemptKeys = List.Of(PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA());
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            topicUpdateTransaction.SetFeeExemptKeys(new List(feeExemptKeys));
            AssertThat(topicUpdateTransaction.GetFeeExemptKeys()).ContainsExactlyElementsOf(feeExemptKeys);
        }

        virtual void ShouldAddFeeExemptKeyToEmptyList()
        {
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            PrivateKey feeExemptKeyToBeAdded = PrivateKey.GenerateECDSA();
            topicUpdateTransaction.AddFeeExemptKey(feeExemptKeyToBeAdded);
            AssertThat(topicUpdateTransaction.GetFeeExemptKeys()).HasSize(1).ContainsExactly(feeExemptKeyToBeAdded);
        }

        virtual void ShouldAddFeeExemptKeyToList()
        {
            PrivateKey feeExemptKey = PrivateKey.GenerateECDSA();
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            topicUpdateTransaction.SetFeeExemptKeys(new List(List.Of(feeExemptKey)));
            PrivateKey feeExemptKeyToBeAdded = PrivateKey.GenerateECDSA();
            topicUpdateTransaction.AddFeeExemptKey(feeExemptKeyToBeAdded);
            AssertThat(topicUpdateTransaction.GetFeeExemptKeys()).HasSize(2).ContainsExactly(feeExemptKey, feeExemptKeyToBeAdded);
        }

        virtual void ShouldClearFeeExemptKeys()
        {
            PrivateKey feeExemptKey = PrivateKey.GenerateECDSA();
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            topicUpdateTransaction.SetFeeExemptKeys(new List(List.Of(feeExemptKey)));
            topicUpdateTransaction.ClearFeeExemptKeys();
            Assert.Empty(topicUpdateTransaction.GetFeeExemptKeys());
        }

        virtual void ShouldSetCustomFees()
        {
            IList<CustomFixedFee> customFixedFees = List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(new TokenId(0, 0, 0)), new CustomFixedFee().SetAmount(2).SetDenominatingTokenId(new TokenId(0, 0, 1)), new CustomFixedFee().SetAmount(3).SetDenominatingTokenId(new TokenId(0, 0, 2)));
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            topicUpdateTransaction.SetCustomFees(new List(customFixedFees));
            AssertThat(topicUpdateTransaction.GetCustomFees()).HasSize(3).ContainsExactlyElementsOf(customFixedFees);
        }

        virtual void ShouldAddCustomFeeToList()
        {
            IList<CustomFixedFee> customFixedFees = List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(new TokenId(0, 0, 0)), new CustomFixedFee().SetAmount(2).SetDenominatingTokenId(new TokenId(0, 0, 1)), new CustomFixedFee().SetAmount(3).SetDenominatingTokenId(new TokenId(0, 0, 2)));
            CustomFixedFee customFixedFeeToBeAdded = new CustomFixedFee().SetAmount(4).SetDenominatingTokenId(new TokenId(0, 0, 3));
            IList<CustomFixedFee> expectedCustomFees = new List(customFixedFees);
            expectedCustomFees.Add(customFixedFeeToBeAdded);
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            topicUpdateTransaction.SetCustomFees(new List(customFixedFees));
            topicUpdateTransaction.AddCustomFee(customFixedFeeToBeAdded);
            AssertThat(topicUpdateTransaction.GetCustomFees()).HasSize(4).ContainsExactlyElementsOf(expectedCustomFees);
        }

        virtual void ShouldAddCustomFeeToEmptyList()
        {
            CustomFixedFee customFixedFeeToBeAdded = new CustomFixedFee().SetAmount(4).SetDenominatingTokenId(new TokenId(0, 0, 3));
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            topicUpdateTransaction.AddCustomFee(customFixedFeeToBeAdded);
            AssertThat(topicUpdateTransaction.GetCustomFees()).HasSize(1).ContainsExactly(customFixedFeeToBeAdded);
        }

        virtual void ShouldClearCustomFees()
        {
            IList<CustomFixedFee> customFixedFees = List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(new TokenId(0, 0, 0)), new CustomFixedFee().SetAmount(2).SetDenominatingTokenId(new TokenId(0, 0, 1)), new CustomFixedFee().SetAmount(3).SetDenominatingTokenId(new TokenId(0, 0, 2)));
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            topicUpdateTransaction.SetCustomFees(new List(customFixedFees));
            topicUpdateTransaction.ClearCustomFees();
            Assert.Empty(topicUpdateTransaction.GetCustomFees());
        }
    }
}