// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractUpdateTransactionTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
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

        public virtual void ShouldSerialize2()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction2().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new ContractUpdateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private ContractUpdateTransaction SpawnTestTransaction()
        {
            return new ContractUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetContractId(ContractId.FromString("0.0.5007")).SetAdminKey(privateKey).SetMaxAutomaticTokenAssociations(101).SetAutoRenewPeriod(Duration.OfDays(1)).SetContractMemo("3").SetStakedAccountId(AccountId.FromString("0.0.3")).SetExpirationTime(Instant.OfEpochMilli(4)).SetProxyAccountId(new AccountId(0, 0, 4)).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetAutoRenewAccountId(new AccountId(0, 0, 30)).Freeze().Sign(privateKey);
        }

        private ContractUpdateTransaction SpawnTestTransaction2()
        {
            return new ContractUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetContractId(ContractId.FromString("0.0.5007")).SetAdminKey(privateKey).SetMaxAutomaticTokenAssociations(101).SetAutoRenewPeriod(Duration.OfDays(1)).SetContractMemo("3").SetStakedNodeId(4).SetExpirationTime(Instant.OfEpochMilli(4)).SetProxyAccountId(new AccountId(0, 0, 4)).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetAutoRenewAccountId(new AccountId(0, 0, 30)).Freeze().Sign(privateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = ContractUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytes2()
        {
            var tx = SpawnTestTransaction2();
            var tx2 = ContractUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSupportExpirationTimeDurationBytesRoundTrip()
        {
            var tx = new ContractUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetContractId(ContractId.FromString("0.0.5007")).SetAdminKey(privateKey).SetMaxAutomaticTokenAssociations(101).SetAutoRenewPeriod(Duration.OfDays(1)).SetContractMemo("with-duration").SetStakedAccountId(AccountId.FromString("0.0.3")).SetExpirationTime(Duration.OfSeconds(1234)).SetProxyAccountId(new AccountId(0, 0, 4)).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetAutoRenewAccountId(new AccountId(0, 0, 30));

            // When expiration is set via Duration, DateTimeOffset getter should be null
            AssertThat(tx.GetExpirationTime()).IsNull();
            var tx2 = (ContractUpdateTransaction)Transaction.FromBytes(tx.ToBytes());
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
            var tx = new ContractUpdateTransaction().SetExpirationTime(instant);
            Assert.Equal(tx.GetExpirationTime(), instant);
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetContractUpdateInstance(ContractUpdateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<ContractUpdateTransaction>(tx);
        }
    }
}