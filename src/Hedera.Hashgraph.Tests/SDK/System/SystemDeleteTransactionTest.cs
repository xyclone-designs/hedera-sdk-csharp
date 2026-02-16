// SPDX-License-Identifier: Apache-2.0
using System;

using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.System;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.Tests.SDK.System
{
    public class SystemDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly FileId testFileId = FileId.FromString("4.2.0");
        private static readonly ContractId testContractId = ContractId.FromString("0.6.9");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerializeFile()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionFile().ToString()).ToMatchSnapshot();
        }

        private SystemDeleteTransaction SpawnTestTransactionFile()
        {
            return new SystemDeleteTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				ContractId = ContractId.FromString("0.0.444"),
				ExpirationTime = Timestamp.FromDateTimeOffset(validStart),
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerializeContract()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionContract().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new SystemDeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private SystemDeleteTransaction SpawnTestTransactionContract()
        {
            return new SystemDeleteTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				ContractId = ContractId.FromString("0.0.444"),
				ExpirationTime = Timestamp.FromDateTimeOffset(validStart),
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesContract()
        {
            var tx = SpawnTestTransactionContract();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesFile()
        {
            var tx = SpawnTestTransactionFile();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				SystemDelete = new Proto.SystemDeleteTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<SystemDeleteTransaction>(tx);
        }

        public virtual void ConstructSystemDeleteTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyWithFileId = new Proto.SystemDeleteTransactionBody
            {
                FileID = testFileId.ToProtobuf(),
                ExpirationTime = new Proto.TimestampSeconds { Seconds = validStart.ToUnixTimeSeconds() }
            };
            var transactionBodyWithContractId = new Proto.SystemDeleteTransactionBody
            {
                ContractID = testContractId.ToProtobuf(),
                ExpirationTime = new Proto.TimestampSeconds { Seconds = validStart.ToUnixTimeSeconds() }
            };
            var txWithFileId = new Proto.TransactionBody
            {
                SystemDelete = transactionBodyWithFileId
            };
            var systemDeleteTransactionWithFileId = new SystemDeleteTransaction(txWithFileId);
            var txWithContractId = new Proto.TransactionBody
            {
                SystemDelete = transactionBodyWithContractId
            };
            var systemDeleteTransactionWithContractId = new SystemDeleteTransaction(txWithContractId);

            Assert.NotNull(systemDeleteTransactionWithFileId.FileId);
            Assert.Equal(systemDeleteTransactionWithFileId.FileId, testFileId);
            Assert.Null(systemDeleteTransactionWithFileId.ContractId);
            Assert.Equal(systemDeleteTransactionWithFileId.ExpirationTime.ToDateTimeOffset().ToUnixTimeSeconds(), validStart.ToUnixTimeSeconds());
            Assert.Null(systemDeleteTransactionWithContractId.FileId);
            Assert.NotNull(systemDeleteTransactionWithContractId.ContractId);
            Assert.Equal(systemDeleteTransactionWithContractId.ContractId, testContractId);
            Assert.Equal(systemDeleteTransactionWithContractId.ExpirationTime.ToDateTimeOffset().ToUnixTimeSeconds(), validStart.ToUnixTimeSeconds());
        }

        public virtual void GetSetFileId()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction
            {
				FileId = testFileId
			};
            Assert.NotNull(systemDeleteTransaction.FileId);
            Assert.Equal(systemDeleteTransaction.FileId, testFileId);
        }

        public virtual void GetSetFileIdFrozen()
        {
            var tx = SpawnTestTransactionFile();
            Assert.Throws<InvalidOperationException>(() => tx.FileId = testFileId);
        }

        public virtual void GetSetContractId()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction
            {
				ContractId = testContractId
			};
            Assert.NotNull(systemDeleteTransaction.ContractId);
            Assert.Equal(systemDeleteTransaction.ContractId, testContractId);
        }

        public virtual void GetSetContractIdFrozen()
        {
            var tx = SpawnTestTransactionContract();
            Assert.Throws<InvalidOperationException>(() => tx.ContractId = testContractId);
        }

        public virtual void GetSetExpirationTime()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction
            {
				ExpirationTime = Timestamp.FromDateTimeOffset(validStart)
			};
            Assert.NotNull(systemDeleteTransaction.ExpirationTime);
            Assert.Equal(systemDeleteTransaction.ExpirationTime.ToDateTimeOffset().ToUnixTimeSeconds(), validStart.ToUnixTimeSeconds());
        }

        public virtual void GetSetExpirationTimeFrozen()
        {
            var tx = SpawnTestTransactionFile();
            Assert.Throws<InvalidOperationException>(() => tx.ExpirationTime = Timestamp.FromDateTimeOffset(validStart));
        }

        public virtual void ResetFileId()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction();
            systemDeleteTransaction.FileId = testFileId;
            systemDeleteTransaction.ContractId = testContractId;
            Assert.Null(systemDeleteTransaction.FileId);
            Assert.NotNull(systemDeleteTransaction.ContractId);
        }

        public virtual void ResetContractId()
        {
            var systemDeleteTransaction = new SystemDeleteTransaction();
            systemDeleteTransaction.ContractId = testContractId;
            systemDeleteTransaction.FileId = testFileId;
            Assert.Null(systemDeleteTransaction.ContractId);
            Assert.NotNull(systemDeleteTransaction.FileId);
        }
    }
}