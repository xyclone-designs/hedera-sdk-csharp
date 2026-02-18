// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using Hedera.Hashgraph.SDK.HBar;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Keys;
using Google.Protobuf;

namespace Hedera.Hashgraph.Tests.SDK.Nfts
{
    public class TokenUpdateNftsTransactionTest
    {
        private static readonly PrivateKey testMetadataKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly List<long> testSerialNumbers = [8, 9, 10];
        private static readonly byte[] testMetadata = [1, 2, 3, 4, 5];
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

        private TokenUpdateNftsTransaction SpawnTestTransaction()
        {
            return new TokenUpdateNftsTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				TokenId = testTokenId,
				Metadata = testMetadata,
				Serials = testSerialNumbers,
				MaxTransactionFee = new Hbar(1),
            }
            .Freeze()
            .Sign(testMetadataKey);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenUpdateNftsTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenUpdateNfts = new Proto.TokenUpdateNftsTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenUpdateNftsTransaction>(transactionBody);
            Assert.IsType<TokenUpdateNftsTransaction>(tx);
        }

        public virtual void ConstructTokenUpdateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenUpdateNftsTransactionBody
            {
                Token = testTokenId.ToProtobuf(),
                Metadata = ByteString.CopyFrom(testMetadata),
            };

            transactionBody.SerialNumbers.AddRange(testSerialNumbers);
                
            var tx = new Proto.TransactionBody
            {
				TokenUpdateNfts = transactionBody
			};
            var tokenUpdateNftsTransaction = new TokenUpdateNftsTransaction(tx);
            Assert.Equal(tokenUpdateNftsTransaction.TokenId, testTokenId);
            Assert.Equal(tokenUpdateNftsTransaction.Metadata, testMetadata);
            Assert.Equal(tokenUpdateNftsTransaction.Serials, testSerialNumbers);
        }

        public virtual void GetSetTokenId()
        {
            var tokenUpdateNftsTransaction = new TokenUpdateNftsTransaction
            {
				TokenId = testTokenId
			};
            
            Assert.Equal(tokenUpdateNftsTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }

        public virtual void GetSetMetadata()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.Metadata, testMetadata);
        }

        public virtual void GetSetMetadataFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.Metadata = testMetadata);
        }

        public virtual void GetSetSerialNumbers()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.Serials, testSerialNumbers);
        }

        public virtual void GetSetSerialNumbersFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.Serials.AddRange(testSerialNumbers));
        }
    }
}