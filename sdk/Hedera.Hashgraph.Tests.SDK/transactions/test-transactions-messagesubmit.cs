// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Transactions;

using System;

using VerifyXunit;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    /// <include file="test-transactions-messagesubmit.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Transactions.MessageSubmitTransactionTest"]' />
    public class MessageSubmitTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        [Fact]
        /// <include file="test-transactions-messagesubmit.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.MessageSubmitTransactionTest.ShouldBytes"]' />
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TopicMessageSubmitTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-transactions-messagesubmit.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.MessageSubmitTransactionTest.ShouldBytesNoSetters"]' />
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TopicMessageSubmitTransaction();
            var tx2 = Transaction.FromBytes<TopicMessageSubmitTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private TopicMessageSubmitTransaction SpawnTestTransaction()
        {
            return new TopicMessageSubmitTransaction()
			{
                NodeAccountIds = new (AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")),
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				TopicId = TopicId.FromString("0.0.5007"),
				Message = ByteString.CopyFromUtf8("hello"),
				MaxTransactionFee = Hbar.FromTinybars(100000)
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        /// <include file="test-transactions-messagesubmit.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.MessageSubmitTransactionTest.FromScheduledTransaction"]' />
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
                ConsensusSubmitMessage = new Proto.Services.ConsensusSubmitMessageTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TopicMessageSubmitTransaction>(transactionBody);

            Assert.IsType<TopicMessageSubmitTransaction>(tx);
        }
    }
}
