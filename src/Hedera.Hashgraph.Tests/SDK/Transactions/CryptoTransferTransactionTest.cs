// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Nfts;

using Google.Protobuf;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class CryptoTransferTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TransferTransaction();
            var tx2 = Transaction.FromBytes<TransferTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TransferTransaction SpawnTestTransaction()
        {
            return new TransferTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				MaxTransactionFee = Hbar.FromTinybars(100000),
			}
                .AddHbarTransfer(AccountId.FromString("0.0.5008"), Hbar.FromTinybars(400))
                .AddHbarTransfer(AccountId.FromString("0.0.5006"), Hbar.FromTinybars(800).Negated())
                .AddHbarTransfer(AccountId.FromString("0.0.5007"), Hbar.FromTinybars(400))
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
                .SetHbarTransferApproval(AccountId.FromString("0.0.5007"), true)
                .SetTokenTransferApproval(TokenId.FromString("0.0.4"), AccountId.FromString("0.0.5006"), true)
                .SetNftTransferApproval(new NftId(TokenId.FromString("0.0.4"), 4), true)
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        private TransferTransaction SpawnModifiedTestTransaction()
        {
			return new TransferTransaction
			{
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				MaxTransactionFee = Hbar.FromTinybars(100000),
			}
                .AddHbarTransfer(AccountId.FromString("0.0.5008"), Hbar.FromTinybars(400))
                .AddHbarTransfer(AccountId.FromString("0.0.5006"), Hbar.FromTinybars(800).Negated())
                .AddHbarTransfer(AccountId.FromString("0.0.5007"), Hbar.FromTinybars(400))
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
                .SetHbarTransferApproval(AccountId.FromString("0.0.5007"), true)
                .SetNftTransferApproval(new NftId(TokenId.FromString("0.0.4"), 4), true)
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TransferTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void DecimalsMustBeConsistent()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new TransferTransaction()
                    .AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100, 2)
                    .AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 3);
            });
        }
        [Fact]
        public virtual void CanGetDecimals()
        {
            var tx = new TransferTransaction();
            Assert.Null(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]);
            tx.AddTokenTransfer(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.8"), 100);
            Assert.Null(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")]);
            tx.AddTokenTransferWithDecimals(TokenId.FromString("0.0.5"), AccountId.FromString("0.0.7"), -100, 5);
            Assert.Equal(tx.GetTokenIdDecimals()[TokenId.FromString("0.0.5")], (uint)5);
        }
        [Fact]
        public virtual void TransactionBodiesMustMatch()
        {
            Proto.Services.Transaction tx1 = Proto.Services.TransactionList.Parser.ParseFrom(SpawnTestTransaction().ToBytes()).TransactionList_[0];
            Proto.Services.Transaction tx2 = Proto.Services.TransactionList.Parser.ParseFrom(SpawnModifiedTestTransaction().ToBytes()).TransactionList_[1];
            var brokenTxList = new Proto.Services.TransactionList();
            brokenTxList.TransactionList_.Add(tx1);
            brokenTxList.TransactionList_.Add(tx2);
            var brokenTxBytes = brokenTxList.ToByteArray();

            Assert.Throws<ArgumentException>(() =>
            {
                Transaction.FromBytes<TransferTransaction>(brokenTxBytes);
            });
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody()
            {
                CryptoTransfer = new Proto.CryptoTransferTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TransferTransaction>(transactionBody);
            Assert.IsType<TransferTransaction>(tx);
        }
    }
}