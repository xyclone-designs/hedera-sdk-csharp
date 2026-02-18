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
    public class TokenRejectTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId TEST_OWNER_ID = AccountId.FromString("0.6.9");
        private static readonly List<TokenId> TEST_TOKEN_IDS = List.Of(TokenId.FromString("1.2.3"), TokenId.FromString("4.5.6"), TokenId.FromString("7.8.9"));
        private static readonly List<NftId> TEST_NFT_IDS = List.Of(new NftId(TokenId.FromString("4.5.6"), 2), new NftId(TokenId.FromString("7.8.9"), 3));
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
            return new TokenRejectTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START)).SetOwnerId(TEST_OWNER_ID).SetTokenIds(TEST_TOKEN_IDS).SetNftIds(TEST_NFT_IDS).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(TEST_PRIVATE_KEY);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenRejectTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenUpdateNftsTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenReject(TokenRejectTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenRejectTransaction>(tx);
        }

        public virtual void ConstructTokenRejectTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyBuilder = TokenRejectTransactionBody.NewBuilder();
            transactionBodyBuilder.SetOwner(TEST_OWNER_ID.ToProtobuf());
            foreach (TokenId tokenId in TEST_TOKEN_IDS)
            {
                transactionBodyBuilder.AddRejections(TokenReference.NewBuilder().SetFungibleToken(tokenId.ToProtobuf()).Build());
            }

            foreach (NftId nftId in TEST_NFT_IDS)
            {
                transactionBodyBuilder.AddRejections(TokenReference.NewBuilder().SetNft(nftId.ToProtobuf()).Build());
            }

            var tx = TransactionBody.NewBuilder().SetTokenReject(transactionBodyBuilder.Build()).Build();
            var tokenRejectTransaction = new TokenRejectTransaction(tx);
            Assert.Equal(tokenRejectTransaction.GetOwnerId(), TEST_OWNER_ID);
            AssertThat(tokenRejectTransaction.GetTokenIds()).HasSize(TEST_TOKEN_IDS.Count);
            AssertThat(tokenRejectTransaction.GetNftIds()).HasSize(TEST_NFT_IDS.Count);
        }

        public virtual void GetSetOwnerId()
        {
            var transaction = new TokenRejectTransaction().SetOwnerId(TEST_OWNER_ID);
            Assert.Equal(transaction.GetOwnerId(), TEST_OWNER_ID);
        }

        public virtual void GetSetOwnerIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => transaction.SetOwnerId(TEST_OWNER_ID));
        }

        public virtual void GetSetTokenIds()
        {
            var transaction = new TokenRejectTransaction().SetTokenIds(TEST_TOKEN_IDS);
            Assert.Equal(transaction.GetTokenIds(), TEST_TOKEN_IDS);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => transaction.SetTokenIds(TEST_TOKEN_IDS));
        }

        public virtual void GetSetNftIds()
        {
            var transaction = new TokenRejectTransaction().SetNftIds(TEST_NFT_IDS);
            Assert.Equal(transaction.GetNftIds(), TEST_NFT_IDS);
        }

        public virtual void GetSetNftIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => transaction.SetNftIds(TEST_NFT_IDS));
        }
    }
}