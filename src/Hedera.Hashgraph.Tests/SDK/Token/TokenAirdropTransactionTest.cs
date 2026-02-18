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
    class TokenAirdropTransactionTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private TokenAirdropTransaction transaction;
        public virtual void SetUp()
        {
            transaction = new TokenAirdropTransaction();
        }

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

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenAirdropTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenAirdropTransaction SpawnTestTransaction()
        {
            return new TokenAirdropTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5008"), 400).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5006"), -800, 3).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5007"), 400, 3).AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5008"), 1).AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), -1).AddNftTransfer(TokenId.FromString("0.0.3").Nft(2), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(1), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(3), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5006")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006")).AddNftTransfer(TokenId.FromString("0.0.2").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006")).AddApprovedTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), 123).AddApprovedNftTransfer(new NftId(TokenId.FromString("0.0.4"), 4), AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(privateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenAirdropTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void DecimalsMustBeConsistent()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new TokenAirdropTransaction().AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100, 2).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 3);
            });
        }

        public virtual void CanGetDecimals()
        {
            var tx = new TokenAirdropTransaction();
            Assert.Null(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]);
            tx.AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100);
            Assert.Null(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]);
            tx.AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 5);
            Assert.Equal(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")], 5);
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenAirdrop(TokenAirdropTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenAirdropTransaction>(tx);
        }

        public virtual void TestDefaultMaxTransactionFeeIsSet()
        {
            Assert.Equal(new Hbar(1), transaction.GetDefaultMaxTransactionFee(), "Default max transaction fee should be 1 Hbar");
        }

        public virtual void TestAddTokenTransfer()
        {
            TokenId tokenId = new TokenId(0, 0, 123);
            AccountId accountId = new AccountId(0, 0, 456);
            long value = 1000;
            transaction.AddTokenTransfer(tokenId, accountId, value);
            Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers = transaction.GetTokenTransfers();
            Assert.True(tokenTransfers.ContainsKey(tokenId));
            Assert.Equal(1, tokenTransfers[tokenId].Count);
            Assert.Equal(value, tokenTransfers[tokenId][accountId]);
        }

        public virtual void TestAddApprovedTokenTransfer()
        {
            TokenId tokenId = new TokenId(0, 0, 123);
            AccountId accountId = new AccountId(0, 0, 456);
            long value = 1000;
            transaction.AddApprovedTokenTransfer(tokenId, accountId, value);
            Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers = transaction.GetTokenTransfers();
            Assert.True(tokenTransfers.ContainsKey(tokenId));
            Assert.Equal(1, tokenTransfers[tokenId].Count);
            Assert.Equal(value, tokenTransfers[tokenId][accountId]);
        }

        public virtual void TestAddNftTransfer()
        {
            NftId nftId = new NftId(new TokenId(0, 0, 123), 1);
            AccountId sender = new AccountId(0, 0, 456);
            AccountId receiver = new AccountId(0, 0, 789);
            transaction.AddNftTransfer(nftId, sender, receiver);
            Dictionary<TokenId, IList<TokenNftTransfer>> nftTransfers = transaction.GetTokenNftTransfers();
            Assert.True(nftTransfers.ContainsKey(nftId.tokenId));
            Assert.Equal(1, nftTransfers[nftId.tokenId].Count);
            Assert.Equal(sender, nftTransfers[nftId.tokenId][0].sender);
            Assert.Equal(receiver, nftTransfers[nftId.tokenId][0].receiver);
        }

        public virtual void TestAddApprovedNftTransfer()
        {
            NftId nftId = new NftId(new TokenId(0, 0, 123), 1);
            AccountId sender = new AccountId(0, 0, 456);
            AccountId receiver = new AccountId(0, 0, 789);
            transaction.AddApprovedNftTransfer(nftId, sender, receiver);
            Dictionary<TokenId, IList<TokenNftTransfer>> nftTransfers = transaction.GetTokenNftTransfers();
            Assert.True(nftTransfers.ContainsKey(nftId.tokenId));
            Assert.Equal(1, nftTransfers[nftId.tokenId].Count);
            Assert.Equal(sender, nftTransfers[nftId.tokenId][0].sender);
            Assert.Equal(receiver, nftTransfers[nftId.tokenId][0].receiver);
        }

        public virtual void TestGetTokenIdDecimals()
        {
            TokenId tokenId = new TokenId(0, 0, 123);
            AccountId accountId = new AccountId(0, 0, 456);
            long value = 1000;
            int decimals = 8;
            transaction.AddTokenTransferWithDecimals(tokenId, accountId, value, decimals);
            Dictionary<TokenId, int> decimalsMap = transaction.GetTokenIdDecimals();
            Assert.True(decimalsMap.ContainsKey(tokenId));
            Assert.Equal(decimals, decimalsMap[tokenId]);
        }

        public virtual void TestBuildTransactionBody()
        {
            TokenAirdropTransactionBody builder = SpawnTestTransaction().Build();
            Assert.NotNull(builder);
        }

        public virtual void TestGetMethodDescriptor()
        {
            Assert.Equal(TokenServiceGrpc.GetAirdropTokensMethod(), transaction.GetMethodDescriptor());
        }
    }
}