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
    public class TokenWipeTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId testAccountId = AccountId.FromString("0.6.9");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly long testAmount = 4;
        private static readonly IList<long> testSerialNumbers = Arrays.AsList(8, 9, 10);
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerializeFungible()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenWipeTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenWipeTransaction SpawnTestTransaction()
        {
            return new TokenWipeTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetTokenId(TokenId.FromString("0.0.111")).SetAccountId(testAccountId).SetAmount(testAmount).SetSerials(testSerialNumbers).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerializeNft()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionNft().ToString()).ToMatchSnapshot();
        }

        private TokenWipeTransaction SpawnTestTransactionNft()
        {
            return new TokenWipeTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetTokenId(TokenId.FromString("0.0.111")).SetAccountId(testAccountId).SetSerials(Collections.SingletonList(444)).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesFungible()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenWipeTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransactionNft();
            var tx2 = TokenWipeTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenWipe(TokenWipeAccountTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenWipeTransaction>(tx);
        }

        public virtual void ConstructTokenWipeTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenWipeAccountTransactionBody.NewBuilder().SetToken(testTokenId.ToProtobuf()).SetAccount(testAccountId.ToProtobuf()).SetAmount(testAmount).AddAllSerialNumbers(testSerialNumbers).Build();
            var txBody = TransactionBody.NewBuilder().SetTokenWipe(transactionBody).Build();
            var tokenWipeTransaction = new TokenWipeTransaction(txBody);
            Assert.Equal(tokenWipeTransaction.GetTokenId(), testTokenId);
            Assert.Equal(tokenWipeTransaction.GetAccountId(), testAccountId);
            Assert.Equal(tokenWipeTransaction.GetAmount(), testAmount);
            Assert.Equal(tokenWipeTransaction.GetSerials(), testSerialNumbers);
        }

        public virtual void GetSetTokenId()
        {
            var tokenWipeTransaction = new TokenWipeTransaction().SetTokenId(testTokenId);
            Assert.Equal(tokenWipeTransaction.GetTokenId(), testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenId(testTokenId));
        }

        public virtual void GetSetAccountId()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.GetAccountId(), testAccountId);
        }

        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAccountId(testAccountId));
        }

        public virtual void GetSetAmount()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.GetAmount(), testAmount);
        }

        public virtual void GetSetAmountFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAmount(testAmount));
        }

        public virtual void GetSetSerialNumbers()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.GetSerials(), testSerialNumbers);
        }

        public virtual void GetSetSerialNumbersFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetSerials(testSerialNumbers));
        }
    }
}