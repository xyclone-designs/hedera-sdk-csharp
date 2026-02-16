// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class CryptoTransferTransactionTest
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

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TransferTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TransferTransaction SpawnTestTransaction()
        {
            return new TransferTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).AddHbarTransfer(AccountId.FromString("0.0.5008"), Hbar.FromTinybars(400)).AddHbarTransfer(AccountId.FromString("0.0.5006"), Hbar.FromTinybars(800).Negated()).AddHbarTransfer(AccountId.FromString("0.0.5007"), Hbar.FromTinybars(400)).AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5008"), 400).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5006"), -800, 3).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5007"), 400, 3).AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5008"), 1).AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), -1).AddNftTransfer(TokenId.FromString("0.0.3").Nft(2), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(1), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(3), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5006")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006")).AddNftTransfer(TokenId.FromString("0.0.2").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006")).SetHbarTransferApproval(AccountId.FromString("0.0.5007"), true).SetTokenTransferApproval(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), true).SetNftTransferApproval(new NftId(TokenId.FromString("0.0.4"), 4), true).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(unusedPrivateKey);
        }

        private TransferTransaction SpawnModifiedTestTransaction()
        {
            return new TransferTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).AddHbarTransfer(AccountId.FromString("0.0.5008"), Hbar.FromTinybars(400)).AddHbarTransfer(AccountId.FromString("0.0.5006"), Hbar.FromTinybars(800).Negated()).AddHbarTransfer(AccountId.FromString("0.0.5007"), Hbar.FromTinybars(400)).AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5008"), 400).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5006"), -800, 3).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5007"), 400, 3).AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5008"), 1).AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), -1).AddNftTransfer(TokenId.FromString("0.0.3").Nft(2), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(1), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(3), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5006")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006")).AddNftTransfer(TokenId.FromString("0.0.2").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006")).SetHbarTransferApproval(AccountId.FromString("0.0.5007"), true).SetNftTransferApproval(new NftId(TokenId.FromString("0.0.4"), 4), true).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TransferTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void DecimalsMustBeConsistent()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new TransferTransaction().AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100, 2).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 3);
            });
        }

        public virtual void CanGetDecimals()
        {
            var tx = new TransferTransaction();
            AssertThat(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]).IsNull();
            tx.AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100);
            AssertThat(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]).IsNull();
            tx.AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 5);
            Assert.Equal(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")], 5);
        }

        public virtual void TransactionBodiesMustMatch()
        {
            Proto.Transaction tx1 = Proto.TransactionList.Parser.ParseFrom(SpawnTestTransaction().ToBytes()).GetTransactionList(0);
            Proto.Transaction tx2 = Proto.TransactionList.Parser.ParseFrom(SpawnModifiedTestTransaction().ToBytes()).GetTransactionList(1);
            var brokenTxList = TransactionList.NewBuilder().AddTransactionList(tx1).AddTransactionList(tx2);
            var brokenTxBytes = brokenTxList.Build().ToByteArray();
            Assert.Throws<ArgumentException>(() =>
            {
                Transaction.FromBytes(brokenTxBytes);
            });
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetCryptoTransfer(CryptoTransferTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TransferTransaction>(tx);
        }
    }
}