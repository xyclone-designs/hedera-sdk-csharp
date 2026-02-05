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
    class TokenAirdropTransactionTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
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

        virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenAirdropTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenAirdropTransaction SpawnTestTransaction()
        {
            return new TokenAirdropTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5008"), 400).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5006"), -800, 3).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5007"), 400, 3).AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5008"), 1).AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), -1).AddNftTransfer(TokenId.FromString("0.0.3").Nft(2), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(1), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(3), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5006")).AddNftTransfer(TokenId.FromString("0.0.3").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006")).AddNftTransfer(TokenId.FromString("0.0.2").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006")).AddApprovedTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), 123).AddApprovedNftTransfer(new NftId(TokenId.FromString("0.0.4"), 4), AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(privateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenAirdropTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void DecimalsMustBeConsistent()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                new TokenAirdropTransaction().AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100, 2).AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 3);
            });
        }

        virtual void CanGetDecimals()
        {
            var tx = new TokenAirdropTransaction();
            AssertThat(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]).IsNull();
            tx.AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100);
            AssertThat(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]).IsNull();
            tx.AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 5);
            Assert.Equal(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")], 5);
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenAirdrop(TokenAirdropTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenAirdropTransaction>(tx);
        }

        virtual void TestDefaultMaxTransactionFeeIsSet()
        {
            AssertEquals(new Hbar(1), transaction.GetDefaultMaxTransactionFee(), "Default max transaction fee should be 1 Hbar");
        }

        virtual void TestAddTokenTransfer()
        {
            TokenId tokenId = new TokenId(0, 0, 123);
            AccountId accountId = new AccountId(0, 0, 456);
            long value = 1000;
            transaction.AddTokenTransfer(tokenId, accountId, value);
            Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers = transaction.GetTokenTransfers();
            AssertTrue(tokenTransfers.ContainsKey(tokenId));
            AssertEquals(1, tokenTransfers[tokenId].Count);
            AssertEquals(value, tokenTransfers[tokenId][accountId]);
        }

        virtual void TestAddApprovedTokenTransfer()
        {
            TokenId tokenId = new TokenId(0, 0, 123);
            AccountId accountId = new AccountId(0, 0, 456);
            long value = 1000;
            transaction.AddApprovedTokenTransfer(tokenId, accountId, value);
            Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers = transaction.GetTokenTransfers();
            AssertTrue(tokenTransfers.ContainsKey(tokenId));
            AssertEquals(1, tokenTransfers[tokenId].Count);
            AssertEquals(value, tokenTransfers[tokenId][accountId]);
        }

        virtual void TestAddNftTransfer()
        {
            NftId nftId = new NftId(new TokenId(0, 0, 123), 1);
            AccountId sender = new AccountId(0, 0, 456);
            AccountId receiver = new AccountId(0, 0, 789);
            transaction.AddNftTransfer(nftId, sender, receiver);
            Dictionary<TokenId, IList<TokenNftTransfer>> nftTransfers = transaction.GetTokenNftTransfers();
            AssertTrue(nftTransfers.ContainsKey(nftId.tokenId));
            AssertEquals(1, nftTransfers[nftId.tokenId].Count);
            AssertEquals(sender, nftTransfers[nftId.tokenId][0].sender);
            AssertEquals(receiver, nftTransfers[nftId.tokenId][0].receiver);
        }

        virtual void TestAddApprovedNftTransfer()
        {
            NftId nftId = new NftId(new TokenId(0, 0, 123), 1);
            AccountId sender = new AccountId(0, 0, 456);
            AccountId receiver = new AccountId(0, 0, 789);
            transaction.AddApprovedNftTransfer(nftId, sender, receiver);
            Dictionary<TokenId, IList<TokenNftTransfer>> nftTransfers = transaction.GetTokenNftTransfers();
            AssertTrue(nftTransfers.ContainsKey(nftId.tokenId));
            AssertEquals(1, nftTransfers[nftId.tokenId].Count);
            AssertEquals(sender, nftTransfers[nftId.tokenId][0].sender);
            AssertEquals(receiver, nftTransfers[nftId.tokenId][0].receiver);
        }

        virtual void TestGetTokenIdDecimals()
        {
            TokenId tokenId = new TokenId(0, 0, 123);
            AccountId accountId = new AccountId(0, 0, 456);
            long value = 1000;
            int decimals = 8;
            transaction.AddTokenTransferWithDecimals(tokenId, accountId, value, decimals);
            Dictionary<TokenId, int> decimalsMap = transaction.GetTokenIdDecimals();
            AssertTrue(decimalsMap.ContainsKey(tokenId));
            AssertEquals(decimals, decimalsMap[tokenId]);
        }

        virtual void TestBuildTransactionBody()
        {
            TokenAirdropTransactionBody.Builder builder = SpawnTestTransaction().Build();
            AssertNotNull(builder);
        }

        virtual void TestGetMethodDescriptor()
        {
            AssertEquals(TokenServiceGrpc.GetAirdropTokensMethod(), transaction.GetMethodDescriptor());
        }
    }
}