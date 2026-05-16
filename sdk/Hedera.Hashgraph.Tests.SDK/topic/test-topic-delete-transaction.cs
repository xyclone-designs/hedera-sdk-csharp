// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptocurrency;

using VerifyXunit;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    /// <include file="test-topic-delete-transaction.ts.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Topic.TopicDeleteTransactionTest"]' />
    public class TopicDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
 
        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private TopicDeleteTransaction SpawnTestTransaction()
        {
            return new TopicDeleteTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                TopicId = TopicId.FromString("0.0.5007"),
                MaxTransactionFee = Hbar.FromTinybars(100000),
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        /// <include file="test-topic-delete-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Topic.TopicDeleteTransactionTest.ShouldBytesNoSetters"]' />
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TopicDeleteTransaction();
            var tx2 = ITransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-topic-delete-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Topic.TopicDeleteTransactionTest.ShouldBytes"]' />
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TopicDeleteTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-topic-delete-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Topic.TopicDeleteTransactionTest.FromScheduledTransaction"]' />
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
                ConsensusDeleteTopic = new Proto.Services.ConsensusDeleteTopicTransactionBody { }
            };
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TopicDeleteTransaction>(tx);
        }
    }
}
