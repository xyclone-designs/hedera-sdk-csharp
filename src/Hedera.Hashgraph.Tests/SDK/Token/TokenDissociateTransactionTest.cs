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
    public class TokenDissociateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId testAccountId = AccountId.FromString("6.9.0");
        private static readonly List<TokenId> testTokenIds = Arrays.AsList(TokenId.FromString("4.2.0"), TokenId.FromString("4.2.1"), TokenId.FromString("4.2.2"));
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
            var tx = new TokenDissociateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenDissociateTransaction SpawnTestTransaction()
        {
            return new TokenDissociateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetAccountId(testAccountId).SetTokenIds(testTokenIds).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenDissociateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenDissociate(TokenDissociateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenDissociateTransaction>(tx);
        }

        public virtual void ConstructTokenDissociateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenDissociateTransactionBody.NewBuilder().SetAccount(testAccountId.ToProtobuf()).AddAllTokens(testTokenIds.Stream().Map(TokenId.ToProtobuf()).ToList()).Build();
            var tx = TransactionBody.NewBuilder().SetTokenDissociate(transactionBody).Build();
            var tokenDissociateTransaction = new TokenDissociateTransaction(tx);
            Assert.Equal(tokenDissociateTransaction.GetAccountId(), testAccountId);
            Assert.Equal(tokenDissociateTransaction.GetTokenIds().Count, testTokenIds.Count);
        }

        public virtual void GetSetAccountId()
        {
            var tokenDissociateTransaction = new TokenDissociateTransaction().SetAccountId(testAccountId);
            Assert.Equal(tokenDissociateTransaction.GetAccountId(), testAccountId);
        }

        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAccountId(testAccountId));
        }

        public virtual void GetSetTokenIds()
        {
            var tokenDissociateTransaction = new TokenDissociateTransaction().SetTokenIds(testTokenIds);
            Assert.Equal(tokenDissociateTransaction.GetTokenIds(), testTokenIds);
        }

        public virtual void GetSetTokenIdsFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenIds(testTokenIds));
        }
    }
}