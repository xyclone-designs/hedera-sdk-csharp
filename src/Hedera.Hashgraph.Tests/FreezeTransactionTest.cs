// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class FreezeTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly FileId testFileId = FileId.FromString("4.5.6");
        private static readonly byte[] testFileHash = Hex.Decode("1723904587120938954702349857");
        private static readonly FreezeType testFreezeType = FreezeType.TELEMETRY_UPGRADE;
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
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

        private FreezeTransaction SpawnTestTransaction()
        {
            return new FreezeTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetFileId(testFileId).SetFileHash(testFileHash).SetStartTime(validStart).SetFreezeType(testFreezeType).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = FreezeTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new FreezeTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetFreeze(FreezeTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<FreezeTransaction>(tx);
        }

        virtual void ConstructFreezeTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = FreezeTransactionBody.NewBuilder().SetUpdateFile(testFileId.ToProtobuf()).SetFileHash(ByteString.CopyFrom(testFileHash)).SetStartTime(Timestamp.NewBuilder().SetSeconds(validStart.GetEpochSecond())).SetFreezeType(testFreezeType.code);
            var tx = TransactionBody.NewBuilder().SetFreeze(transactionBody).Build();
            var freezeTransaction = new FreezeTransaction(tx);
            AssertNotNull(freezeTransaction.GetFileId());
            Assert.Equal(freezeTransaction.GetFileId(), testFileId);
            Assert.Equal(freezeTransaction.GetFileHash(), testFileHash);
            AssertNotNull(freezeTransaction.GetStartTime());
            Assert.Equal(freezeTransaction.GetStartTime().GetEpochSecond(), validStart.GetEpochSecond());
            Assert.Equal(freezeTransaction.GetFreezeType(), testFreezeType);
        }

        virtual void GetSetFileId()
        {
            var freezeTransaction = new FreezeTransaction().SetFileId(testFileId);
            AssertNotNull(freezeTransaction.GetFileId());
            Assert.Equal(freezeTransaction.GetFileId(), testFileId);
        }

        virtual void GetSetFileIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetFileId(testFileId));
        }

        virtual void GetSetFileHash()
        {
            var freezeTransaction = new FreezeTransaction().SetFileHash(testFileHash);
            AssertNotNull(freezeTransaction.GetFileHash());
            Assert.Equal(freezeTransaction.GetFileHash(), testFileHash);
        }

        virtual void GetSetFileHashFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetFileHash(testFileHash));
        }

        virtual void GetSetStartTime()
        {
            var freezeTransaction = new FreezeTransaction().SetStartTime(validStart);
            AssertNotNull(freezeTransaction.GetStartTime());
            Assert.Equal(freezeTransaction.GetStartTime().GetEpochSecond(), validStart.GetEpochSecond());
        }

        virtual void GetSetStartTimeFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetStartTime(validStart));
        }

        virtual void GetSetFreezeType()
        {
            var freezeTransaction = new FreezeTransaction().SetFreezeType(testFreezeType);
            Assert.Equal(freezeTransaction.GetFreezeType(), testFreezeType);
        }

        virtual void GetSetFreezeTypeFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetFreezeType(testFreezeType));
        }
    }
}