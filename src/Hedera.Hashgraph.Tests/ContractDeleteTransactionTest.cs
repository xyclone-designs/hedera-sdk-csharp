// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ContractDeleteTransactionTest
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

        private ContractDeleteTransaction SpawnTestTransaction()
        {
            return new ContractDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetContractId(ContractId.FromString("0.0.5007")).SetTransferAccountId(new AccountId(0, 0, 9)).SetTransferContractId(ContractId.FromString("0.0.5008")).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = ContractDeleteTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new ContractDeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetContractDeleteInstance(ContractDeleteTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<ContractDeleteTransaction>(tx);
        }

        virtual void SetsPermanentRemovalInProtobufBody()
        {
            var tx = new ContractDeleteTransaction().SetContractId(ContractId.FromString("0.0.5007")).SetPermanentRemoval(true);
            var proto = tx.Build();
            AssertThat(proto.GetPermanentRemoval()).IsTrue();
        }

        virtual void ShouldSupportPermanentRemovalBytesRoundTrip()
        {
            var tx = new ContractDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetContractId(ContractId.FromString("0.0.5007")).SetTransferAccountId(new AccountId(0, 0, 9)).SetPermanentRemoval(true).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze();
            AssertThat(tx.GetPermanentRemoval()).IsTrue();
            Assert.Equal(tx.GetContractId(), ContractId.FromString("0.0.5007"));
            Assert.Equal(tx.GetTransferAccountId(), new AccountId(0, 0, 9));
            AssertThat(tx.GetTransferContractId()).IsNull();
            Assert.Equal(tx.GetNodeAccountIds(), Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")));
            Assert.Equal(tx.GetMaxTransactionFee(), Hbar.FromTinybars(100000));
            var tx2 = (ContractDeleteTransaction)Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
            AssertThat(tx2.GetPermanentRemoval()).IsTrue();
            Assert.Equal(tx2.GetContractId(), tx.GetContractId());
            Assert.Equal(tx2.GetTransferAccountId(), tx.GetTransferAccountId());
            AssertThat(tx2.GetTransferContractId()).IsNull();
            Assert.Equal(tx2.GetNodeAccountIds(), tx.GetNodeAccountIds());
            Assert.Equal(tx2.GetMaxTransactionFee(), tx.GetMaxTransactionFee());
        }
    }
}