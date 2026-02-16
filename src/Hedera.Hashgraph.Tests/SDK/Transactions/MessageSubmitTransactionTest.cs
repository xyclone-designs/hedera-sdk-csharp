// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class MessageSubmitTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TopicMessageSubmitTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TopicMessageSubmitTransaction();
            var tx2 = Transaction.FromBytes<TopicMessageSubmitTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        private TopicMessageSubmitTransaction SpawnTestTransaction()
        {
            return new TopicMessageSubmitTransaction()
			{
                NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006") ],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				TopicId = TopicId.FromString("0.0.5007"),
				Message = ByteString.CopyFromUtf8("hello"),
				MaxTransactionFee = Hbar.FromTinybars(100000)
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                ConsensusSubmitMessage = new Proto.ConsensusSubmitMessageTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TopicMessageSubmitTransaction>(transactionBody);

            Assert.IsType<TopicMessageSubmitTransaction>(tx);
        }
    }
}