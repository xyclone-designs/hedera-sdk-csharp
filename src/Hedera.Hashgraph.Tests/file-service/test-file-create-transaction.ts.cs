// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Cryptocurrency;

using VerifyXunit;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.TCK.FileService
{
    public class FileCreateTransactionTest
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
            var tx = new FileCreateTransaction();
            var tx2 = Transaction.FromBytes<FileCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private FileCreateTransaction SpawnTestTransaction()
        {
            return new FileCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				Contents = new byte[] { 1, 2, 3, 4 },
				ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1554158728),
				Keys = KeyList.Of(null, unusedPrivateKey),
				MaxTransactionFee = Hbar.FromTinybars(100000),
				FileMemo = "Hello memo",
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<FileCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
				FileCreate = new Proto.Services.FileCreateTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<FileCreateTransaction>(transactionBody);

            Assert.IsType<FileCreateTransaction>(tx);
        }
    }
}