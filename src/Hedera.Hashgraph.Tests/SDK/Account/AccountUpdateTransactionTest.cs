// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountUpdateTransactionTest
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

        public virtual AccountUpdateTransaction SpawnTestTransaction()
        {
            return new AccountUpdateTransaction().SetKey(unusedPrivateKey).SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetAccountId(AccountId.FromString("0.0.2002")).SetProxyAccountId(AccountId.FromString("0.0.1001")).SetAutoRenewPeriod(Duration.OfHours(10)).SetExpirationTime(DateTimeOffset.FromUnixTimeMilliseconds(1554158543)).SetReceiverSignatureRequired(false).SetMaxAutomaticTokenAssociations(100).SetAccountMemo("Some memo").SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetStakedAccountId(AccountId.FromString("0.0.3")).Freeze().Sign(unusedPrivateKey);
        }

        public virtual AccountUpdateTransaction SpawnTestTransaction2()
        {
            return new AccountUpdateTransaction().SetKey(unusedPrivateKey).SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetAccountId(AccountId.FromString("0.0.2002")).SetProxyAccountId(AccountId.FromString("0.0.1001")).SetAutoRenewPeriod(Duration.OfHours(10)).SetExpirationTime(DateTimeOffset.FromUnixTimeMilliseconds(1554158543)).SetReceiverSignatureRequired(false).SetMaxAutomaticTokenAssociations(100).SetAccountMemo("Some memo").SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetStakedNodeId(4).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = AccountUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new AccountUpdateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerialize2()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction2().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytes2()
        {
            var tx = SpawnTestTransaction2();
            var tx2 = AccountUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetCryptoUpdateAccount(CryptoUpdateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<AccountUpdateTransaction>(tx);
        }
    }
}