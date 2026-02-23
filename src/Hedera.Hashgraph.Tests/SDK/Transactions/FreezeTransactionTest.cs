// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK;

using System;
using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class FreezeTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly FileId testFileId = FileId.FromString("4.5.6");
        private static readonly byte[] testFileHash = Hex.Decode("1723904587120938954702349857");
        private static readonly FreezeType testFreezeType = FreezeType.TelemetryUpgrade;
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
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

        private FreezeTransaction SpawnTestTransaction()
        {
            return new FreezeTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
                FileId = testFileId,
                FileHash = testFileHash,
                StartTime = Timestamp.FromDateTimeOffset(validStart),
                FreezeType = testFreezeType,
                MaxTransactionFee = Hbar.FromTinybars(100000),
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<FreezeTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new FreezeTransaction();
            var tx2 = Transaction.FromBytes<FreezeTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                Freeze = new Proto.FreezeTransactionBody()
            };
            var tx = Transaction.FromScheduledTransaction<FreezeTransaction>(transactionBody);

            Assert.IsType<FreezeTransaction>(tx);
        }

        public virtual void ConstructFreezeTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.FreezeTransactionBody
			{
				UpdateFile = testFileId.ToProtobuf(),
                FreezeType = (Proto.FreezeType)testFreezeType,
				FileHash = ByteString.CopyFrom(testFileHash),
                StartTime = new Proto.Timestamp 
                {
                    Seconds = validStart.ToUnixTimeSeconds() 
                }
			};
            var tx = new Proto.TransactionBody
            {
				Freeze = transactionBody
			};
            var freezeTransaction = new FreezeTransaction(tx);

            Assert.NotNull(freezeTransaction.FileId);
            Assert.Equal(freezeTransaction.FileId, testFileId);
            Assert.Equal(freezeTransaction.FileHash, testFileHash);
            Assert.NotNull(freezeTransaction.StartTime);
            Assert.Equal(freezeTransaction.StartTime.ToDateTimeOffset().ToUnixTimeSeconds(), validStart.ToUnixTimeSeconds());
            Assert.Equal(freezeTransaction.FreezeType, testFreezeType);
        }

        public virtual void GetSetFileId()
        {
            var freezeTransaction = new FreezeTransaction
            {
				FileId = testFileId
			};
            Assert.NotNull(freezeTransaction.FileId);
            Assert.Equal(freezeTransaction.FileId, testFileId);
        }

        public virtual void GetSetFileIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.FileId = testFileId);
        }

        public virtual void GetSetFileHash()
        {
            var freezeTransaction = new FreezeTransaction
            {
				FileHash = testFileHash
			};
            Assert.NotNull(freezeTransaction.FileHash);
            Assert.Equal(freezeTransaction.FileHash, testFileHash);
        }

        public virtual void GetSetFileHashFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.FileHash = testFileHash);
        }

        public virtual void GetSetStartTime()
        {
            var freezeTransaction = new FreezeTransaction
            {
				StartTime = validStart.ToTimestamp()
			};
            Assert.NotNull(freezeTransaction.StartTime);
            Assert.Equal(freezeTransaction.StartTime.ToDateTimeOffset().ToUnixTimeSeconds(), validStart.ToUnixTimeSeconds());
        }

        public virtual void GetSetStartTimeFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.StartTime = validStart.ToTimestamp());
        }

        public virtual void GetSetFreezeType()
        {
            var freezeTransaction = new FreezeTransaction
            {
				FreezeType = testFreezeType
			};
            Assert.Equal(freezeTransaction.FreezeType, testFreezeType);
        }

        public virtual void GetSetFreezeTypeFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.FreezeType = testFreezeType);
        }
    }
}