// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;

using VerifyXunit;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.TCK.CryptoService
{
    public class AccountCreateTransactionTest
    {
        private static readonly PrivateKey privateKeyED25519 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        PrivateKey privateKeyECDSA = PrivateKey.FromStringECDSA("7f109a9e3b0d8ecfba9cc23a3614433ce0fa7ddcc80f2a8f10b222179a5a80d6");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual AccountCreateTransaction SpawnTestTransaction()
        {
            return new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				Key = privateKeyED25519,
				InitialBalance = Hbar.FromTinybars(450),
				ProxyAccountId = AccountId.FromString("0.0.1001"),
				AccountMemo = "some dumb memo",
				ReceiverSigRequired = true,
				AutoRenewPeriod = TimeSpan.FromHours(10),
				StakedAccountId = AccountId.FromString("0.0.3"),
				Alias = EvmAddress.FromString("0x5c562e90feaf0eebd33ea75d21024f249d451417"),
				MaxAutomaticTokenAssociations = 100,
				MaxTransactionFee = Hbar.FromTinybars(100000),
            }
            .SetKeyWithAlias(privateKeyECDSA)
            .SetKeyWithAlias(privateKeyED25519, privateKeyECDSA)    
            .Freeze()
            .Sign(privateKeyED25519);
        }

        public virtual AccountCreateTransaction SpawnTestTransaction2()
        {
            return new AccountCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				Key = privateKeyED25519,
				InitialBalance = Hbar.FromTinybars(450),
				ProxyAccountId = AccountId.FromString("0.0.1001"),
				AccountMemo = "some dumb memo",
				ReceiverSigRequired = true,
				AutoRenewPeriod = TimeSpan.FromHours(10),
				StakedNodeId = 4,
				MaxAutomaticTokenAssociations = 100,
				MaxTransactionFee = Hbar.FromTinybars(100000),
			}
			.SetKeyWithAlias(privateKeyECDSA)
            .SetKeyWithAlias(privateKeyED25519, privateKeyECDSA)                
            .Freeze()
            .Sign(privateKeyED25519);
        }

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<AccountCreateTransaction>(tx.ToBytes());

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
            var tx2 = Transaction.FromBytes<AccountCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new AccountCreateTransaction();
            var tx2 = Transaction.FromBytes<AccountCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void PropertiesTest()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.Key, privateKeyED25519);
            Assert.Equal(tx.InitialBalance, Hbar.FromTinybars(450));
            Assert.True(tx.ReceiverSigRequired);
            Assert.Equal(tx.ProxyAccountId.ToString(), "0.0.1001");
            Assert.Equal(tx.AutoRenewPeriod.Hours, 10);
            Assert.Equal(tx.MaxAutomaticTokenAssociations, 100);
            Assert.Equal(tx.AccountMemo, "some dumb memo");
            Assert.Equal(tx.StakedAccountId.ToString(), "0.0.3");
            Assert.Null(tx.StakedNodeId);
            Assert.False(tx.DeclineStakingReward);
            Assert.Equal(tx.Alias, EvmAddress.FromString("0x5c562e90feaf0eebd33ea75d21024f249d451417"));
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
				CryptoCreateAccount = new Proto.Services.CryptoCreateTransactionBody()
			};
                
            var tx = Transaction.FromScheduledTransaction<AccountCreateTransaction>(transactionBody);

            Assert.IsType<AccountCreateTransaction>(tx);
        }
    }
}