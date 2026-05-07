// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Systems;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Contract;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.System
{
    public class SystemUndeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerializeFile()
        {
            Verifier.Verify(SpawnTestTransactionFile().ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new SystemUndeleteTransaction();
            var tx2 = Transaction.FromBytes<SystemUndeleteTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private SystemUndeleteTransaction SpawnTestTransactionFile()
        {
            return new SystemUndeleteTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				FileId = FileId.FromString("0.0.444"),
				MaxTransactionFee = new Hbar(1)
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerializeContract()
        {
            Verifier.Verify(SpawnTestTransactionContract().ToString());
        }

        private SystemUndeleteTransaction SpawnTestTransactionContract()
        {
            return new SystemUndeleteTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				ContractId = ContractId.FromString("0.0.444"),
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytesContract()
        {
            var tx = SpawnTestTransactionContract();
            var tx2 = Transaction.FromBytes<SystemUndeleteTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesFile()
        {
            var tx = SpawnTestTransactionFile();
            var tx2 = Transaction.FromBytes<SystemUndeleteTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
                SystemUndelete = new Proto.Services.SystemUndeleteTransactionBody()
            };
            
            var tx = Transaction.FromScheduledTransaction(transactionBody);

            Assert.IsType<SystemUndeleteTransaction>(tx);
        }
    }
}