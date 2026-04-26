// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Cryptography;

using VerifyXunit;

namespace Hedera.Hashgraph.TCK.TokenService
{
    public class TokenFreezeTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private TokenFreezeTransaction SpawnTestTransaction()
        {
            return new TokenFreezeTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				AccountId = AccountId.FromString("0.0.222"),
				TokenId = TokenId.FromString("6.5.4"),
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenFreezeTransaction();
            var tx2 = Transaction.FromBytes<TokenFreezeTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenFreezeTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
				TokenFreeze = new Proto.Services.TokenFreezeAccountTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenFreezeTransaction>(transactionBody);
            
            Assert.IsType<TokenFreezeTransaction>(tx);
        }
    }
}