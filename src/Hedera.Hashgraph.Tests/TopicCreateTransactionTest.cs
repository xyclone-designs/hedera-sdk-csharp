// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
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
    public class TopicCreateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TopicCreateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TopicCreateTransaction SpawnTestTransaction()
        {
            return new TopicCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetSubmitKey(unusedPrivateKey).SetAdminKey(unusedPrivateKey).SetAutoRenewAccountId(AccountId.FromString("0.0.5007")).SetAutoRenewPeriod(Duration.OfHours(24)).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetTopicMemo("hello memo").Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TopicCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetConsensusCreateTopic(ConsensusCreateTopicTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TopicCreateTransaction>(tx);
        }

        virtual void ShouldSetFeeScheduleKey()
        {
            PrivateKey feeScheduleKey = PrivateKey.GenerateECDSA();
            TopicCreateTransaction topicCreateTransaction = new TopicCreateTransaction();
            topicCreateTransaction.SetFeeScheduleKey(feeScheduleKey);
            Assert.Equal(topicCreateTransaction.GetFeeScheduleKey().ToString(), feeScheduleKey.ToString());
        }

        virtual void ShouldSetFeeExemptKeys()
        {
            Key feeExemptKey1 = PrivateKey.GenerateECDSA();
            Key feeExemptKey2 = PrivateKey.GenerateECDSA();
            IList<Key> feeExemptKeys = Arrays.AsList(feeExemptKey1, feeExemptKey2);
            TopicCreateTransaction topicCreateTransaction = new TopicCreateTransaction();
            topicCreateTransaction.SetFeeExemptKeys(feeExemptKeys);
            IList<Key> retrievedKeys = topicCreateTransaction.GetFeeExemptKeys();
            for (int i = 0; i < feeExemptKeys.Count; i++)
            {
                Assert.Equal(retrievedKeys[i].ToString(), feeExemptKeys[i].ToString());
            }
        }

        virtual void ShouldAddFeeExemptKeyToEmptyList()
        {
            TopicCreateTransaction topicCreateTransaction = new TopicCreateTransaction();
            PrivateKey feeExemptKeyToBeAdded = PrivateKey.GenerateECDSA();
            topicCreateTransaction.AddFeeExemptKey(feeExemptKeyToBeAdded);
            AssertThat(topicCreateTransaction.GetFeeExemptKeys()).HasSize(1).ContainsExactly(feeExemptKeyToBeAdded);
        }

        virtual void ShouldAddFeeExemptKeyToList()
        {
            PrivateKey feeExemptKey = PrivateKey.GenerateECDSA();
            TopicCreateTransaction topicCreateTransaction = new TopicCreateTransaction();
            topicCreateTransaction.SetFeeExemptKeys(new List(List.Of(feeExemptKey)));
            Key feeExemptKeyToBeAdded = PrivateKey.GenerateECDSA();
            topicCreateTransaction.AddFeeExemptKey(feeExemptKeyToBeAdded);
            AssertThat(topicCreateTransaction.GetFeeExemptKeys()).HasSize(2).ContainsExactly(feeExemptKey, feeExemptKeyToBeAdded);
        }

        virtual void ShouldSetTopicCustomFees()
        {
            IList<CustomFixedFee> customFixedFees = new List(List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(new TokenId(0, 0, 0)), new CustomFixedFee().SetAmount(2).SetDenominatingTokenId(new TokenId(0, 0, 1)), new CustomFixedFee().SetAmount(3).SetDenominatingTokenId(new TokenId(0, 0, 2))));
            TopicCreateTransaction topicCreateTransaction = new TopicCreateTransaction();
            topicCreateTransaction.SetCustomFees(customFixedFees);
            AssertThat(topicCreateTransaction.GetCustomFees()).HasSize(customFixedFees.Count).ContainsExactlyElementsOf(customFixedFees);
        }

        virtual void ShouldAddTopicCustomFeeToList()
        {
            IList<CustomFixedFee> customFixedFees = new List(List.Of(new CustomFixedFee().SetAmount(1).SetDenominatingTokenId(new TokenId(0, 0, 0)), new CustomFixedFee().SetAmount(2).SetDenominatingTokenId(new TokenId(0, 0, 1)), new CustomFixedFee().SetAmount(3).SetDenominatingTokenId(new TokenId(0, 0, 2))));
            CustomFixedFee customFixedFeeToBeAdded = new CustomFixedFee().SetAmount(4).SetDenominatingTokenId(new TokenId(0, 0, 3));
            IList<CustomFixedFee> expectedCustomFees = new List(customFixedFees);
            expectedCustomFees.Add(customFixedFeeToBeAdded);
            TopicCreateTransaction topicCreateTransaction = new TopicCreateTransaction();
            topicCreateTransaction.SetCustomFees(customFixedFees);
            topicCreateTransaction.AddCustomFee(customFixedFeeToBeAdded);
            AssertThat(topicCreateTransaction.GetCustomFees()).HasSize(expectedCustomFees.Count).ContainsExactlyElementsOf(expectedCustomFees);
        }

        virtual void ShouldAddTopicCustomFeeToEmptyList()
        {
            CustomFixedFee customFixedFeeToBeAdded = new CustomFixedFee().SetAmount(4).SetDenominatingTokenId(new TokenId(0, 0, 3));
            TopicCreateTransaction topicCreateTransaction = new TopicCreateTransaction();
            topicCreateTransaction.AddCustomFee(customFixedFeeToBeAdded);
            AssertThat(topicCreateTransaction.GetCustomFees()).HasSize(1).ContainsExactly(customFixedFeeToBeAdded);
        }
    }
}