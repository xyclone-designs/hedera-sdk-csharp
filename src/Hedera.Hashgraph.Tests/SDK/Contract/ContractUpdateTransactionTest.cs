// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractUpdateTransactionTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        public virtual void ShouldSerialize2()
        {
            Verifier.Verify(SpawnTestTransaction2().ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new ContractUpdateTransaction();
            var tx2 = ITransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private ContractUpdateTransaction SpawnTestTransaction()
        {
            return new ContractUpdateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				ContractId = ContractId.FromString("0.0.5007"),
				AdminKey = privateKey,
				MaxAutomaticTokenAssociations = 101,
				AutoRenewPeriod = TimeSpan.FromDays(1),
				ContractMemo = "3",
				StakedAccountId = AccountId.FromString("0.0.3"),
				ExpirationTime = DateTime.UnixEpoch.AddMilliseconds(4),
				ProxyAccountId = new AccountId(0, 0, 4),
				MaxTransactionFee = Hbar.FromTinybars(100000),
				AutoRenewAccountId = new AccountId(0, 0, 30),
			}
            .Freeze()
            .Sign(privateKey);
        }

        private ContractUpdateTransaction SpawnTestTransaction2()
        {
            return new ContractUpdateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				ContractId = ContractId.FromString("0.0.5007"),
				AdminKey = privateKey,
				MaxAutomaticTokenAssociations = 101,
				AutoRenewPeriod = TimeSpan.FromDays(1),
				ContractMemo = "3",
				StakedNodeId = 4,
				ExpirationTime = DateTime.UnixEpoch.AddMilliseconds(4),
				ProxyAccountId = new AccountId(0, 0, 4),
				MaxTransactionFee = Hbar.FromTinybars(100000),
				AutoRenewAccountId = new AccountId(0, 0, 30),
			}
            .Freeze()
            .Sign(privateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<ContractUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytes2()
        {
            var tx = SpawnTestTransaction2();
            var tx2 = Transaction.FromBytes<ContractUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldSupportExpirationTimeDurationBytesRoundTrip()
        {
            var tx = new ContractUpdateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				ContractId = ContractId.FromString("0.0.5007"),
				AdminKey = privateKey,
				MaxAutomaticTokenAssociations = 101,
				AutoRenewPeriod = TimeSpan.FromDays(1),
				ContractMemo = "with-duration",
				StakedAccountId = AccountId.FromString("0.0.3"),
				ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(1234),
				ProxyAccountId = new AccountId(0, 0, 4),
				MaxTransactionFee = Hbar.FromTinybars(100000),
				AutoRenewAccountId = new AccountId(0, 0, 30),
			};

            // When expiration is set via Duration, DateTimeOffset getter should be null
            Assert.Null(tx.ExpirationTime);
            var tx2 = Transaction.FromBytes<ContractUpdateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
            Assert.Equal(tx2.ExpirationTime, DateTimeOffset.FromUnixTimeMilliseconds(1234));
        }
        [Fact]
        public virtual void SetExpirationTimeDurationOnFrozenTransactionShouldThrow()
        {
            var tx = SpawnTestTransaction();

            Assert.Throws<InvalidOperationException>(() => tx.ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1));
        }
        [Fact]
        public virtual void GetSetExpirationTimeDateTime()
        {
            var instant = DateTimeOffset.FromUnixTimeMilliseconds(1234567);
            var tx = new ContractUpdateTransaction
            {
				ExpirationTime = instant
			};
            Assert.Equal(tx.ExpirationTime, instant);
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                ContractUpdateInstance = new Proto.ContractUpdateTransactionBody { }
            };
            var tx = Transaction.FromScheduledTransaction(transactionBody);

            Assert.IsType<ContractUpdateTransaction>(tx);
        }
    }
}