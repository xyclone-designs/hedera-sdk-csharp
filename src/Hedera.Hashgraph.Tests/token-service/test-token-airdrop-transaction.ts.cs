// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptography;

using VerifyXunit;

namespace Hedera.Hashgraph.TCK.TokenService
{
    public class TokenAirdropTransactionTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private TokenAirdropTransaction transaction;
        public virtual void SetUp()
        {
            transaction = new TokenAirdropTransaction();
        }

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenAirdropTransaction();
            var tx2 = Transaction.FromBytes<TokenAirdropTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenAirdropTransaction SpawnTestTransaction()
        {
            return new TokenAirdropTransaction
            {
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)
            }
                .SetNodeAccountIds([AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")])
                .AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5008"), 400)
                .AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5006"), -800, 3)
                .AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.5007"), 400, 3)
                .AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5008"), 1)
                .AddTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), -1)
                .AddNftTransfer(TokenId.FromString("0.0.3").Nft(2), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007"))
                .AddNftTransfer(TokenId.FromString("0.0.3").Nft(1), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5007"))
                .AddNftTransfer(TokenId.FromString("0.0.3").Nft(3), AccountId.FromString("0.0.5008"), AccountId.FromString("0.0.5006"))
                .AddNftTransfer(TokenId.FromString("0.0.3").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006"))
                .AddNftTransfer(TokenId.FromString("0.0.2").Nft(4), AccountId.FromString("0.0.5007"), AccountId.FromString("0.0.5006"))
                .AddApprovedTokenTransfer(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), 123)
                .AddApprovedNftTransfer(new NftId(TokenId.FromString("0.0.4"), 4), AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))
                .Freeze()
                .Sign(privateKey);
        }

        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenAirdropTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        [Fact]
        public virtual void DecimalsMustBeConsistent()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new TokenAirdropTransaction()
                .AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100, 2)
                .AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 3);
            });
        }

        [Fact]
        public virtual void CanGetDecimals()
        {
            var tx = new TokenAirdropTransaction();
            
            Assert.Null(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]);
            
            tx.AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100);
            
            Assert.Null(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]);
            
            tx.AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 5);
            
            Assert.Equal(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")], (uint)5);
        }

        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
				TokenAirdrop = new Proto.Services.TokenAirdropTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenAirdropTransaction>(transactionBody);
            
            Assert.IsType<TokenAirdropTransaction>(tx);
        }

        [Fact]
        public virtual void TestDefaultMaxTransactionFeeIsSet()
        {
            Assert.Equal(new Hbar(1), transaction.DefaultMaxTransactionFee);
            //Assert.Equal(new Hbar(1), transaction.DefaultMaxTransactionFee, "Default max transaction fee should be 1 Hbar");
        }

        [Fact]
        public virtual void TestAddTokenTransfer()
        {
            TokenId tokenId = new (0, 0, 123);
            AccountId accountId = new (0, 0, 456);
            long value = 1000;
            transaction
                .AddTokenTransfer(tokenId, accountId, value);
            Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers = transaction.GetTokenTransfers();
            Assert.True(tokenTransfers.ContainsKey(tokenId));
            Assert.Equal(1, tokenTransfers[tokenId].Count);
            Assert.Equal(value, tokenTransfers[tokenId][accountId]);
        }

        [Fact]
        public virtual void TestAddApprovedTokenTransfer()
        {
            TokenId tokenId = new (0, 0, 123);
            AccountId accountId = new (0, 0, 456);
            long value = 1000;
            
            transaction.AddApprovedTokenTransfer(tokenId, accountId, value);

            Dictionary<TokenId, Dictionary<AccountId, long>> tokenTransfers = transaction.GetTokenTransfers();
            Assert.True(tokenTransfers.ContainsKey(tokenId));
            Assert.Equal(1, tokenTransfers[tokenId].Count);
            Assert.Equal(value, tokenTransfers[tokenId][accountId]);
        }

        [Fact]
        public virtual void TestAddNftTransfer()
        {
            NftId nftId = new NftId(new TokenId(0, 0, 123), 1);
            AccountId sender = new (0, 0, 456);
            AccountId receiver = new (0, 0, 789);
            
            transaction.AddNftTransfer(nftId, sender, receiver);

            Dictionary<TokenId, IList<TokenNftTransfer>> nftTransfers = transaction.GetTokenNftTransfers();
            Assert.True(nftTransfers.ContainsKey(nftId.TokenId));
            Assert.Equal(1, nftTransfers[nftId.TokenId].Count);
            Assert.Equal(sender, nftTransfers[nftId.TokenId][0].Sender);
            Assert.Equal(receiver, nftTransfers[nftId.TokenId][0].Receiver);
        }

        [Fact]
        public virtual void TestAddApprovedNftTransfer()
        {
            NftId nftId = new (new TokenId(0, 0, 123), 1);
            AccountId sender = new (0, 0, 456);
            AccountId receiver = new (0, 0, 789);
            
            transaction.AddApprovedNftTransfer(nftId, sender, receiver);

            Dictionary<TokenId, IList<TokenNftTransfer>> nftTransfers = transaction.GetTokenNftTransfers();
            Assert.True(nftTransfers.ContainsKey(nftId.TokenId));
            Assert.Equal(1, nftTransfers[nftId.TokenId].Count);
            Assert.Equal(sender, nftTransfers[nftId.TokenId][0].Sender);
            Assert.Equal(receiver, nftTransfers[nftId.TokenId][0].Receiver);
        }

        [Fact]
        public virtual void TestGetTokenIdDecimals()
        {
            TokenId tokenId = new (0, 0, 123);
            AccountId accountId = new (0, 0, 456);
            long value = 1000;
            uint decimals = 8;
            transaction.AddTokenTransferWithDecimals(tokenId, accountId, value, decimals);
            Dictionary<TokenId, uint?> decimalsMap = transaction.GetTokenIdDecimals();
            Assert.True(decimalsMap.ContainsKey(tokenId));
            Assert.Equal(decimals, decimalsMap[tokenId]);
        }

        [Fact]
        public virtual void TestBuildTransactionBody()
        {
            Proto.Services.TokenAirdropTransactionBody builder = SpawnTestTransaction().ToProtobuf();
            Assert.NotNull(builder);
        }

        [Fact]
        public virtual void TestGetMethodDescriptor()
        {
            //Assert.Equal(Proto.Services.TokenService.Descriptor.GetAirdropTokensMethod(), transaction.GetMethodDescriptor());
        }
    }
}