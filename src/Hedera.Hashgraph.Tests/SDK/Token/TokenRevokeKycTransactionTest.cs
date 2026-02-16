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

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenRevokeKycTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly AccountId testAccountId = AccountId.FromString("6.9.0");
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

        private TokenRevokeKycTransaction SpawnTestTransaction()
        {
            return new TokenRevokeKycTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetAccountId(testAccountId).SetTokenId(testTokenId).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenRevokeKycTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenRevokeKycTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenRevokeKyc(TokenRevokeKycTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenRevokeKycTransaction>(tx);
        }

        public virtual void ConstructTokenRevokeKycTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenRevokeKycTransactionBody.NewBuilder().SetAccount(testAccountId.ToProtobuf()).SetToken(testTokenId.ToProtobuf()).Build();
            var tx = TransactionBody.NewBuilder().SetTokenRevokeKyc(transactionBody).Build();
            var tokenRevokeKycTransaction = new TokenRevokeKycTransaction(tx);
            Assert.Equal(tokenRevokeKycTransaction.GetTokenId(), testTokenId);
        }

        public virtual void GetSetAccountId()
        {
            var tokenRevokeKycTransaction = new TokenRevokeKycTransaction().SetAccountId(testAccountId);
            Assert.Equal(tokenRevokeKycTransaction.GetAccountId(), testAccountId);
        }

        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAccountId(testAccountId));
        }

        public virtual void GetSetTokenId()
        {
            var tokenRevokeKycTransaction = new TokenRevokeKycTransaction().SetTokenId(testTokenId);
            Assert.Equal(tokenRevokeKycTransaction.GetTokenId(), testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenId(testTokenId));
        }
    }
}