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
    public class AccountAllowanceDeleteTransactionTest
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

        virtual AccountAllowanceDeleteTransaction SpawnTestTransaction()
        {
            var ownerId = AccountId.FromString("5.6.7");
            return new AccountAllowanceDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).DeleteAllHbarAllowances(ownerId).DeleteAllTokenAllowances(TokenId.FromString("2.2.2"), ownerId).DeleteAllTokenNftAllowances(TokenId.FromString("4.4.4").Nft(123), ownerId).DeleteAllTokenNftAllowances(TokenId.FromString("4.4.4").Nft(456), ownerId).DeleteAllTokenNftAllowances(TokenId.FromString("8.8.8").Nft(456), ownerId).DeleteAllTokenNftAllowances(TokenId.FromString("4.4.4").Nft(789), ownerId).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = AccountAllowanceDeleteTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new AccountAllowanceDeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetCryptoDeleteAllowance(CryptoDeleteAllowanceTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<AccountAllowanceDeleteTransaction>(tx);
        }
    }
}