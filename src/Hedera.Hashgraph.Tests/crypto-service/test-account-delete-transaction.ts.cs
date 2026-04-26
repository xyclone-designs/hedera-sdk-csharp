// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;

using VerifyXunit;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.TCK.CryptoService
{
    public class AccountDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private AccountDeleteTransaction SpawnTestTransaction()
        {
            return new AccountDeleteTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				AccountId = AccountId.FromString("0.0.5007"),
				TransferAccountId = AccountId.FromString("0.0.5008"),
				MaxTransactionFee = Hbar.FromTinybars(100000),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<AccountDeleteTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new AccountDeleteTransaction();
            var tx2 = Transaction.FromBytes<AccountDeleteTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody()
            {
                CryptoDelete = new Proto.Services.CryptoDeleteTransactionBody
                {
                    DeleteAccountId = AccountId.FromString("6.6.6").ToProtobuf()
                }
            };
            
            var tx = Transaction.FromScheduledTransaction<AccountDeleteTransaction>(transactionBody);

            Assert.IsType<AccountDeleteTransaction>(tx);
        }
    }
}