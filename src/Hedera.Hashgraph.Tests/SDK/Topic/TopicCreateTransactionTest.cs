// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.HBar;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    public class TopicCreateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TopicCreateTransaction();
            var tx2 = Transaction.FromBytes<TopicCreateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TopicCreateTransaction SpawnTestTransaction()
        {
            return new TopicCreateTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                SubmitKey = unusedPrivateKey,
                AdminKey = unusedPrivateKey,
                AutoRenewAccountId = AccountId.FromString("0.0.5007"),
                AutoRenewPeriod = TimeSpan.FromHours(24),
                MaxTransactionFee = Hbar.FromTinybars(100000),
                TopicMemo = "hello memo",
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TopicCreateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                ConsensusCreateTopic = new Proto.ConsensusCreateTopicTransactionBody()
            };
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TopicCreateTransaction>(tx);
        }

        public virtual void ShouldSetFeeScheduleKey()
        {
            PrivateKey feeScheduleKey = PrivateKey.GenerateECDSA();
            TopicCreateTransaction topicCreateTransaction = new () { FeeScheduleKey = feeScheduleKey };
            Assert.Equal(topicCreateTransaction.FeeScheduleKey.ToString(), feeScheduleKey.ToString());
        }

        public virtual void ShouldSetFeeExemptKeys()
        {
            Key feeExemptKey1 = PrivateKey.GenerateECDSA();
            Key feeExemptKey2 = PrivateKey.GenerateECDSA();
            IList<Key> feeExemptKeys = [feeExemptKey1, feeExemptKey2];
            TopicCreateTransaction topicCreateTransaction = new () { FeeExemptKeys = [.. feeExemptKeys] };
            IList<Key> retrievedKeys = topicCreateTransaction.FeeExemptKeys;
            for (int i = 0; i < feeExemptKeys.Count; i++)
            {
                Assert.Equal(retrievedKeys[i].ToString(), feeExemptKeys[i].ToString());
            }
        }

        public virtual void ShouldAddFeeExemptKeyToEmptyList()
        {
            TopicCreateTransaction topicCreateTransaction = new ();
            PrivateKey feeExemptKeyToBeAdded = PrivateKey.GenerateECDSA();
            topicCreateTransaction.FeeExemptKeys.Add(feeExemptKeyToBeAdded);

            Assert.Equal(topicCreateTransaction.FeeExemptKeys, [feeExemptKeyToBeAdded]);
        }

        public virtual void ShouldAddFeeExemptKeyToList()
        {
            PrivateKey feeExemptKey = PrivateKey.GenerateECDSA();
            TopicCreateTransaction topicCreateTransaction = new () { FeeExemptKeys = [feeExemptKey] };
            Key feeExemptKeyToBeAdded = PrivateKey.GenerateECDSA();
            topicCreateTransaction.FeeExemptKeys.Add(feeExemptKeyToBeAdded);

            Assert.Equal(topicCreateTransaction.FeeExemptKeys, [feeExemptKey, feeExemptKeyToBeAdded]);
        }

        public virtual void ShouldSetTopicCustomFees()
        {
            IList<CustomFixedFee> customFixedFees =
            [
                new CustomFixedFee
                {
                    Amount = 1,
                    DenominatingTokenId = new TokenId(0, 0, 0)
                },
                new CustomFixedFee
                {
                    Amount = 2,
                    DenominatingTokenId = new TokenId(0, 0, 1)
                },
                new CustomFixedFee
                {
                    Amount = 3,
                    DenominatingTokenId = new TokenId(0, 0, 2)
                },
            ];
            TopicCreateTransaction topicCreateTransaction = new () { CustomFees = [..customFixedFees] };
            Assert.Equal(topicCreateTransaction.CustomFees, customFixedFees);
        }

        public virtual void ShouldAddTopicCustomFeeToList()
        {
            IList<CustomFixedFee> customFixedFees =
            [
                new CustomFixedFee
                {
                    Amount = 1,
                    DenominatingTokenId = new TokenId(0, 0, 0)
                },
                new CustomFixedFee
                {
                    Amount = 2,
                    DenominatingTokenId = new TokenId(0, 0, 1)
                },
                new CustomFixedFee
                {
                    Amount = 3,
                    DenominatingTokenId = new TokenId(0, 0, 2)
                },
            ];

            CustomFixedFee customFixedFeeToBeAdded = new CustomFixedFee
            {
                Amount = 4,
                DenominatingTokenId = new TokenId(0, 0, 3)
            };
            IList<CustomFixedFee> expectedCustomFees = [ .. customFixedFees, customFixedFeeToBeAdded];
            TopicCreateTransaction topicCreateTransaction = new () { CustomFees = [.. customFixedFees] };
            topicCreateTransaction.CustomFees.Add(customFixedFeeToBeAdded);
            Assert.Equal(topicCreateTransaction.CustomFees, expectedCustomFees);
        }

        public virtual void ShouldAddTopicCustomFeeToEmptyList()
        {
            CustomFixedFee customFixedFeeToBeAdded = new()
            {
                Amount = 4,
                DenominatingTokenId = new TokenId(0, 0, 3)
            };
            TopicCreateTransaction topicCreateTransaction = new();
            topicCreateTransaction.CustomFees.Add(customFixedFeeToBeAdded);

            Assert.Equal(topicCreateTransaction.CustomFees, [customFixedFeeToBeAdded]);
        }
    }
}