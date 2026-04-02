// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountAllowanceDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual AccountAllowanceDeleteTransaction SpawnTestTransaction()
        {
            var ownerId = AccountId.FromString("5.6.7");
            return new AccountAllowanceDeleteTransaction
            {
				MaxTransactionFee = Hbar.FromTinybars(100000),
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
			}
            .DeleteAllHbarAllowances(ownerId)
            .DeleteAllTokenAllowances(TokenId.FromString("2.2.2"), ownerId)
            .DeleteAllTokenNftAllowances(TokenId.FromString("4.4.4").Nft(123), ownerId)
            .DeleteAllTokenNftAllowances(TokenId.FromString("4.4.4").Nft(456), ownerId)
            .DeleteAllTokenNftAllowances(TokenId.FromString("8.8.8").Nft(456), ownerId)
            .DeleteAllTokenNftAllowances(TokenId.FromString("4.4.4").Nft(789), ownerId)
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<AccountAllowanceDeleteTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new AccountAllowanceDeleteTransaction();
            var tx2 = Transaction.FromBytes<AccountAllowanceDeleteTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody 
            {
				CryptoDeleteAllowance = new Proto.CryptoDeleteAllowanceTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<AccountAllowanceDeleteTransaction>(transactionBody);

            Assert.IsType<AccountAllowanceDeleteTransaction>(tx);
        }
    }
}