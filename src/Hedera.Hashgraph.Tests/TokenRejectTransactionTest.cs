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
    public class TokenRejectTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId TEST_OWNER_ID = AccountId.FromString("0.6.9");
        private static readonly IList<TokenId> TEST_TOKEN_IDS = List.Of(TokenId.FromString("1.2.3"), TokenId.FromString("4.5.6"), TokenId.FromString("7.8.9"));
        private static readonly IList<NftId> TEST_NFT_IDS = List.Of(new NftId(TokenId.FromString("4.5.6"), 2), new NftId(TokenId.FromString("7.8.9"), 3));
        readonly Instant TEST_VALID_START = Instant.OfEpochSecond(1554158542);
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

        private TokenRejectTransaction SpawnTestTransaction()
        {
            return new TokenRejectTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START)).SetOwnerId(TEST_OWNER_ID).SetTokenIds(TEST_TOKEN_IDS).SetNftIds(TEST_NFT_IDS).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(TEST_PRIVATE_KEY);
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenRejectTransaction();
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
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenReject(TokenRejectTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenRejectTransaction>(tx);
        }

        virtual void ConstructTokenRejectTransactionFromTransactionBodyProtobuf()
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

        virtual void GetSetOwnerId()
        {
            var transaction = new TokenRejectTransaction().SetOwnerId(TEST_OWNER_ID);
            Assert.Equal(transaction.GetOwnerId(), TEST_OWNER_ID);
        }

        virtual void GetSetOwnerIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.SetOwnerId(TEST_OWNER_ID));
        }

        virtual void GetSetTokenIds()
        {
            var transaction = new TokenRejectTransaction().SetTokenIds(TEST_TOKEN_IDS);
            Assert.Equal(transaction.GetTokenIds(), TEST_TOKEN_IDS);
        }

        virtual void GetSetTokenIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.SetTokenIds(TEST_TOKEN_IDS));
        }

        virtual void GetSetNftIds()
        {
            var transaction = new TokenRejectTransaction().SetNftIds(TEST_NFT_IDS);
            Assert.Equal(transaction.GetNftIds(), TEST_NFT_IDS);
        }

        virtual void GetSetNftIdFrozen()
        {
            var transaction = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.SetNftIds(TEST_NFT_IDS));
        }
    }
}