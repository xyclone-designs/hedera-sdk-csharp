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
    public class SystemDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly FileId testFileId = FileId.FromString("4.2.0");
        private static readonly ContractId testContractId = ContractId.FromString("0.6.9");
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void ShouldSerializeFile()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionFile().ToString()).ToMatchSnapshot();
        }

        private SystemDeleteTransaction SpawnTestTransactionFile()
        {
            return new SystemDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetFileId(FileId.FromString("0.0.444")).SetExpirationTime(validStart).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldSerializeContract()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionContract().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new SystemDeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private SystemDeleteTransaction SpawnTestTransactionContract()
        {
            return new SystemDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetContractId(ContractId.FromString("0.0.444")).SetExpirationTime(validStart).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytesContract()
        {
            var tx = SpawnTestTransactionContract();
            var tx2 = ScheduleDeleteTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytesFile()
        {
            var tx = SpawnTestTransactionFile();
            var tx2 = SystemDeleteTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetSystemDelete(SystemDeleteTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<SystemDeleteTransaction>(tx);
        }

        virtual void ConstructSystemDeleteTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyWithFileId = SystemDeleteTransactionBody.NewBuilder().SetFileID(testFileId.ToProtobuf()).SetExpirationTime(TimestampSeconds.NewBuilder().SetSeconds(validStart.GetEpochSecond()));
            var transactionBodyWithContractId = SystemDeleteTransactionBody.NewBuilder().SetContractID(testContractId.ToProtobuf()).SetExpirationTime(TimestampSeconds.NewBuilder().SetSeconds(validStart.GetEpochSecond()));
            var txWithFileId = TransactionBody.NewBuilder().SetSystemDelete(transactionBodyWithFileId).Build();
            var systemDeleteTransactionWithFileId = new SystemDeleteTransaction(txWithFileId);
            var txWithContractId = TransactionBody.NewBuilder().SetSystemDelete(transactionBodyWithContractId).Build();
            var systemDeleteTransactionWithContractId = new SystemDeleteTransaction(txWithContractId);
            AssertNotNull(systemDeleteTransactionWithFileId.GetFileId());
            Assert.Equal(systemDeleteTransactionWithFileId.GetFileId(), testFileId);
            Assert.Null(systemDeleteTransactionWithFileId.GetContractId());
            Assert.Equal(systemDeleteTransactionWithFileId.GetExpirationTime().GetEpochSecond(), validStart.GetEpochSecond());
            Assert.Null(systemDeleteTransactionWithContractId.GetFileId());
            AssertNotNull(systemDeleteTransactionWithContractId.GetContractId());
            Assert.Equal(systemDeleteTransactionWithContractId.GetContractId(), testContractId);
            Assert.Equal(systemDeleteTransactionWithContractId.GetExpirationTime().GetEpochSecond(), validStart.GetEpochSecond());
        }

        virtual void GetSetFileId()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction().SetFileId(testFileId);
            AssertNotNull(systemDeleteTransaction.GetFileId());
            Assert.Equal(systemDeleteTransaction.GetFileId(), testFileId);
        }

        virtual void GetSetFileIdFrozen()
        {
            var tx = SpawnTestTransactionFile();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetFileId(testFileId));
        }

        virtual void GetSetContractId()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction().SetContractId(testContractId);
            AssertNotNull(systemDeleteTransaction.GetContractId());
            Assert.Equal(systemDeleteTransaction.GetContractId(), testContractId);
        }

        virtual void GetSetContractIdFrozen()
        {
            var tx = SpawnTestTransactionContract();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetContractId(testContractId));
        }

        virtual void GetSetExpirationTime()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction().SetExpirationTime(validStart);
            AssertNotNull(systemDeleteTransaction.GetExpirationTime());
            Assert.Equal(systemDeleteTransaction.GetExpirationTime().GetEpochSecond(), validStart.GetEpochSecond());
        }

        virtual void GetSetExpirationTimeFrozen()
        {
            var tx = SpawnTestTransactionFile();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetExpirationTime(validStart));
        }

        virtual void ResetFileId()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction();
            systemDeleteTransaction.SetFileId(testFileId);
            systemDeleteTransaction.SetContractId(testContractId);
            Assert.Null(systemDeleteTransaction.GetFileId());
            AssertNotNull(systemDeleteTransaction.GetContractId());
        }

        virtual void ResetContractId()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction();
            systemDeleteTransaction.SetContractId(testContractId);
            systemDeleteTransaction.SetFileId(testFileId);
            Assert.Null(systemDeleteTransaction.GetContractId());
            AssertNotNull(systemDeleteTransaction.GetFileId());
        }
    }
}