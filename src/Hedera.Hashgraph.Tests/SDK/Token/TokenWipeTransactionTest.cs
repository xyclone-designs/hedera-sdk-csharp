// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenWipeTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId testAccountId = AccountId.FromString("0.6.9");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly ulong testAmount = 4;
        private static readonly List<long> testSerialNumbers = [8, 9, 10];
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerializeFungible()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenWipeTransaction();
            var tx2 = Transaction.FromBytes<TokenWipeTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenWipeTransaction SpawnTestTransaction()
        {
            return new TokenWipeTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				TokenId = TokenId.FromString("0.0.111"),
				AccountId = testAccountId,
				Amount = testAmount,
				Serials = [..testSerialNumbers],
				MaxTransactionFee = new Hbar(1)
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerializeNft()
        {
            Verifier.Verify(SpawnTestTransactionNft().ToString());
        }

        private TokenWipeTransaction SpawnTestTransactionNft()
        {
            return new TokenWipeTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				TokenId = TokenId.FromString("0.0.111"),
				AccountId = testAccountId,
				Serials = [444],
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytesFungible()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenWipeTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransactionNft();
            var tx2 = Transaction.FromBytes<TokenWipeTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                TokenWipe = new Proto.TokenWipeAccountTransactionBody()
            };
            
            var tx = Transaction.FromScheduledTransaction<TokenWipeTransaction>(transactionBody);
            Assert.IsType<TokenWipeTransaction>(tx);
        }
        [Fact]
        public virtual void ConstructTokenWipeTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenWipeAccountTransactionBody
            {
                Token = testTokenId.ToProtobuf(),
                Account = testAccountId.ToProtobuf(),
                Amount = testAmount,
                SerialNumbers = { testSerialNumbers }
            };
            var txBody = new Proto.TransactionBody
            {
				TokenWipe = transactionBody
			};
            var tokenWipeTransaction = new TokenWipeTransaction(txBody);

            Assert.Equal(tokenWipeTransaction.TokenId, testTokenId);
            Assert.Equal(tokenWipeTransaction.AccountId, testAccountId);
            Assert.Equal(tokenWipeTransaction.Amount, testAmount);
            Assert.Equal(tokenWipeTransaction.Serials, testSerialNumbers);
        }
        [Fact]
        public virtual void GetSetTokenId()
        {
            var tokenWipeTransaction = new TokenWipeTransaction
            {
				TokenId = testTokenId
			};
            Assert.Equal(tokenWipeTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }
        [Fact]
        public virtual void GetSetAccountId()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.AccountId, testAccountId);
        }
        [Fact]
        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AccountId = testAccountId);
        }
        [Fact]
        public virtual void GetSetAmount()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.Amount, (ulong)testAmount);
        }
        [Fact]
        public virtual void GetSetAmountFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.Amount = (ulong)testAmount);
        }
        [Fact]
        public virtual void GetSetSerialNumbers()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.Serials, testSerialNumbers);
        }
        [Fact]
        public virtual void GetSetSerialNumbersFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.Serials.ClearAndSet(testSerialNumbers));
        }
    }
}