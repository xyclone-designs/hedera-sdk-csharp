// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;
using System;
using Hedera.Hashgraph.SDK.System;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Contract;

namespace Hedera.Hashgraph.Tests.SDK.System
{
    public class SystemUndeleteTransactionTest
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

        public virtual void ShouldSerializeFile()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionFile().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new SystemUndeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private SystemUndeleteTransaction SpawnTestTransactionFile()
        {
            return new SystemUndeleteTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				FileId = FileId.FromString("0.0.444"),
				MaxTransactionFee = new Hbar(1)
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerializeContract()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionContract().ToString()).ToMatchSnapshot();
        }

        private SystemUndeleteTransaction SpawnTestTransactionContract()
        {
            return new SystemUndeleteTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				ContractId = ContractId.FromString("0.0.444"),
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesContract()
        {
            var tx = SpawnTestTransactionContract();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesFile()
        {
            var tx = SpawnTestTransactionFile();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                SystemUndelete = new Proto.SystemUndeleteTransactionBody()
            };
            
            var tx = Transaction.FromScheduledTransaction(transactionBody);

            Assert.IsType<SystemUndeleteTransaction>(tx);
        }
    }
}