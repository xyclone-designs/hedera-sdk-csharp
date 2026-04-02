// SPDX-License-Identifier: Apache-2.0
using System;

using Google.Protobuf;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractExecuteTransactionTest
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
            var tx = new ContractExecuteTransaction();
            var tx2 = Transaction.FromBytes<ContractExecuteTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private ContractExecuteTransaction SpawnTestTransaction()
        {
            return new ContractExecuteTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				ContractId = ContractId.FromString("0.0.5007"),
				Gas = 10,
				PayableAmount = Hbar.FromTinybars(1000),
				FunctionParameters = ByteString.CopyFrom(new byte[] { 24, 43, 11 }),
				MaxTransactionFee = Hbar.FromTinybars(100000),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<ContractExecuteTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                ContractCall = new Proto.ContractCallTransactionBody { }
            };
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<ContractExecuteTransaction>(tx);
        }
        [Fact]
        public virtual void SetGasShouldRejectNegativeValues()
        {
            var tx = new ContractExecuteTransaction();
            var ex = Assert.Throws<ArgumentException>(() => tx.Gas = -1);
            
            Assert.Equal(ex.Message, "Gas must be non-negative");
        }
        [Fact]
        public virtual void SetGasShouldAcceptZeroAndPositiveValues()
        {
            var tx = new ContractExecuteTransaction();
            tx.Gas = 0;
            Assert.Equal(tx.Gas, 0);
            tx.Gas = 123456;
            Assert.Equal(tx.Gas, 123456);
        }
    }
}