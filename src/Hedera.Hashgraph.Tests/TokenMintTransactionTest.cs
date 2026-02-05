// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Common.Collect;
using Com.Google.Protobuf;
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
    public class TokenMintTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly long testAmount = 10;
        private static readonly List<byte[]> testMetadataList = List.Of(new byte[] { 1, 2, 3, 4, 5 });
        private static readonly ByteString testMetadataByteString = ByteString.CopyFrom(new byte[] { 1, 2, 3, 4, 5 });
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

        virtual void ShouldSerializeMetadata()
        {
            SnapshotMatcher.Expect(SpawnMetadataTestTransaction().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenMintTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenMintTransaction SpawnTestTransaction()
        {
            return new TokenMintTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetTokenId(testTokenId).SetAmount(testAmount).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        private TokenMintTransaction SpawnMetadataTestTransaction()
        {
            return new TokenMintTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetTokenId(TokenId.FromString("1.2.3")).SetMetadata(testMetadataList).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytesMetadata()
        {
            var tx = SpawnMetadataTestTransaction();
            var tx2 = TokenUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenMint(TokenMintTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenMintTransaction>(tx);
        }

        virtual void ConstructTokenMintTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenMintTransactionBody.NewBuilder().SetToken(testTokenId.ToProtobuf()).SetAmount(testAmount).AddMetadata(testMetadataByteString).Build();
            var tx = TransactionBody.NewBuilder().SetTokenMint(transactionBody).Build();
            var tokenMintTransaction = new TokenMintTransaction(tx);
            Assert.Equal(tokenMintTransaction.GetTokenId(), testTokenId);
            Assert.Equal(tokenMintTransaction.GetAmount(), testAmount);
            Assert.Equal(Iterables.GetLast(tokenMintTransaction.GetMetadata()), testMetadataByteString.ToByteArray());
        }

        virtual void GetSetTokenId()
        {
            var tokenMintTransaction = new TokenMintTransaction().SetTokenId(testTokenId);
            Assert.Equal(tokenMintTransaction.GetTokenId(), testTokenId);
        }

        virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenId(testTokenId));
        }

        virtual void GetSetAmount()
        {
            var tokenMintTransaction = new TokenMintTransaction().SetAmount(testAmount);
            Assert.Equal(tokenMintTransaction.GetAmount(), testAmount);
        }

        virtual void GetSetAmountFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAmount(testAmount));
        }

        virtual void GetSetMetadata()
        {
            var tokenMintTransaction = new TokenMintTransaction().SetMetadata(testMetadataList);
            Assert.Equal(tokenMintTransaction.GetMetadata(), testMetadataList);
        }

        virtual void GetSetMetadataFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetMetadata(testMetadataList));
        }

        virtual void AddMetadata()
        {
            var tokenMintTransaction = new TokenMintTransaction().AddMetadata(Iterables.GetLast(testMetadataList));
            Assert.Equal(Iterables.GetLast(tokenMintTransaction.GetMetadata()), Iterables.GetLast(testMetadataList));
        }
    }
}