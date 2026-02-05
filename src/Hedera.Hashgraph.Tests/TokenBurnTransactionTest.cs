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
    public class TokenBurnTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly long testAmount = 69;
        private static readonly IList<long> testSerials = Collections.SingletonList(420);
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void ShouldSerializeFungible()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        private TokenBurnTransaction SpawnTestTransaction()
        {
            return new TokenBurnTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetTokenId(testTokenId).SetAmount(testAmount).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenBurnTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldSerializeNft()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionNft().ToString()).ToMatchSnapshot();
        }

        private TokenBurnTransaction SpawnTestTransactionNft()
        {
            return new TokenBurnTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetTokenId(testTokenId).SetSerials(testSerials).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytesFungible()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenBurnTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransactionNft();
            var tx2 = TokenBurnTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenBurn(TokenBurnTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenBurnTransaction>(tx);
        }

        virtual void ConstructTokenBurnTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenBurnTransactionBody.NewBuilder().SetToken(testTokenId.ToProtobuf()).SetAmount(testAmount).AddAllSerialNumbers(testSerials).Build();
            var tx = TransactionBody.NewBuilder().SetTokenBurn(transactionBody).Build();
            var tokenBurnTransaction = new TokenBurnTransaction(tx);
            Assert.Equal(tokenBurnTransaction.GetTokenId(), testTokenId);
            Assert.Equal(tokenBurnTransaction.GetAmount(), testAmount);
            Assert.Equal(tokenBurnTransaction.GetSerials(), testSerials);
        }

        virtual void GetSetTokenId()
        {
            var tokenBurnTransaction = new TokenBurnTransaction().SetTokenId(testTokenId);
            Assert.Equal(tokenBurnTransaction.GetTokenId(), testTokenId);
        }

        virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenId(testTokenId));
        }

        virtual void GetSetAmount()
        {
            var tokenBurnTransaction = new TokenBurnTransaction().SetAmount(testAmount);
            Assert.Equal(tokenBurnTransaction.GetAmount(), testAmount);
        }

        virtual void GetSetAmountFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAmount(testAmount));
        }

        virtual void GetSetSerials()
        {
            var tokenBurnTransaction = new TokenBurnTransaction().SetSerials(testSerials);
            Assert.Equal(tokenBurnTransaction.GetSerials(), testSerials);
        }

        virtual void GetSetSerialsFrozen()
        {
            var tx = SpawnTestTransactionNft();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetSerials(testSerials));
        }
    }
}