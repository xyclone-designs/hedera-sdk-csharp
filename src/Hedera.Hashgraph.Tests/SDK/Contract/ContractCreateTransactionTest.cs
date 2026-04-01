// SPDX-License-Identifier: Apache-2.0
using System;

using Org.BouncyCastle.Utilities.Encoders;

using Google.Protobuf;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.File;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractCreateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        public virtual void ShouldSerialize2()
        {
            Verifier.Verify(SpawnTestTransaction2().ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new ContractCreateTransaction();
            var tx2 = Transaction.FromBytes<ContractCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private ContractCreateTransaction SpawnTestTransaction()
        {
            return new ContractCreateTransaction
			{
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				BytecodeFileId = FileId.FromString("0.0.3003"),

				AdminKey = unusedPrivateKey,
				Gas = 0,
				InitialBalance = Hbar.FromTinybars(1000),
				StakedAccountId = AccountId.FromString("0.0.3"),
                MaxAutomaticTokenAssociations = 101,
				AutoRenewPeriod = TimeSpan.FromHours(10),
				ConstructorParameters = ByteString.CopyFrom([10, 11, 12, 13, 25]),
				MaxTransactionFee = Hbar.FromTinybars(100000),
				AutoRenewAccountId = new AccountId(0, 0, 30),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        private ContractCreateTransaction SpawnTestTransaction2()
        {
            return new ContractCreateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				Bytecode = Hex.Decode("deadbeef"),
				AdminKey = unusedPrivateKey,
				Gas = 0,
				InitialBalance = Hbar.FromTinybars(1000),
				StakedNodeId = 4,
				MaxAutomaticTokenAssociations = 101,
				AutoRenewPeriod = TimeSpan.FromHours(10),
				ConstructorParameters = ByteString.CopyFrom([ 10, 11, 12, 13, 25 ]),
				MaxTransactionFee = Hbar.FromTinybars(100000),
				AutoRenewAccountId = new AccountId(0, 0, 30),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<ContractCreateTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytes2()
        {
            var tx = SpawnTestTransaction2();
            var tx2 = Transaction.FromBytes<ContractCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx2.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                ContractCreateInstance = new Proto.ContractCreateTransactionBody { }
            };
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<ContractCreateTransaction>(tx);
        }

        public virtual void SetGasShouldRejectNegativeValues()
        {
            var tx = new ContractCreateTransaction();
            var ex = Assert.Throws<ArgumentException>(() => tx.Gas = -1);

            Assert.Equal(ex.Message, "Gas must be non-negative");
        }

        public virtual void SetGasShouldAcceptZeroAndPositiveValues()
        {
            var tx = new ContractCreateTransaction();
            tx.Gas = 0;
            Assert.Equal(tx.Gas, 0);
            tx.Gas = 123456;
            Assert.Equal(tx.Gas, 123456);
        }
    }
}