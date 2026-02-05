// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
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
    public class FileUpdateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
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

        private FileUpdateTransaction SpawnTestTransaction()
        {
            return new FileUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetFileId(FileId.FromString("0.0.6006")).SetExpirationTime(Instant.OfEpochSecond(1554158728)).SetContents(new byte[] { 1, 2, 3, 4, 5 }).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetKeys(unusedPrivateKey).SetFileMemo("Hello memo").Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = FileUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new FileUpdateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetFileUpdate(FileUpdateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<FileUpdateTransaction>(tx);
        }
    }
}