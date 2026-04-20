// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenPauseTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public virtual TokenPauseTransaction SpawnTestTransaction()
        {
            return new TokenPauseTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				TokenId = testTokenId,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        [Fact]
        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenPauseTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenPauseTransaction();
            var tx2 = Transaction.FromBytes<TokenPauseTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenPause = new Proto.TokenPauseTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenPauseTransaction>(transactionBody);

            Assert.IsType<TokenPauseTransaction>(tx);
        }
        [Fact]
        public virtual void ConstructTokenPauseTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenPauseTransactionBody
            {
				Token = testTokenId.ToProtobuf()
			};
            var tx = new Proto.Services.TransactionBody
            {
				TokenPause = transactionBody
			};
            var tokenPauseTransaction = new TokenPauseTransaction(tx);

            Assert.Equal(tokenPauseTransaction.TokenId, testTokenId);
        }
        [Fact]
        public virtual void GetSetTokenId()
        {
            var tokenPauseTransaction = new TokenPauseTransaction
            {
				TokenId = testTokenId
			};
            Assert.Equal(tokenPauseTransaction.TokenId, testTokenId);
        }
        [Fact]
        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }
    }
}