// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ContractCreateTransactionTest
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

        virtual void ShouldSerialize2()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction2().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new ContractCreateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private ContractCreateTransaction SpawnTestTransaction()
        {
            return new ContractCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetBytecodeFileId(FileId.FromString("0.0.3003")).SetAdminKey(unusedPrivateKey).SetGas(0).SetInitialBalance(Hbar.FromTinybars(1000)).SetStakedAccountId(AccountId.FromString("0.0.3")).SetMaxAutomaticTokenAssociations(101).SetAutoRenewPeriod(Duration.OfHours(10)).SetConstructorParameters(new byte[] { 10, 11, 12, 13, 25 }).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetAutoRenewAccountId(new AccountId(0, 0, 30)).Freeze().Sign(unusedPrivateKey);
        }

        private ContractCreateTransaction SpawnTestTransaction2()
        {
            return new ContractCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetBytecode(Hex.Decode("deadbeef")).SetAdminKey(unusedPrivateKey).SetGas(0).SetInitialBalance(Hbar.FromTinybars(1000)).SetStakedNodeId(4).SetMaxAutomaticTokenAssociations(101).SetAutoRenewPeriod(Duration.OfHours(10)).SetConstructorParameters(new byte[] { 10, 11, 12, 13, 25 }).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetAutoRenewAccountId(new AccountId(0, 0, 30)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = ContractCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytes2()
        {
            var tx = SpawnTestTransaction2();
            var tx2 = ContractCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx2.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetContractCreateInstance(ContractCreateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<ContractCreateTransaction>(tx);
        }

        virtual void SetGasShouldRejectNegativeValues()
        {
            var tx = new ContractCreateTransaction();
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => tx.SetGas(-1));
            Assert.Equal(ex.GetMessage(), "Gas must be non-negative");
        }

        virtual void SetGasShouldAcceptZeroAndPositiveValues()
        {
            var tx = new ContractCreateTransaction();
            tx.SetGas(0);
            Assert.Equal(tx.GetGas(), 0);
            tx.SetGas(123456);
            Assert.Equal(tx.GetGas(), 123456);
        }
    }
}