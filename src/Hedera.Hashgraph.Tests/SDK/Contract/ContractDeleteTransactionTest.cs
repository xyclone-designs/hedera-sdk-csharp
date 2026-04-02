// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private ContractDeleteTransaction SpawnTestTransaction()
        {
            return new ContractDeleteTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				ContractId = ContractId.FromString("0.0.5007"),
				TransferAccountId = new AccountId(0, 0, 9),
				TransferContractId = ContractId.FromString("0.0.5008"),
				MaxTransactionFee = Hbar.FromTinybars(100000),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }
        [Fact]
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<ContractDeleteTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new ContractDeleteTransaction();
            var tx2 = Transaction.FromBytes<ContractDeleteTransaction>(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                ContractDeleteInstance = new Proto.ContractDeleteTransactionBody { }
            };
            
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<ContractDeleteTransaction>(tx);
        }
        [Fact]
        public virtual void SetsPermanentRemovalInProtobufBody()
        {
            var tx = new ContractDeleteTransaction
            {
				ContractId = ContractId.FromString("0.0.5007"),
				PermanentRemoval = true
			};
            var proto = tx.ToProtobuf();

            Assert.True(proto.PermanentRemoval);
        }
        [Fact]
        public virtual void ShouldSupportPermanentRemovalBytesRoundTrip()
        {
            var tx = new ContractDeleteTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				ContractId = ContractId.FromString("0.0.5007"),
				TransferAccountId = new AccountId(0, 0, 9),
				PermanentRemoval = true,
				MaxTransactionFee = Hbar.FromTinybars(100000),

			}.Freeze();

            Assert.True(tx.PermanentRemoval);
            Assert.Equal(tx.ContractId, ContractId.FromString("0.0.5007"));
            Assert.Equal(tx.TransferAccountId, new AccountId(0, 0, 9));
            Assert.Null(tx.TransferContractId);
            Assert.Equal(tx.NodeAccountIds, [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")]);
            Assert.Equal(tx.MaxTransactionFee, Hbar.FromTinybars(100000));

            var tx2 = Transaction.FromBytes<ContractDeleteTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
            Assert.True(tx2.PermanentRemoval);
            Assert.Equal(tx2.ContractId, tx.ContractId);
            Assert.Equal(tx2.TransferAccountId, tx.TransferAccountId);
            Assert.Null(tx2.TransferContractId);
            Assert.Equal(tx2.NodeAccountIds, tx.NodeAccountIds);
            Assert.Equal(tx2.MaxTransactionFee, tx.MaxTransactionFee);
        }
    }
}