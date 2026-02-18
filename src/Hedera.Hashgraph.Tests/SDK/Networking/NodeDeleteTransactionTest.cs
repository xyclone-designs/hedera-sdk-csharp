// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class NodeDeleteTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly ulong TEST_NODE_ID = 420;
        readonly DateTimeOffset TEST_VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }
        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        private NodeDeleteTransaction SpawnTestTransaction()
        {
            return new NodeDeleteTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START),
				NodeId = TEST_NODE_ID,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(TEST_PRIVATE_KEY);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new NodeDeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				NodeDelete = new Proto.NodeDeleteTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<NodeDeleteTransaction>(tx);
        }

        public virtual void ConstructNodeDeleteTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyBuilder = new Proto.NodeDeleteTransactionBody();
            transactionBodyBuilder.NodeId = TEST_NODE_ID;
            var tx = new Proto.TransactionBody
            {
				NodeDelete = transactionBodyBuilder
			};
            var nodeDeleteTransaction = new NodeDeleteTransaction(tx);
            Assert.Equal(nodeDeleteTransaction.NodeId, TEST_NODE_ID);
        }

        public virtual void GetSetNodeId()
        {
            var nodeDeleteTransaction = new NodeDeleteTransaction
            {
				NodeId = TEST_NODE_ID
			};
            Assert.Equal(nodeDeleteTransaction.NodeId, TEST_NODE_ID);
        }

        public virtual void GetSetNodeIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.NodeId = TEST_NODE_ID);
        }

        public virtual void ShouldFreezeSuccessfullyWhenNodeIdIsSet()
        {
            DateTimeOffset VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1596210382);
            AccountId ACCOUNT_ID = AccountId.FromString("0.6.9");
            var transaction = new NodeDeleteTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.3")],
                TransactionId = TransactionId.WithValidStart(ACCOUNT_ID, VALID_START),
                NodeId = 420,
            };

            AssertThatCode(() => transaction.FreezeWith(null)).DoesNotThrowAnyException();
            Assert.Equal(transaction.NodeId, 420);
        }

        public virtual void ShouldThrowErrorWhenFreezingWithoutSettingNodeId()
        {
            DateTimeOffset VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1596210382);
            AccountId ACCOUNT_ID = AccountId.FromString("0.6.9");
            var transaction = new NodeDeleteTransaction
            {
                NodeAccountIds = [ AccountId.FromString("0.0.3")],
                TransactionId = TransactionId.WithValidStart(ACCOUNT_ID, VALID_START)
            };
            var exception = Assert.Throws<InvalidOperationException>(() => transaction.FreezeWith(null));
            Assert.Equal(exception.Message, "NodeDeleteTransaction: 'nodeId' must be explicitly set before calling freeze().");
        }

        public virtual void ShouldThrowErrorWhenFreezingWithZeroNodeId()
        {
            DateTimeOffset VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1596210382);
            AccountId ACCOUNT_ID = AccountId.FromString("0.6.9");
            var transaction = new NodeDeleteTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.3")],
				TransactionId = TransactionId.WithValidStart(ACCOUNT_ID, VALID_START)

			};
            var exception = Assert.Throws<InvalidOperationException>(() => transaction.FreezeWith(null));
            Assert.Equal(exception.Message, "NodeDeleteTransaction: 'nodeId' must be explicitly set before calling freeze().");
        }

        public virtual void ShouldFreezeSuccessfullyWithActualClientWhenNodeIdIsSet()
        {
            DateTimeOffset VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1596210382);
            AccountId ACCOUNT_ID = AccountId.FromString("0.6.9");
            var transaction = new NodeDeleteTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.3")],
				TransactionId = TransactionId.WithValidStart(ACCOUNT_ID, VALID_START),
				NodeId = 420
			};
            var mockClient = Client.ForTestnet();
            transaction.FreezeWith(mockClient); //.DoesNotThrowAnyException();
            Assert.Equal(transaction.NodeId, 420);
        }

        public virtual void ShouldThrowErrorWhenGettingNodeIdWithoutSettingIt()
        {
            var transaction = new NodeDeleteTransaction();
            var exception = Assert.Throws<InvalidOperationException>(() => transaction.NodeId);
            Assert.Equal(exception.Message, "NodeDeleteTransaction: 'nodeId' has not been set");
        }


        public virtual void ShouldAllowSettingNodeIdToZero()
        {
            var transaction = new NodeDeleteTransaction
            {
				NodeId = 0
			};

            Assert.Equal(transaction.NodeId, 0);
        }
    }
}