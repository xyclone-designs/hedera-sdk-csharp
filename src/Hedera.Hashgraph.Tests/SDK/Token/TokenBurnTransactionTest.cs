// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenBurnTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly ulong testAmount = 69;
        private static readonly List<long> testSerials = [420];
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerializeFungible()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private TokenBurnTransaction SpawnTestTransaction()
        {
            return new TokenBurnTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                TokenId = testTokenId,
                Amount = testAmount,
                MaxTransactionFee = new Hbar(1),
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenBurnTransaction();
            var tx2 = ITransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerializeNft()
        {
            Verifier.Verify(SpawnTestTransactionNft().ToString());
        }

        private TokenBurnTransaction SpawnTestTransactionNft()
        {
            return new TokenBurnTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                TokenId = testTokenId,
                Serials = [.. testSerials],
                MaxTransactionFee = new Hbar(1),
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytesFungible()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenBurnTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransactionNft();
            var tx2 = Transaction.FromBytes<TokenBurnTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                TokenBurn = new Proto.TokenBurnTransactionBody { }
            };
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenBurnTransaction>(tx);
        }
        [Fact]
        public virtual void ConstructTokenBurnTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenBurnTransactionBody
            {
				Token = testTokenId.ToProtobuf(),
				Amount = testAmount,
                SerialNumbers = { testSerials }
            };
            var tx = new Proto.TransactionBody
            {
                TokenBurn = transactionBody
            };

            var tokenBurnTransaction = new TokenBurnTransaction(tx);
            
            Assert.Equal(tokenBurnTransaction.TokenId, testTokenId);
            Assert.Equal(tokenBurnTransaction.Amount, testAmount);
            Assert.Equal(tokenBurnTransaction.Serials, testSerials);
        }
        [Fact]
        public virtual void GetSetTokenId()
        {
            var tokenBurnTransaction = new TokenBurnTransaction { TokenId = testTokenId };
            
            Assert.Equal(tokenBurnTransaction.TokenId, testTokenId);
        }
        [Fact]
        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }
        [Fact]
        public virtual void GetSetAmount()
        {
            var tokenBurnTransaction = new TokenBurnTransaction { Amount = testAmount };

            Assert.Equal(tokenBurnTransaction.Amount, testAmount);
        }
        [Fact]
        public virtual void GetSetAmountFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.Amount = testAmount);
        }
        [Fact]
        public virtual void GetSetSerials()
        {
            var tokenBurnTransaction = new TokenBurnTransaction
            {
				Serials = [.. testSerials]
			};
            
            Assert.Equal(tokenBurnTransaction.Serials, testSerials);
        }
        [Fact]
        public virtual void GetSetSerialsFrozen()
        {
            var tx = SpawnTestTransactionNft();
            
            // Assert.Throws<InvalidOperationException>(() => tx.Serials = testSerials);
        }
    }
}