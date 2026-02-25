// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;

using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenRejectTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId TEST_OWNER_ID = AccountId.FromString("0.6.9");
        private static readonly List<TokenId> TEST_TOKEN_IDS = [ TokenId.FromString("1.2.3"), TokenId.FromString("4.5.6"), TokenId.FromString("7.8.9") ];
        private static readonly List<NftId> TEST_NFT_IDS = [ new NftId(TokenId.FromString("4.5.6"), 2), new NftId(TokenId.FromString("7.8.9"), 3) ];
        readonly DateTimeOffset TEST_VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
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

        private TokenRejectTransaction SpawnTestTransaction()
        {
            return new TokenRejectTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START.ToTimestamp()),
				OwnerId = TEST_OWNER_ID,
				TokenIds = TEST_TOKEN_IDS,
				NftIds = TEST_NFT_IDS,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(TEST_PRIVATE_KEY);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenRejectTransaction();
            var tx2 = Transaction.FromBytes<TokenRejectTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenUpdateNftsTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenReject = new Proto.TokenRejectTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenRejectTransaction>(transactionBody);

            Assert.IsType<TokenRejectTransaction>(tx);
        }

        public virtual void ConstructTokenRejectTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyBuilder = new Proto.TokenRejectTransactionBody
            {
				Owner = TEST_OWNER_ID.ToProtobuf()
			};
            
            foreach (TokenId tokenId in TEST_TOKEN_IDS)
				transactionBodyBuilder.Rejections.Add(new Proto.TokenReference
				{
					FungibleToken = tokenId.ToProtobuf()
				});

			foreach (NftId nftId in TEST_NFT_IDS)
				transactionBodyBuilder.Rejections.Add(new Proto.TokenReference
				{
					Nft = nftId.ToProtobuf()
				});

            var tx = new Proto.TransactionBody
            {
				TokenReject = transactionBodyBuilder
			};

            var tokenRejectTransaction = new TokenRejectTransaction(tx);

            Assert.Equal(tokenRejectTransaction.OwnerId, TEST_OWNER_ID);
            Assert.Equal(tokenRejectTransaction.TokenIds_Read.Count, TEST_TOKEN_IDS.Count);
            Assert.Equal(tokenRejectTransaction.NftIds_Read.Count, TEST_NFT_IDS.Count);
        }

        public virtual void GetSetOwnerId()
        {
            var transaction = new TokenRejectTransaction { OwnerId = TEST_OWNER_ID };
            Assert.Equal(transaction.OwnerId, TEST_OWNER_ID);
        }

        public virtual void GetSetOwnerIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => transaction.OwnerId = TEST_OWNER_ID);
        }

        public virtual void GetSetTokenIds()
        {
            var transaction = new TokenRejectTransaction { TokenIds = TEST_TOKEN_IDS };
            Assert.Equal(transaction.TokenIds_Read, TEST_TOKEN_IDS);
        }
        public virtual void GetSetNftIds()
        {
            var transaction = new TokenRejectTransaction { NftIds = TEST_NFT_IDS };
            Assert.Equal(transaction.NftIds_Read, TEST_NFT_IDS);
        }
    }
}