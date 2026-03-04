// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Schedule;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;

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
            var transferTransaction = new TransferTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				AdminKey = unusedPrivateKey,
				PayerAccountId = AccountId.FromString("0.0.222"),
				ScheduleMemo = "hi",
				MaxTransactionFee = new Hbar(1),
				ExpirationTime = validStart,

			}.AddHbarTransfer(AccountId.FromString("0.0.555"), new Hbar(-10))
            .AddHbarTransfer(AccountId.FromString("0.0.333"), new Hbar(10));
            
            return transferTransaction.Schedule()
                
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<ScheduleCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new ScheduleCreateTransaction();
            var tx2 = Transaction.FromBytes<ScheduleCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSupportExpirationTimeDurationBytesRoundTrip()
        {
            var transferTransaction = new TransferTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				AdminKey = unusedPrivateKey,
				PayerAccountId = AccountId.FromString("0.0.222"),
				ScheduleMemo = "with-duration",
				MaxTransactionFee = new Hbar(1),
				ExpirationTime = TimeSpan.FromSeconds(1234),
			}
                .AddHbarTransfer(AccountId.FromString("0.0.555"), new Hbar(-10))
                .AddHbarTransfer(AccountId.FromString("0.0.333"), new Hbar(10));
            var tx = transferTransaction.Schedule();
            // When expiration is set via Duration, DateTimeOffset getter should be null
            Assert.Null(tx.ExpirationTime);
            var tx2 = Transaction.FromBytes<ScheduleCreateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
            Assert.Equal(tx2.ExpirationTime, DateTimeOffset.FromUnixTimeMilliseconds(1234).ToTimestamp());
        }

        public virtual void SetExpirationTimeDurationOnFrozenTransactionShouldThrow()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.ExpirationTime = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(1)));
        }

        public virtual void GetSetExpirationTimeDateTime()
        {
            var instant = DateTimeOffset.FromUnixTimeMilliseconds(1234567).ToTimestamp();
            var tx = new ScheduleCreateTransaction
            {
				ExpirationTime = instant
			};

            Assert.Equal(tx.ExpirationTime, instant);
        }
    }
}