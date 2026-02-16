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
    public class TokenGrantKycTransactionTest
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

        private TokenGrantKycTransaction SpawnTestTransaction()
        {
            return new TokenGrantKycTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetAccountId(testAccountId).SetTokenId(testTokenId).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenGrantKycTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenGrantKycTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenGrantKyc(TokenGrantKycTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenGrantKycTransaction>(tx);
        }

        public virtual void ConstructTokenGrantKycTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenGrantKycTransactionBody.NewBuilder().SetAccount(testAccountId.ToProtobuf()).SetToken(testTokenId.ToProtobuf()).Build();
            var tx = TransactionBody.NewBuilder().SetTokenGrantKyc(transactionBody).Build();
            var tokenGrantKycTransaction = new TokenGrantKycTransaction(tx);
            Assert.Equal(tokenGrantKycTransaction.GetTokenId(), testTokenId);
        }

        public virtual void GetSetAccountId()
        {
            var tokenGrantKycTransaction = new TokenGrantKycTransaction().SetAccountId(testAccountId);
            Assert.Equal(tokenGrantKycTransaction.GetAccountId(), testAccountId);
        }

        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAccountId(testAccountId));
        }

        public virtual void GetSetTokenId()
        {
            var tokenGrantKycTransaction = new TokenGrantKycTransaction().SetTokenId(testTokenId);
            Assert.Equal(tokenGrantKycTransaction.GetTokenId(), testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenId(testTokenId));
        }
    }
}