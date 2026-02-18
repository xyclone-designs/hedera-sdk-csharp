// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;

using System;

namespace Hedera.Hashgraph.Tests.SDK.File
{
    public class FileUpdateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        private FileUpdateTransaction SpawnTestTransaction()
        {
            return new FileUpdateTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				FileId = FileId.FromString("0.0.6006"),
				ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1554158728).ToTimestamp(),
				Contents = ByteString.CopyFrom([1, 2, 3, 4, 5]),
				MaxTransactionFee = Hbar.FromTinybars(100000),
				Keys = KeyList.Of(null, unusedPrivateKey),
				FileMemo = "Hello memo",
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new FileUpdateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                FileUpdate = new Proto.FileUpdateTransactionBody { }
            };
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<FileUpdateTransaction>(tx);
        }
    }
}