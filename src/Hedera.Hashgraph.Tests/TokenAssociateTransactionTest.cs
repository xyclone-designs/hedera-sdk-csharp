// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
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
    public class TokenAssociateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId accountId = AccountId.FromString("1.2.3");
        private static readonly IList<TokenId> tokenIds = List.Of(TokenId.FromString("4.5.6"), TokenId.FromString("7.8.9"), TokenId.FromString("10.11.12"));
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

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenAssociateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenAssociateTransaction SpawnTestTransaction()
        {
            return new TokenAssociateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetAccountId(AccountId.FromString("0.0.222")).SetTokenIds(Collections.SingletonList(TokenId.FromString("0.0.666"))).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenAssociateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenAssociate(TokenAssociateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenAssociateTransaction>(tx);
        }

        virtual void ConstructTokenDeleteTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenAssociateTransactionBody.NewBuilder().AddAllTokens(tokenIds.Stream().Map(TokenId.ToProtobuf()).ToList()).SetAccount(accountId.ToProtobuf()).Build();
            var txBody = TransactionBody.NewBuilder().SetTokenAssociate(transactionBody).Build();
            var tokenAssociateTransaction = new TokenAssociateTransaction(txBody);
            Assert.Equal(tokenAssociateTransaction.GetAccountId(), accountId);
            AssertThat(tokenAssociateTransaction.GetTokenIds()).HasSize(tokenIds.Count);
        }

        virtual void GetSetAccountId()
        {
            var transaction = new TokenAssociateTransaction().SetAccountId(accountId);
            Assert.Equal(transaction.GetAccountId(), accountId);
        }

        virtual void GetSetAccountIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.SetAccountId(accountId));
        }

        virtual void GetSetTokenIds()
        {
            var transaction = new TokenAssociateTransaction().SetTokenIds(tokenIds);
            Assert.Equal(transaction.GetTokenIds(), tokenIds);
        }

        virtual void GetSetTokenIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.SetTokenIds(tokenIds));
        }
    }
}