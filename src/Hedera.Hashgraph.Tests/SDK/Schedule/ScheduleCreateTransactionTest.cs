// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Schedule
{
    public class ScheduleCreateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
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

        private ScheduleCreateTransaction SpawnTestTransaction()
        {
            var transferTransaction = new TransferTransaction().AddHbarTransfer(AccountId.FromString("0.0.555"), new Hbar(-10)).AddHbarTransfer(AccountId.FromString("0.0.333"), new Hbar(10));
            return transferTransaction.Schedule().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetAdminKey(unusedPrivateKey).SetPayerAccountId(AccountId.FromString("0.0.222")).SetScheduleMemo("hi").SetMaxTransactionFee(new Hbar(1)).SetExpirationTime(validStart).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = ScheduleCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new ScheduleCreateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSupportExpirationTimeDurationBytesRoundTrip()
        {
            var transferTransaction = new TransferTransaction().AddHbarTransfer(AccountId.FromString("0.0.555"), new Hbar(-10)).AddHbarTransfer(AccountId.FromString("0.0.333"), new Hbar(10));
            var tx = transferTransaction.Schedule().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetAdminKey(unusedPrivateKey).SetPayerAccountId(AccountId.FromString("0.0.222")).SetScheduleMemo("with-duration").SetMaxTransactionFee(new Hbar(1)).SetExpirationTime(Duration.OfSeconds(1234));

            // When expiration is set via Duration, DateTimeOffset getter should be null
            AssertThat(tx.GetExpirationTime()).IsNull();
            var tx2 = (ScheduleCreateTransaction)Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
            Assert.Equal(tx2.GetExpirationTime(), DateTimeOffset.FromUnixTimeMilliseconds(1234));
        }

        public virtual void SetExpirationTimeDurationOnFrozenTransactionShouldThrow()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetExpirationTime(Duration.OfSeconds(1)));
        }

        public virtual void GetSetExpirationTimeInstant()
        {
            var instant = DateTimeOffset.FromUnixTimeMilliseconds(1234567);
            var tx = new ScheduleCreateTransaction().SetExpirationTime(instant);
            Assert.Equal(tx.GetExpirationTime(), instant);
        }
    }
}