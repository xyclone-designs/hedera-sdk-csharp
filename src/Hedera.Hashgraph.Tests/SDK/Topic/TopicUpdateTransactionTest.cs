// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Fees;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    public class TopicUpdateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PublicKey testAdminKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e11").GetPublicKey();
        private static readonly PublicKey testSubmitKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e12").GetPublicKey();
        private static readonly TopicId testTopicId = TopicId.FromString("0.0.5007");
        private static readonly string testTopicMemo = "test memo";
        private static readonly TimeSpan testAutoRenewPeriod = TimeSpan.FromHours(10);
        private static readonly DateTimeOffset testExpirationTime = DateTimeOffset.UtcNow;
        private static readonly AccountId testAutoRenewAccountId = AccountId.FromString("8.8.8");
        private static readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ClearShouldSerialize()
        {
            Verifier.Verify(new TopicUpdateTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                TopicId = testTopicId,
                AdminKey = null,
                AutoRenewAccountId = null,
                SubmitKey = null,
                TopicMemo = null,
            
            }.Freeze().Sign(unusedPrivateKey).ToString());
        }

        public virtual void SetShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TopicUpdateTransaction();
            var tx2 = ITransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TopicUpdateTransaction SpawnTestTransaction()
        {
            return new TopicUpdateTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                TopicId = testTopicId,
                AdminKey = testAdminKey,
                AutoRenewAccountId = testAutoRenewAccountId,
                AutoRenewPeriod = testAutoRenewPeriod,
                SubmitKey = testSubmitKey,
                TopicMemo = testTopicMemo,
                ExpirationTime = validStart

            }.Freeze().Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TopicUpdateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                ConsensusUpdateTopic = new Proto.ConsensusUpdateTopicTransactionBody()
            };
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TopicUpdateTransaction>(tx);
        }
        [Fact]
        public virtual void ConstructTopicUpdateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.ConsensusUpdateTopicTransactionBody
            {
                TopicID = testTopicId.ToProtobuf(),
                Memo = testTopicMemo,
                ExpirationTime = new Proto.Timestamp { Seconds = testExpirationTime.ToUnixTimeSeconds() },
                AdminKey = testAdminKey.ToProtobufKey(),
                SubmitKey = testSubmitKey.ToProtobufKey(),
                AutoRenewPeriod = new Proto.Duration { Seconds = (long)testAutoRenewPeriod.TotalSeconds },
                AutoRenewAccount = testAutoRenewAccountId.ToProtobuf()
            };
            var tx = new Proto.TransactionBody { ConsensusUpdateTopic = transactionBody };
            var topicUpdateTransaction = new TopicUpdateTransaction(tx);

            Assert.Equal(topicUpdateTransaction.TopicId, testTopicId);
            Assert.Equal(topicUpdateTransaction.TopicMemo, testTopicMemo);
            Assert.Equal(topicUpdateTransaction.ExpirationTime?.ToUnixTimeSeconds(), testExpirationTime.ToUnixTimeSeconds());
            Assert.Equal(topicUpdateTransaction.AdminKey, testAdminKey);
            Assert.Equal(topicUpdateTransaction.SubmitKey, testSubmitKey);
            Assert.Equal(topicUpdateTransaction.AutoRenewPeriod?.TotalSeconds, testAutoRenewPeriod.TotalSeconds);
            Assert.Equal(topicUpdateTransaction.AutoRenewAccountId, testAutoRenewAccountId);
        }

        // doesn't throw an exception as opposed to C++ sdk
        public virtual void ConstructTopicUpdateTransactionFromWrongTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.CryptoDeleteTransactionBody { };
            var tx = new Proto.TransactionBody { CryptoDelete = transactionBody };
            new TopicUpdateTransaction(tx);
        }
        [Fact]
        public virtual void GetSetTopicId()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { TopicId = testTopicId };
            Assert.Equal(topicUpdateTransaction.TopicId, testTopicId);
        }
        [Fact]
        public virtual void GetSetTopicIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TopicId = testTopicId);
        }
        [Fact]
        public virtual void GetSetTopicMemo()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { TopicMemo = testTopicMemo };
            Assert.Equal(topicUpdateTransaction.TopicMemo, testTopicMemo);
        }
        [Fact]
        public virtual void GetSetTopicMemoFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TopicMemo = testTopicMemo);
        }
        [Fact]
        public virtual void ClearTopicMemo()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { TopicMemo = testTopicMemo };
            topicUpdateTransaction.TopicMemo = null;
            Assert.Empty(topicUpdateTransaction.TopicMemo);
        }
        [Fact]
        public virtual void ClearTopicMemoFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TopicMemo = null);
        }
        [Fact]
        public virtual void GetSetExpirationTime()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { ExpirationTime = testExpirationTime };
            Assert.Equal(topicUpdateTransaction.ExpirationTime, testExpirationTime);
        }
        [Fact]
        public virtual void GetSetExpirationTimeFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.ExpirationTime = testExpirationTime);
        }
        [Fact]
        public virtual void GetSetAdminKey()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { AdminKey = testAdminKey };
            Assert.Equal(topicUpdateTransaction.AdminKey, testAdminKey);
        }
        [Fact]
        public virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AdminKey = testAdminKey);
        }
        [Fact]
        public virtual void ClearAdminKey()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { AdminKey = testAdminKey };
            topicUpdateTransaction.AdminKey = null;
            Assert.Equal(topicUpdateTransaction.AdminKey, new KeyList());
        }
        [Fact]
        public virtual void ClearAdminKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AdminKey = null);
        }
        [Fact]
        public virtual void GetSetSubmitKey()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { SubmitKey = testSubmitKey };
            Assert.Equal(topicUpdateTransaction.SubmitKey, testSubmitKey);
        }
        [Fact]
        public virtual void GetSetSubmitKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SubmitKey = testSubmitKey);
        }
        [Fact]
        public virtual void ClearSubmitKey()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { SubmitKey = testSubmitKey };
            topicUpdateTransaction.SubmitKey = null;
            Assert.Equal(topicUpdateTransaction.SubmitKey, new KeyList());
        }
        [Fact]
        public virtual void ClearSubmitKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SubmitKey = null);
        }
        [Fact]
        public virtual void GetSetAutoRenewPeriod()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { AutoRenewPeriod = testAutoRenewPeriod };
            Assert.Equal(topicUpdateTransaction.AutoRenewPeriod, testAutoRenewPeriod);
        }
        [Fact]
        public virtual void GetSetAutoRenewPeriodFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AutoRenewPeriod = testAutoRenewPeriod);
        }
        [Fact]
        public virtual void GetSetAutoRenewAccountId()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { AutoRenewAccountId = testAutoRenewAccountId };
            Assert.Equal(topicUpdateTransaction.AutoRenewAccountId, testAutoRenewAccountId);
        }
        [Fact]
        public virtual void GetSetAutoRenewAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AutoRenewAccountId = testAutoRenewAccountId);
        }
        [Fact]
        public virtual void ClearAutoRenewAccountId()
        {
            var topicUpdateTransaction = new TopicUpdateTransaction { AutoRenewAccountId = testAutoRenewAccountId };
            topicUpdateTransaction.AutoRenewAccountId = null;
            Assert.Equal(topicUpdateTransaction.AutoRenewAccountId, new AccountId(0, 0, 0));
        }
        [Fact]
        public virtual void ClearAutoRenewAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AutoRenewAccountId = null);
        }
        [Fact]
        public virtual void ShouldSetFeeScheduleKey()
        {
            PrivateKey feeScheduleKey = PrivateKey.GenerateECDSA();
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction
            {
                FeeScheduleKey = feeScheduleKey
            };
            Assert.Equal(topicUpdateTransaction.FeeScheduleKey.ToString(), feeScheduleKey.ToString());
        }
        [Fact]
        public virtual void ShouldSetFeeExemptKeys()
        {
            IList<PrivateKey> feeExemptKeys = [PrivateKey.GenerateECDSA(), PrivateKey.GenerateECDSA()];
            TopicUpdateTransaction topicUpdateTransaction = new()
            {
                FeeExemptKeys = [.. feeExemptKeys] 
            };
            Assert.Equal(topicUpdateTransaction.FeeExemptKeys, feeExemptKeys);
        }
        [Fact]
        public virtual void ShouldAddFeeExemptKeyToEmptyList()
        {
            TopicUpdateTransaction topicUpdateTransaction = new TopicUpdateTransaction();
            PrivateKey feeExemptKeyToBeAdded = PrivateKey.GenerateECDSA();
            topicUpdateTransaction.FeeExemptKeys.Add(feeExemptKeyToBeAdded);
            Assert.Equal(topicUpdateTransaction.FeeExemptKeys.Count, 1);
            Assert.Equal(topicUpdateTransaction.FeeExemptKeys, [feeExemptKeyToBeAdded]);
        }
        [Fact]
        public virtual void ShouldAddFeeExemptKeyToList()
        {
            PrivateKey feeExemptKey = PrivateKey.GenerateECDSA();
            TopicUpdateTransaction topicUpdateTransaction = new()
            {
                FeeExemptKeys = [feeExemptKey]
            };

            PrivateKey feeExemptKeyToBeAdded = PrivateKey.GenerateECDSA();
            topicUpdateTransaction.FeeExemptKeys.Add(feeExemptKeyToBeAdded);
            Assert.Equal(topicUpdateTransaction.FeeExemptKeys.Count, 2);
            Assert.Equal(topicUpdateTransaction.FeeExemptKeys, [feeExemptKey, feeExemptKeyToBeAdded]);
        }
        [Fact]
        public virtual void ShouldSetCustomFees()
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
                }
            ];
            TopicUpdateTransaction topicUpdateTransaction = new()
            {
                CustomFees = [.. customFixedFees]
            };
            Assert.Equal(topicUpdateTransaction.CustomFees.Count, 3);
            Assert.Equal(topicUpdateTransaction.CustomFees, customFixedFees);
        }
        [Fact]
        public virtual void ShouldAddCustomFeeToList()
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
                }
            ];
            CustomFixedFee customFixedFeeToBeAdded = new CustomFixedFee
            {
                Amount = 4,
                DenominatingTokenId = new TokenId(0, 0, 3)
            };
            IList<CustomFixedFee> expectedCustomFees = [];
            expectedCustomFees.Add(customFixedFeeToBeAdded);
            TopicUpdateTransaction topicUpdateTransaction = new()
            {
                CustomFees = [.. customFixedFees] 
            };
            topicUpdateTransaction.CustomFees.Add(customFixedFeeToBeAdded);
            Assert.Equal(topicUpdateTransaction.CustomFees.Count, 4);
            Assert.Equal(topicUpdateTransaction.CustomFees, expectedCustomFees);
        }
        [Fact]
        public virtual void ShouldAddCustomFeeToEmptyList()
        {
            CustomFixedFee customFixedFeeToBeAdded = new CustomFixedFee
            {
                Amount = 4,
                DenominatingTokenId = new TokenId(0, 0, 3)
            };
            TopicUpdateTransaction topicUpdateTransaction = new ();
            topicUpdateTransaction.CustomFees.Add(customFixedFeeToBeAdded);
            Assert.Equal(topicUpdateTransaction.CustomFees.Count, 1);
            Assert.Equal(topicUpdateTransaction.CustomFees, [customFixedFeeToBeAdded]);
        }
        [Fact]
        public virtual void ShouldClearCustomFees()
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
                }
            ];
            TopicUpdateTransaction topicUpdateTransaction = new()
            {
                CustomFees = [.. customFixedFees]
            };
            topicUpdateTransaction.CustomFees.Clear();
            Assert.Empty(topicUpdateTransaction.CustomFees);
        }
    }
}