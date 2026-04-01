// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Schedule
{
    public class ScheduleDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private ScheduleDeleteTransaction SpawnTestTransaction()
        {
            return new ScheduleDeleteTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				ScheduleId = ScheduleId.FromString("0.0.444"),
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<ScheduleDeleteTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new ScheduleDeleteTransaction();
            var tx2 = Transaction.FromBytes<ScheduleDeleteTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				ScheduleDelete = new Proto.ScheduleDeleteTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<ScheduleDeleteTransaction>(transactionBody);
            
            Assert.IsType<ScheduleDeleteTransaction>(tx);
        }
    }
}