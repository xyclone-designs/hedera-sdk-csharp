// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.LiveHashes;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.Tests.SDK.LiveHashes
{
    class LiveHashDeleteTransactionTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
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

        private LiveHashDeleteTransaction SpawnTestTransaction()
        {
            return new LiveHashDeleteTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				AccountId = AccountId.FromString("0.0.100"),
				Hash = Encoding.UTF8.GetBytes("hash"),
			}
            .Freeze()
            .Sign(privateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new LiveHashDeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
    }
}