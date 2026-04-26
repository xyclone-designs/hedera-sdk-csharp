// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptography;

using VerifyXunit;

namespace Hedera.Hashgraph.TCK.TokenService
{
    public class TokenGrantKycTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly AccountId testAccountId = AccountId.FromString("6.9.0");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private TokenGrantKycTransaction SpawnTestTransaction()
        {
            return new TokenGrantKycTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				AccountId = testAccountId,
				TokenId = testTokenId,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenGrantKycTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenGrantKycTransaction();
            var tx2 = Transaction.FromBytes<TokenGrantKycTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
                TokenGrantKyc = new Proto.Services.TokenGrantKycTransactionBody()
            };
            
            var tx = Transaction.FromScheduledTransaction<TokenGrantKycTransaction>(transactionBody);

            Assert.IsType<TokenGrantKycTransaction>(tx);
        }
        [Fact]
        public virtual void ConstructTokenGrantKycTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.Services.TokenGrantKycTransactionBody
            {
				Account = testAccountId.ToProtobuf(),
				Token = testTokenId.ToProtobuf(),
			};
            var tx = new Proto.Services.TransactionBody
            {
				TokenGrantKyc = transactionBody
			};
            var tokenGrantKycTransaction = new TokenGrantKycTransaction(tx);
            
            Assert.Equal(tokenGrantKycTransaction.TokenId, testTokenId);
        }
        [Fact]
        public virtual void GetSetAccountId()
        {
            var tokenGrantKycTransaction = new TokenGrantKycTransaction
            {
				AccountId = testAccountId
			};
            
            Assert.Equal(tokenGrantKycTransaction.AccountId, testAccountId);
        }
        [Fact]
        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            
            Assert.Throws<InvalidOperationException>(() => tx.AccountId = testAccountId);
        }
        [Fact]
        public virtual void GetSetTokenId()
        {
            var tokenGrantKycTransaction = new TokenGrantKycTransaction
            {
				TokenId = testTokenId
			};

            Assert.Equal(tokenGrantKycTransaction.TokenId, testTokenId);
        }
        [Fact]
        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();

            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }
    }
}