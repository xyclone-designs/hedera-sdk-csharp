// SPDX-License-Identifier: Apache-2.0

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;

using VerifyXunit;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.TCK.CryptoService
{
    public class AccountUpdateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual AccountUpdateTransaction SpawnTestTransaction()
        {
            return new AccountUpdateTransaction
            {
				Key = unusedPrivateKey,
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				AccountId = AccountId.FromString("0.0.2002"),
				ProxyAccountId = AccountId.FromString("0.0.1001"),
				AutoRenewPeriod = TimeSpan.FromHours(10),
				ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1554158543),
				ReceiverSigRequired = false,
				MaxAutomaticTokenAssociations = 100,
				AccountMemo = "Some memo",
				MaxTransactionFee = Hbar.FromTinybars(100000),
				StakedAccountId = AccountId.FromString("0.0.3"),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual AccountUpdateTransaction SpawnTestTransaction2()
        {
            return new AccountUpdateTransaction
            {
				Key = unusedPrivateKey,
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				AccountId = AccountId.FromString("0.0.2002"),
				ProxyAccountId = AccountId.FromString("0.0.1001"),
				AutoRenewPeriod = TimeSpan.FromHours(10),
				ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1554158543),
				ReceiverSigRequired = false,
				MaxAutomaticTokenAssociations = 100,
				AccountMemo = "Some memo",
				MaxTransactionFee = Hbar.FromTinybars(100000),
				StakedNodeId = 4,
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<AccountUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new AccountUpdateTransaction();
            var tx2 = Transaction.FromBytes<AccountUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerialize2()
        {
            Verifier.Verify(SpawnTestTransaction2().ToString());
        }
        [Fact]
        public virtual void ShouldBytes2()
        {
            var tx = SpawnTestTransaction2();
            var tx2 = Transaction.FromBytes<AccountUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
				CryptoUpdateAccount = new Proto.Services.CryptoUpdateTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<AccountUpdateTransaction>(transactionBody);

            Assert.IsType<AccountUpdateTransaction>(tx);
        }
    }
}