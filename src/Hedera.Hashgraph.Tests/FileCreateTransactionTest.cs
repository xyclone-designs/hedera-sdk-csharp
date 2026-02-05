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
    public class FileCreateTransactionTest
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

        virtual void ShouldBytesNoSetters()
        {
            var tx = new FileCreateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private FileCreateTransaction SpawnTestTransaction()
        {
            return new FileCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetContents(new byte[] { 1, 2, 3, 4 }).SetExpirationTime(Instant.OfEpochSecond(1554158728)).SetKeys(unusedPrivateKey).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetFileMemo("Hello memo").Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = FileCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetFileCreate(FileCreateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<FileCreateTransaction>(tx);
        }
    }
}