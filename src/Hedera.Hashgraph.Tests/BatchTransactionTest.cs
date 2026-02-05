// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
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
    class BatchTransactionTest
    {
        private static readonly PrivateKey privateKeyED25519 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PrivateKey privateKeyECDSA = PrivateKey.FromStringECDSA("7f109a9e3b0d8ecfba9cc23a3614433ce0fa7ddcc80f2a8f10b222179a5a80d6");
        static readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        private static readonly IList<Transaction> INNER_TRANSACTIONS = List.Of(SpawnTestTransactionAccountCreate(), SpawnTestTransactionAccountCreate(), SpawnTestTransactionAccountCreate());
        private static AccountCreateTransaction SpawnTestTransactionAccountCreate()
        {
            return new AccountCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetKeyWithAlias(privateKeyECDSA).SetKeyWithAlias(privateKeyED25519, privateKeyECDSA).SetKeyWithoutAlias(privateKeyED25519).SetInitialBalance(Hbar.FromTinybars(450)).SetAccountMemo("some memo").SetReceiverSignatureRequired(true).SetAutoRenewPeriod(Duration.OfHours(10)).SetStakedAccountId(AccountId.FromString("0.0.3")).SetAlias("0x5c562e90feaf0eebd33ea75d21024f249d451417").SetMaxAutomaticTokenAssociations(100).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetBatchKey(privateKeyECDSA).Freeze().Sign(privateKeyED25519);
        }

        private BatchTransaction SpawnTestTransaction()
        {
            var batchKey = PrivateKey.GenerateECDSA();
            return new BatchTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetInnerTransactions(INNER_TRANSACTIONS).Freeze().Sign(batchKey);
        }

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

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = BatchTransaction.FromBytes(tx.ToBytes());
            AssertThat(tx2).HasToString(tx.ToString());
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new BatchTransaction();
            var tx2 = BatchTransaction.FromBytes(tx.ToBytes());
            AssertThat(tx2).HasToString(tx.ToString());
        }

        virtual void GetInnerTransactionsShouldReturnCorrectTransactions()
        {
            var batchTransaction = SpawnTestTransaction();
            Assert.Equal(batchTransaction.GetInnerTransactions()).IsNotNull().HasSize(3, INNER_TRANSACTIONS);
        }

        virtual void SetInnerTransactionsShouldUpdateTransactions()
        {
            var batchTransaction = new BatchTransaction();
            IList<Transaction> newInnerTransactions = List.Of(SpawnTestTransactionAccountCreate(), SpawnTestTransactionAccountCreate());
            batchTransaction.SetInnerTransactions(newInnerTransactions);
            Assert.Equal(batchTransaction.GetInnerTransactions()).IsNotNull().HasSize(2, newInnerTransactions);
        }

        virtual void AddInnerTransactionShouldAppendTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var newTransaction = SpawnTestTransactionAccountCreate();
            batchTransaction.AddInnerTransaction(newTransaction);
            AssertThat(batchTransaction.GetInnerTransactions()).IsNotNull().HasSize(1).Contains(newTransaction);
        }

        virtual void GetInnerTransactionIdsShouldReturnCorrectIds()
        {
            var batchTransaction = SpawnTestTransaction();
            var expectedTransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart);
            var transactionIds = batchTransaction.GetInnerTransactionIds();
            Assert.Equal(transactionIds).IsNotNull().HasSize(3).AllSatisfy((id) => AssertThat(id, expectedTransactionId));
        }

        virtual void ShouldAllowChainedSetters()
        {
            var batchTransaction = new BatchTransaction().SetNodeAccountIds(Collections.SingletonList(AccountId.FromString("0.0.5005"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).AddInnerTransaction(SpawnTestTransactionAccountCreate()).Freeze();
            AssertThat(batchTransaction.GetInnerTransactions()).HasSize(1);
            AssertThat(batchTransaction.GetNodeAccountIds()).HasSize(1);
            AssertThat(batchTransaction.GetTransactionId()).IsNotNull();
        }

        virtual void ShouldRejectFreezeTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var freezeTransaction = new FreezeTransaction().SetStartTime(Instant.Now()).SetFreezeType(FreezeType.FREEZE_ONLY).SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).Freeze();
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(freezeTransaction)).WithMessageContaining("FreezeTransaction is not allowed in a batch transaction");
        }

        virtual void ShouldRejectBatchTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var innerBatchTransaction = new BatchTransaction().SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).Freeze();
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(innerBatchTransaction)).WithMessageContaining("BatchTransaction is not allowed in a batch transaction");
        }

        virtual void ShouldRejectBlacklistedTransactionInList()
        {
            var batchTransaction = new BatchTransaction();
            var validTransaction = SpawnTestTransactionAccountCreate();
            var freezeTransaction = new FreezeTransaction().SetStartTime(Instant.Now()).SetFreezeType(FreezeType.FREEZE_ONLY).SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).Freeze();
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => batchTransaction.SetInnerTransactions(List.Of(validTransaction, freezeTransaction))).WithMessageContaining("FreezeTransaction is not allowed in a batch transaction");
        }

        virtual void ShouldRejectNullTransaction()
        {
            var batchTransaction = new BatchTransaction();
            AssertThatExceptionOfType(typeof(NullReferenceException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(null));
        }

        virtual void ShouldRejectNullTransactionList()
        {
            var batchTransaction = new BatchTransaction();
            AssertThatExceptionOfType(typeof(NullReferenceException)).IsThrownBy(() => batchTransaction.SetInnerTransactions(null));
        }

        virtual void ShouldRejectUnfrozenTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var unfrozenTransaction = new AccountCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart));
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(unfrozenTransaction)).WithMessageContaining("Inner transaction should be frozen");
        }

        virtual void ShouldRejectTransactionAfterFreeze()
        {
            var batchTransaction = new BatchTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).Freeze();
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(SpawnTestTransactionAccountCreate())).WithMessageContaining("transaction is immutable");
        }

        virtual void ShouldRejectTransactionListAfterFreeze()
        {
            var batchTransaction = new BatchTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).Freeze();
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => batchTransaction.SetInnerTransactions(INNER_TRANSACTIONS)).WithMessageContaining("transaction is immutable");
        }

        virtual void ShouldAllowEmptyTransactionListBeforeExecution()
        {
            var batchTransaction = new BatchTransaction();
            batchTransaction.SetInnerTransactions(Collections.EmptyList());
            Assert.Empty(batchTransaction.GetInnerTransactions()).IsNotNull();
        }

        virtual void ShouldPreserveTransactionOrder()
        {
            var batchTransaction = new BatchTransaction();
            var transaction1 = SpawnTestTransactionAccountCreate();
            var transaction2 = SpawnTestTransactionAccountCreate();
            var transaction3 = SpawnTestTransactionAccountCreate();
            IList<Transaction> transactions = Arrays.AsList(transaction1, transaction2, transaction3);
            batchTransaction.SetInnerTransactions(transactions);
            AssertThat(batchTransaction.GetInnerTransactions()).ContainsExactly(transaction1, transaction2, transaction3);
        }

        virtual void ShouldCreateDefensiveCopyOfTransactionList()
        {
            var batchTransaction = new BatchTransaction();
            var mutableList = new List(INNER_TRANSACTIONS);
            batchTransaction.SetInnerTransactions(mutableList);
            mutableList.Clear();
            Assert.Equal(batchTransaction.GetInnerTransactions()).IsNotNull().HasSize(3, INNER_TRANSACTIONS);
        }

        virtual void ShouldRejectTransactionWithoutBatchKey()
        {
            var batchTransaction = new BatchTransaction();
            var transactionWithoutBatchKey = new AccountCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).Freeze();
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(transactionWithoutBatchKey)).WithMessageContaining("Batch key needs to be set");
        }

        virtual void ShouldValidateAllTransactionsInList()
        {
            var batchTransaction = new BatchTransaction();
            var validTransaction = SpawnTestTransactionAccountCreate();
            var transactionWithoutBatchKey = new AccountCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).Freeze();
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => batchTransaction.SetInnerTransactions(List.Of(validTransaction, transactionWithoutBatchKey))).WithMessageContaining("Batch key needs to be set");
        }

        virtual void ShouldValidateMultipleConditions()
        {
            var batchTransaction = new BatchTransaction();

            // Test unfrozen transaction with no batch key
            var unfrozenTransactionWithoutBatchKey = new AccountCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart));
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(unfrozenTransactionWithoutBatchKey)).WithMessageContaining("Inner transaction should be frozen");

            // Test frozen transaction with no batch key
            var frozenTransactionWithoutBatchKey = unfrozenTransactionWithoutBatchKey.Freeze();
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(frozenTransactionWithoutBatchKey)).WithMessageContaining("Batch key needs to be set");

            // Test blacklisted transaction with batch key
            var blacklistedTransaction = new FreezeTransaction().SetStartTime(Instant.Now()).SetFreezeType(FreezeType.FREEZE_ONLY).SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetBatchKey(privateKeyECDSA).Freeze();
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(blacklistedTransaction)).WithMessageContaining("FreezeTransaction is not allowed in a batch transaction");
        }

        virtual void ShouldAcceptValidTransaction()
        {
            var batchTransaction = new BatchTransaction();
            var validTransaction = new AccountCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetBatchKey(privateKeyECDSA).Freeze();
            batchTransaction.AddInnerTransaction(validTransaction);
            AssertThat(batchTransaction.GetInnerTransactions()).IsNotNull().HasSize(1).Contains(validTransaction);
        }

        virtual void ShouldValidateTransactionStateInOrder()
        {
            var batchTransaction = new BatchTransaction();
            var transaction = new AccountCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart));

            // First check should be for frozen state
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(transaction)).WithMessageContaining("Inner transaction should be frozen");

            // After freezing, next check should be for batch key
            var frozenTransaction = transaction.Freeze();
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => batchTransaction.AddInnerTransaction(frozenTransaction)).WithMessageContaining("Batch key needs to be set");
        }
    }
}