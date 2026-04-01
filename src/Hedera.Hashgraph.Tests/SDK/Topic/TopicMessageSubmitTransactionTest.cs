// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    public class TopicMessageSubmitTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TopicId testTopicId = new (0, 6, 9);
        private static readonly byte[] testMessageBytes = [ 0x04, 0x05, 0x06 ];
        private static readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        private TopicMessageSubmitTransaction SpawnTestTransactionString()
        {
            return new TopicMessageSubmitTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                TopicId = testTopicId,
                Message = ByteString.CopyFrom(testMessageBytes),
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        private TopicMessageSubmitTransaction SpawnTestTransactionBytes()
        {
            return new TopicMessageSubmitTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                TopicId = testTopicId,
                Message = ByteString.CopyFrom(testMessageBytes),
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TopicMessageSubmitTransaction();
            var tx2 = ITransaction.FromBytes(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                ConsensusSubmitMessage = new Proto.ConsensusSubmitMessageTransactionBody { }
            };

            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TopicMessageSubmitTransaction>(tx);
        }
        [Fact]
        public virtual void ConstructTopicMessageSubmitTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.ConsensusSubmitMessageTransactionBody
            {
                TopicID = testTopicId.ToProtobuf(),
                Message = ByteString.CopyFrom(testMessageBytes),
            };
            var tx = new Proto.TransactionBody
            {
                ConsensusSubmitMessage = transactionBody
            };

            var topicSubmitMessageTransaction = new TopicMessageSubmitTransaction(tx);
            Assert.Equal(topicSubmitMessageTransaction.TopicId, testTopicId);
        }
        [Fact]
        public virtual void GetSetTopicId()
        {
            var topicSubmitMessageTransaction = new TopicMessageSubmitTransaction
            {
                TopicId = testTopicId
            };

            Assert.Equal(topicSubmitMessageTransaction.TopicId, testTopicId);
        }
        [Fact]
        public virtual void GetSetTopicIdFrozen()
        {
            var tx = SpawnTestTransactionString();
            Assert.Throws<InvalidOperationException>(() => tx.TopicId = testTopicId);
        }
        [Fact]
        public virtual void GetSetMessage()
        {
            var topicSubmitMessageTransactionString = new TopicMessageSubmitTransaction
            {
                Message = ByteString.CopyFrom(testMessageBytes)
            };
            var topicSubmitMessageTransactionBytes = new TopicMessageSubmitTransaction
            {
                Message = ByteString.CopyFrom(testMessageBytes)
            };

            Assert.Equal(topicSubmitMessageTransactionString.Message.ToByteArray(), testMessageBytes);
            Assert.Equal(topicSubmitMessageTransactionBytes.Message.ToByteArray(), testMessageBytes);
        }
        [Fact]
        public virtual void GetSetMessageFrozen()
        {
            var topicSubmitMessageTransactionString = SpawnTestTransactionString();
            var topicSubmitMessageTransactionBytes = SpawnTestTransactionBytes();

            Assert.Throws<InvalidOperationException>(() => topicSubmitMessageTransactionString.Message = ByteString.CopyFrom(testMessageBytes));
            Assert.Throws<InvalidOperationException>(() => topicSubmitMessageTransactionBytes.Message = ByteString.CopyFrom(testMessageBytes));
        }
        [Fact]
        public virtual void ShouldSetCustomFeeLimits()
        {
            var customFeeLimits = new List<CustomFeeLimit>
            {
                new ()
                {
                    PayerId = new AccountId(0, 0, 1),
                    CustomFees = [ new CustomFixedFee
                    {
                        Amount = 1,
                        DenominatingTokenId = new TokenId(0, 0, 1)
                    } ]
                },
                new ()
                {
                    PayerId = new AccountId(0, 0, 2),
                    CustomFees = [ new CustomFixedFee
                    {
                        Amount = 1,
                        DenominatingTokenId = new TokenId(0, 0, 2)
                    } ]
                }
            };
            var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction { CustomFeeLimits = customFeeLimits };
            Assert.Equal(topicMessageSubmitTransaction.CustomFeeLimits, customFeeLimits);
        }
        [Fact]
        public virtual void ShouldAddCustomFeeLimitToList()
        {
            var customFeeLimits = new List<CustomFeeLimit>
            {
                new ()
                {
                    PayerId = new AccountId(0, 0, 1),
                    CustomFees = [ new CustomFixedFee
                    {
                        Amount = 1,
                        DenominatingTokenId = new TokenId(0, 0, 1)
                    } ]
                },
                new ()
                {
                    PayerId = new AccountId(0, 0, 2),
                    CustomFees = [ new CustomFixedFee
                    {
                        Amount = 1,
                        DenominatingTokenId = new TokenId(0, 0, 2)
                    } ]
                }
            };

            var customFeeLimitToBeAdded = new CustomFeeLimit
            {
                PayerId = new AccountId(0, 0, 3),
                CustomFees = [ new CustomFixedFee
                {
                    Amount = 3,
                    DenominatingTokenId = new TokenId(0, 0, 3)
                } ]
            };

            List<CustomFeeLimit> expectedCustomFeeLimits = [.. customFeeLimits];
            expectedCustomFeeLimits.Add(customFeeLimitToBeAdded);
            var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction
            {
                CustomFeeLimits = [.. customFeeLimits, customFeeLimitToBeAdded]
            };

            Assert.Equal(topicMessageSubmitTransaction.CustomFeeLimits, expectedCustomFeeLimits);
        }
        [Fact]
        public virtual void ShouldAddCustomFeeLimitToEmptyList()
        {
            var customFeeLimitToBeAdded = new CustomFeeLimit
            {
                PayerId = new AccountId(0, 0, 3),
                CustomFees = [ new CustomFixedFee
                {
                    Amount = 3,
                    DenominatingTokenId = new TokenId(0, 0, 3)
                } ]
            };
            var topicMessageSubmitTransaction = new TopicMessageSubmitTransaction { CustomFeeLimits = [customFeeLimitToBeAdded] };

            Assert.Equal(topicMessageSubmitTransaction.CustomFeeLimits, [customFeeLimitToBeAdded]);
        }
    }
}