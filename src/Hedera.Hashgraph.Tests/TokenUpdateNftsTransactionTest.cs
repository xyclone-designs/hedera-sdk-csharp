// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
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
    public class TokenUpdateNftsTransactionTest
    {
        private static readonly PrivateKey testMetadataKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly IList<long> testSerialNumbers = Arrays.AsList(8, 9, 10);
        private static readonly byte[] testMetadata = new byte[]
        {
            1,
            2,
            3,
            4,
            5
        };
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

        private TokenUpdateNftsTransaction SpawnTestTransaction()
        {
            return new TokenUpdateNftsTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetTokenId(testTokenId).SetMetadata(testMetadata).SetSerials(testSerialNumbers).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(testMetadataKey);
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenUpdateNftsTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenUpdateNftsTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenUpdateNfts(TokenUpdateNftsTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenUpdateNftsTransaction>(tx);
        }

        virtual void ConstructTokenUpdateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenUpdateNftsTransactionBody.NewBuilder().SetToken(testTokenId.ToProtobuf()).SetMetadata(BytesValue.Of(ByteString.CopyFrom(testMetadata))).AddAllSerialNumbers(testSerialNumbers).Build();
            var tx = TransactionBody.NewBuilder().SetTokenUpdateNfts(transactionBody).Build();
            var tokenUpdateNftsTransaction = new TokenUpdateNftsTransaction(tx);
            Assert.Equal(tokenUpdateNftsTransaction.GetTokenId(), testTokenId);
            Assert.Equal(tokenUpdateNftsTransaction.GetMetadata(), testMetadata);
            Assert.Equal(tokenUpdateNftsTransaction.GetSerials(), testSerialNumbers);
        }

        virtual void GetSetTokenId()
        {
            var tokenUpdateNftsTransaction = new TokenUpdateNftsTransaction().SetTokenId(testTokenId);
            Assert.Equal(tokenUpdateNftsTransaction.GetTokenId(), testTokenId);
        }

        virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenId(testTokenId));
        }

        virtual void GetSetMetadata()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.GetMetadata(), testMetadata);
        }

        virtual void GetSetMetadataFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetMetadata(testMetadata));
        }

        virtual void GetSetSerialNumbers()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.GetSerials(), testSerialNumbers);
        }

        virtual void GetSetSerialNumbersFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetSerials(testSerialNumbers));
        }
    }
}