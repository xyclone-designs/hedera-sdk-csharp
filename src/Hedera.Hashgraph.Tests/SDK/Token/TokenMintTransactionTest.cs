// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenMintTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly long testAmount = 10;
        private static readonly List<byte[]> testMetadataList = [new byte[] { 1, 2, 3, 4, 5 }];
        private static readonly ByteString testMetadataByteString = ByteString.CopyFrom(new byte[] { 1, 2, 3, 4, 5 });
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

        public virtual void ShouldSerializeMetadata()
        {
            SnapshotMatcher.Expect(SpawnMetadataTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenMintTransaction();
            var tx2 = Transaction.FromBytes<TokenMintTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenMintTransaction SpawnTestTransaction()
        {
            return new TokenMintTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				TokenId = testTokenId,
				Amount = testAmount,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        private TokenMintTransaction SpawnMetadataTestTransaction()
        {
            return new TokenMintTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				TokenId = TokenId.FromString("1.2.3"),
				Metadata = testMetadataList,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenUpdateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesMetadata()
        {
            var tx = SpawnMetadataTestTransaction();
            var tx2 = Transaction.FromBytes<TokenUpdateTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenMint = new Proto.TokenMintTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenMintTransaction>(transactionBody);
            Assert.IsType<TokenMintTransaction>(tx);
        }

        public virtual void ConstructTokenMintTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenMintTransactionBody
            {
				Token = testTokenId.ToProtobuf(),
				Amount = testAmount
			
            }.AddMetadata(testMetadataByteString);
            var tx = new Proto.TransactionBody
            {
				TokenMint = transactionBody
			};
            var tokenMintTransaction = new TokenMintTransaction(tx);

            Assert.Equal(tokenMintTransaction.TokenId, testTokenId);
            Assert.Equal(tokenMintTransaction.Amount, testAmount);
            Assert.Equal(tokenMintTransaction.MetadataList.Last(), testMetadataByteString.ToByteArray());
        }

        public virtual void GetSetTokenId()
        {
            var tokenMintTransaction = new TokenMintTransaction
            {
				TokenId = testTokenId
			};
            Assert.Equal(tokenMintTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }

        public virtual void GetSetAmount()
        {
            var tokenMintTransaction = new TokenMintTransaction
            {
				Amount = testAmount
			};
            Assert.Equal(tokenMintTransaction.Amount, testAmount);
        }

        public virtual void GetSetAmountFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.Amount = testAmount);
        }

        public virtual void GetSetMetadata()
        {
            var tokenMintTransaction = new TokenMintTransaction().MetadataList = testMetadataList;
            Assert.Equal(tokenMintTransaction.MetadataList, testMetadataList);
        }

        public virtual void GetSetMetadataFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.MetadataList = testMetadataList);
        }

        public virtual void AddMetadata()
        {
            var tokenMintTransaction = new TokenMintTransaction().AddMetadata(testMetadataList.Last());
            
            Assert.Equal(tokenMintTransaction.MetadataList.Last(), testMetadataList.Last());
        }
    }
}