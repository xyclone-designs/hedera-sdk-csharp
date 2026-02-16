// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountCreateTransactionTest
    {
        private static readonly PrivateKey privateKeyED25519 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        PrivateKey privateKeyECDSA = PrivateKey.FromStringECDSA("7f109a9e3b0d8ecfba9cc23a3614433ce0fa7ddcc80f2a8f10b222179a5a80d6");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual AccountCreateTransaction SpawnTestTransaction()
        {
            return new AccountCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetKeyWithAlias(privateKeyECDSA).SetKeyWithAlias(privateKeyED25519, privateKeyECDSA).SetKeyWithoutAlias(privateKeyED25519).SetInitialBalance(Hbar.FromTinybars(450)).SetProxyAccountId(AccountId.FromString("0.0.1001")).SetAccountMemo("some dumb memo").SetReceiverSignatureRequired(true).SetAutoRenewPeriod(Duration.OfHours(10)).SetStakedAccountId(AccountId.FromString("0.0.3")).SetAlias("0x5c562e90feaf0eebd33ea75d21024f249d451417").SetMaxAutomaticTokenAssociations(100).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(privateKeyED25519);
        }

        public virtual AccountCreateTransaction SpawnTestTransaction2()
        {
            return new AccountCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetKeyWithAlias(privateKeyECDSA).SetKeyWithAlias(privateKeyED25519, privateKeyECDSA).SetKeyWithoutAlias(privateKeyED25519).SetInitialBalance(Hbar.FromTinybars(450)).SetProxyAccountId(AccountId.FromString("0.0.1001")).SetAccountMemo("some dumb memo").SetReceiverSignatureRequired(true).SetAutoRenewPeriod(Duration.OfHours(10)).SetStakedNodeId(4).SetMaxAutomaticTokenAssociations(100).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(privateKeyED25519);
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = AccountCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerialize2()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction2().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytes2()
        {
            var tx = SpawnTestTransaction2();
            var tx2 = AccountCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new AccountCreateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void PropertiesTest()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.GetKey(), privateKeyED25519);
            Assert.Equal(tx.GetInitialBalance(), Hbar.FromTinybars(450));
            AssertThat(tx.GetReceiverSignatureRequired()).IsTrue();
            AssertThat(tx.GetProxyAccountId()).HasToString("0.0.1001");
            Assert.Equal(tx.GetAutoRenewPeriod().ToHours(), 10);
            Assert.Equal(tx.GetMaxAutomaticTokenAssociations(), 100);
            Assert.Equal(tx.GetAccountMemo(), "some dumb memo");
            AssertThat(tx.GetStakedAccountId()).HasToString("0.0.3");
            AssertThat(tx.GetStakedNodeId()).IsNull();
            AssertThat(tx.GetDeclineStakingReward()).IsFalse();
            Assert.Equal(tx.GetAlias(), EvmAddress.FromString("0x5c562e90feaf0eebd33ea75d21024f249d451417"));
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetCryptoCreateAccount(CryptoCreateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<AccountCreateTransaction>(tx);
        }
    }
}