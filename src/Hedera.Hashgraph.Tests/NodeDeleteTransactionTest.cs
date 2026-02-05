// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
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
    public class NodeDeleteTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly long TEST_NODE_ID = 420;
        readonly Instant TEST_VALID_START = Instant.OfEpochSecond(1554158542);
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

        private NodeDeleteTransaction SpawnTestTransaction()
        {
            return new NodeDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START)).SetNodeId(TEST_NODE_ID).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(TEST_PRIVATE_KEY);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = NodeDeleteTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new NodeDeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetNodeDelete(NodeDeleteTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<NodeDeleteTransaction>(tx);
        }

        virtual void ConstructNodeDeleteTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyBuilder = NodeDeleteTransactionBody.NewBuilder();
            transactionBodyBuilder.SetNodeId(TEST_NODE_ID);
            var tx = TransactionBody.NewBuilder().SetNodeDelete(transactionBodyBuilder.Build()).Build();
            var nodeDeleteTransaction = new NodeDeleteTransaction(tx);
            Assert.Equal(nodeDeleteTransaction.GetNodeId(), TEST_NODE_ID);
        }

        virtual void GetSetNodeId()
        {
            var nodeDeleteTransaction = new NodeDeleteTransaction().SetNodeId(TEST_NODE_ID);
            Assert.Equal(nodeDeleteTransaction.GetNodeId(), TEST_NODE_ID);
        }

        virtual void GetSetNodeIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetNodeId(TEST_NODE_ID));
        }

        virtual void ShouldFreezeSuccessfullyWhenNodeIdIsSet()
        {
            Instant VALID_START = Instant.OfEpochSecond(1596210382);
            AccountId ACCOUNT_ID = AccountId.FromString("0.6.9");
            var transaction = new NodeDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.3"))).SetTransactionId(TransactionId.WithValidStart(ACCOUNT_ID, VALID_START)).SetNodeId(420);
            AssertThatCode(() => transaction.FreezeWith(null)).DoesNotThrowAnyException();
            Assert.Equal(transaction.GetNodeId(), 420);
        }

        virtual void ShouldThrowErrorWhenFreezingWithoutSettingNodeId()
        {
            Instant VALID_START = Instant.OfEpochSecond(1596210382);
            AccountId ACCOUNT_ID = AccountId.FromString("0.6.9");
            var transaction = new NodeDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.3"))).SetTransactionId(TransactionId.WithValidStart(ACCOUNT_ID, VALID_START));
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.FreezeWith(null));
            Assert.Equal(exception.GetMessage(), "NodeDeleteTransaction: 'nodeId' must be explicitly set before calling freeze().");
        }

        virtual void ShouldThrowErrorWhenFreezingWithZeroNodeId()
        {
            Instant VALID_START = Instant.OfEpochSecond(1596210382);
            AccountId ACCOUNT_ID = AccountId.FromString("0.6.9");
            var transaction = new NodeDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.3"))).SetTransactionId(TransactionId.WithValidStart(ACCOUNT_ID, VALID_START));
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.FreezeWith(null));
            Assert.Equal(exception.GetMessage(), "NodeDeleteTransaction: 'nodeId' must be explicitly set before calling freeze().");
        }

        virtual void ShouldFreezeSuccessfullyWithActualClientWhenNodeIdIsSet()
        {
            Instant VALID_START = Instant.OfEpochSecond(1596210382);
            AccountId ACCOUNT_ID = AccountId.FromString("0.6.9");
            var transaction = new NodeDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.3"))).SetTransactionId(TransactionId.WithValidStart(ACCOUNT_ID, VALID_START)).SetNodeId(420);
            var mockClient = Client.ForTestnet();
            AssertThatCode(() => transaction.FreezeWith(mockClient)).DoesNotThrowAnyException();
            Assert.Equal(transaction.GetNodeId(), 420);
        }

        virtual void ShouldThrowErrorWhenGettingNodeIdWithoutSettingIt()
        {
            var transaction = new NodeDeleteTransaction();
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.GetNodeId());
            Assert.Equal(exception.GetMessage(), "NodeDeleteTransaction: 'nodeId' has not been set");
        }

        virtual void ShouldThrowErrorWhenSettingNegativeNodeId()
        {
            var transaction = new NodeDeleteTransaction();
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetNodeId(-1));
            Assert.Equal(exception.GetMessage(), "NodeDeleteTransaction: 'nodeId' must be non-negative");
        }

        virtual void ShouldAllowSettingNodeIdToZero()
        {
            var transaction = new NodeDeleteTransaction().SetNodeId(0);
            Assert.Equal(transaction.GetNodeId(), 0);
        }
    }
}