// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using VerifyXunit;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.TCK.FileService
{
    public class FileDeleteTransactionTest
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
            var tx = new FileDeleteTransaction();
            var tx2 = Transaction.FromBytes<FileDeleteTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private FileDeleteTransaction SpawnTestTransaction()
        {
            return new FileDeleteTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				FileId = FileId.FromString("0.0.6006"),
				MaxTransactionFee = Hbar.FromTinybars(100000),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<FileDeleteTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
                FileDelete = new Proto.Services.FileDeleteTransactionBody()
            };

            var tx = Transaction.FromScheduledTransaction<FileDeleteTransaction>(transactionBody);
            
            Assert.IsType<FileDeleteTransaction>(tx);
        }
    }
}