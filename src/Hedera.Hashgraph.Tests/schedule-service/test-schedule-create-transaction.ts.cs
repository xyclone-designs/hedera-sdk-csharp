// SPDX-License-Identifier: Apache-2.0
using System;

using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using VerifyXunit;

namespace Hedera.Hashgraph.TCK.ScheduleService
{
    public class ScheduleCreateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private ScheduleCreateTransaction SpawnTestTransaction()
        {
            var transferTransaction = new TransferTransaction()
                .AddHbarTransfer(AccountId.FromString("0.0.555"), new Hbar(-10))
                .AddHbarTransfer(AccountId.FromString("0.0.333"), new Hbar(10));
            
            return transferTransaction.Schedule(_ =>
            {
                _.NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")];
				_.TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart);
				_.AdminKey = unusedPrivateKey;
				_.PayerAccountId = AccountId.FromString("0.0.222");
				_.ScheduleMemo = "hi";
				_.MaxTransactionFee = new Hbar(1);
				_.ExpirationTime = validStart;     
            
            }).Freeze().Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<ScheduleCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new ScheduleCreateTransaction();
            var tx2 = Transaction.FromBytes<ScheduleCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldSupportExpirationTimeDurationBytesRoundTrip()
        {
            var tx = new TransferTransaction()
                .AddHbarTransfer(AccountId.FromString("0.0.555"), new Hbar(-10))
                .AddHbarTransfer(AccountId.FromString("0.0.333"), new Hbar(10))
                .Schedule(_ =>
                {
                    _.NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")];
                    _.TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart);
                    _.AdminKey = unusedPrivateKey;
                    _.PayerAccountId = AccountId.FromString("0.0.222");
                    _.ScheduleMemo = "with-duration";
                    _.MaxTransactionFee = new Hbar(1);
                    _.ExpirationTime = DateTime.UtcNow.AddSeconds(1234);
                });
            // When expiration is set via Duration, DateTimeOffset getter should be null

            Assert.Null(tx.ExpirationTime);
            
            var tx2 = Transaction.FromBytes<ScheduleCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
            Assert.Equal(tx2.ExpirationTime, DateTimeOffset.FromUnixTimeMilliseconds(1234));
        }
        [Fact]
        public virtual void SetExpirationTimeDurationOnFrozenTransactionShouldThrow()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.ExpirationTime = DateTimeOffset.FromUnixTimeSeconds(1));
        }
        [Fact]
        public virtual void GetSetExpirationTimeDateTime()
        {
            var instant = DateTimeOffset.FromUnixTimeMilliseconds(1234567).ToTimestamp();
            var tx = new ScheduleCreateTransaction
            {
				ExpirationTime = instant.ToDateTimeOffset()
			};

            Assert.Equal(tx.ExpirationTime?.ToUnixTimeSeconds(), instant.Seconds);
            Assert.Equal(tx.ExpirationTime?.Nanosecond, instant.Nanos);
        }
    }
}