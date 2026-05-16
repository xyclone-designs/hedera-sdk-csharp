// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK;

using System;

using Org.BouncyCastle.Utilities.Encoders;

using VerifyXunit;
using Hedera.Hashgraph.SDK.Core;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest"]' />
    public class FreezeTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly FileId testFileId = FileId.FromString("4.5.6");
        private static readonly byte[] testFileHash = Hex.Decode("1723904587120938954702349857");
        private static readonly FreezeType testFreezeType = FreezeType.TelemetryUpgrade;
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private FreezeTransaction SpawnTestTransaction()
        {
            return new FreezeTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                FileId = testFileId,
                FileHash = testFileHash,
                StartTime = validStart,
                FreezeType = testFreezeType,
                MaxTransactionFee = Hbar.FromTinybars(100000),
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.ShouldBytes"]' />
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<FreezeTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.ShouldBytesNoSetters"]' />
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new FreezeTransaction();
            var tx2 = Transaction.FromBytes<FreezeTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.FromScheduledTransaction"]' />
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
                Freeze = new Proto.Services.FreezeTransactionBody()
            };
            var tx = Transaction.FromScheduledTransaction<FreezeTransaction>(transactionBody);

            Assert.IsType<FreezeTransaction>(tx);
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.ConstructFreezeTransactionFromTransactionBodyProtobuf"]' />
        public virtual void ConstructFreezeTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.Services.FreezeTransactionBody
			{
				UpdateFile = testFileId.ToProtobuf(),
                FreezeType = (Proto.Services.FreezeType)testFreezeType,
				FileHash = ByteString.CopyFrom(testFileHash),
                StartTime = new Proto.Services.Timestamp 
                {
                    Seconds = validStart.ToUnixTimeSeconds() 
                }
			};
            var tx = new Proto.Services.TransactionBody
            {
				Freeze = transactionBody
			};
            var freezeTransaction = new FreezeTransaction(tx);

            Assert.NotNull(freezeTransaction.FileId);
            Assert.Equal(freezeTransaction.FileId, testFileId);
            Assert.Equal(freezeTransaction.FileHash, testFileHash);
            Assert.NotNull(freezeTransaction.StartTime);
            Assert.Equal(freezeTransaction.StartTime.ToUnixTimeSeconds(), validStart.ToUnixTimeSeconds());
            Assert.Equal(freezeTransaction.FreezeType, testFreezeType);
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.GetSetFileId"]' />
        public virtual void GetSetFileId()
        {
            var freezeTransaction = new FreezeTransaction
            {
				FileId = testFileId
			};
            Assert.NotNull(freezeTransaction.FileId);
            Assert.Equal(freezeTransaction.FileId, testFileId);
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.GetSetFileIdFrozen"]' />
        public virtual void GetSetFileIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.FileId = testFileId);
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.GetSetFileHash"]' />
        public virtual void GetSetFileHash()
        {
            var freezeTransaction = new FreezeTransaction
            {
				FileHash = testFileHash
			};
            Assert.NotNull(freezeTransaction.FileHash);
            Assert.Equal(freezeTransaction.FileHash, testFileHash);
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.GetSetFileHashFrozen"]' />
        public virtual void GetSetFileHashFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.FileHash = testFileHash);
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.GetSetStartTime"]' />
        public virtual void GetSetStartTime()
        {
            var freezeTransaction = new FreezeTransaction
            {
				StartTime = validStart
			};
            Assert.NotNull(freezeTransaction.StartTime);
            Assert.Equal(freezeTransaction.StartTime.ToUnixTimeSeconds(), validStart.ToUnixTimeSeconds());
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.GetSetStartTimeFrozen"]' />
        public virtual void GetSetStartTimeFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.StartTime = validStart);
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.GetSetFreezeType"]' />
        public virtual void GetSetFreezeType()
        {
            var freezeTransaction = new FreezeTransaction
            {
				FreezeType = testFreezeType
			};
            Assert.Equal(freezeTransaction.FreezeType, testFreezeType);
        }
        [Fact]
        /// <include file="test-transactions-freeze.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.FreezeTransactionTest.GetSetFreezeTypeFrozen"]' />
        public virtual void GetSetFreezeTypeFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.FreezeType = testFreezeType);
        }
    }
}
