// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Linq;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractUpdateTransactionHooksTest
    {
        public virtual void ShouldAddHookToCreate()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            tx.HookCreationDetails_.Add(hookDetails);
            var result = tx;
            Assert.Equal(result, tx);
            Assert.Single(tx.HookCreationDetails_);
            Assert.Equal(tx.HookCreationDetails_[0], hookDetails);
        }

        public virtual void ShouldSetHooksToCreate()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, lambdaHook);
            
            tx.HookCreationDetails_.ClearAndSet([hookDetails1, hookDetails2]);
            var result = tx;
            Assert.Equal(result, tx);
            //Assert.Equal(2, tx.HbarTransfers.Count);
            Assert.Contains(hookDetails1, tx.HookCreationDetails_);
            Assert.Contains(hookDetails2, tx.HookCreationDetails_);
        }

        public virtual void ShouldAddHookToDelete()
        {
            var tx = new ContractUpdateTransaction();
            var hookId = 123;
            tx.HookIdsToDelete.Add(hookId);
            var result = tx;
            Assert.Equal(result, tx);
            Assert.Single(tx.HookIdsToDelete);
            Assert.Contains(hookId, tx.HookIdsToDelete);
        }

        public virtual void ShouldAddHooksToDelete()
        {
            var tx = new ContractUpdateTransaction();
            long[] hookIds = [123, 456, 789];
            tx.HookIdsToDelete.ClearAndSet(hookIds);
            var result = tx;
            Assert.Equal(result, tx);
            //Assert.Equal(2, tx.HbarTransfers.Count);
            Assert.Contains(123, tx.HookIdsToDelete);
            Assert.Contains(456, tx.HookIdsToDelete);
            Assert.Contains(789, tx.HookIdsToDelete);
        }

        public virtual void ShouldGetHooksToCreate()
		{
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            tx.HookCreationDetails_.Add(hookDetails);
            var result = tx.HookCreationDetails_;
            Assert.Single(result);
            Assert.Equal(result[0], hookDetails);

            // Verify it returns a copy
            result.Clear();
            Assert.Single(tx.HookCreationDetails_);
        }

        public virtual void ShouldGetHooksToDelete()
		{
            var tx = new ContractUpdateTransaction();
            tx.HookIdsToDelete.Add(123);
            var result = tx.HookIdsToDelete;
            Assert.Single(result);
            Assert.True(result.Contains(123L));

            // Verify it returns a copy
            result.Clear();
            Assert.Single(tx.HookIdsToDelete);
        }

        public virtual void ShouldThrowWhenAddingHookAfterFreeze()
        {
			var tx = new ContractUpdateTransaction
			{
				NodeAccountIds = [AccountId.FromString("0.0.5005")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), DateTimeOffset.FromUnixTimeMilliseconds(1554158542))
			};
			tx.Freeze();
            
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => tx.HookCreationDetails_.Add(hookDetails));
            Assert.Contains(exception.Message, "transaction is immutable");
        }

        public virtual void ShouldThrowWhenSettingHooksAfterFreeze()
        {
			var tx = new ContractUpdateTransaction
			{
				NodeAccountIds = [AccountId.FromString("0.0.5005")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), DateTimeOffset.FromUnixTimeMilliseconds(1554158542))
			};
			tx.Freeze();
            
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => tx.HookCreationDetails_.ClearAndSet(hookDetails));
            Assert.Contains(exception.Message, "transaction is immutable");
        }

        public virtual void ShouldThrowWhenDeletingHookAfterFreeze()
        {
			var tx = new ContractUpdateTransaction
			{
				NodeAccountIds = [AccountId.FromString("0.0.5005")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), DateTimeOffset.FromUnixTimeMilliseconds(1554158542))
			};

			tx.Freeze();
            
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => tx.HookIdsToDelete.Add(123));
            Assert.Contains(exception.Message, "transaction is immutable");
        }

        public virtual void ShouldThrowWhenDeletingHooksAfterFreeze()
        {
            var tx = new ContractUpdateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), DateTimeOffset.FromUnixTimeMilliseconds(1554158542))
			};

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => tx.HookIdsToDelete.ClearAndSet(123, 456));
            Assert.Contains(exception.Message, "transaction is immutable");
        }

        public virtual void ShouldSerializeHooksInToProtobuf()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            
            tx.HookCreationDetails_.Add(hookDetails);
            tx.HookIdsToDelete.Add(123);

            var builder = tx.ToProtobuf();
            Assert.Single(builder.HookCreationDetails);
            Assert.Single(builder.HookIdsToDelete);
            Assert.Contains(123, builder.HookIdsToDelete);
        }

        public virtual void ShouldDeserializeHooksFromTransactionBody()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            
            tx.HookCreationDetails_.Add(hookDetails);
            tx.HookIdsToDelete.Add(123);
            
            var bytes = tx.ToBytes();
            var deserializedTx = Transaction.FromBytes<ContractUpdateTransaction>(bytes);

            Assert.Single(deserializedTx.HookCreationDetails_);
            Assert.Single(deserializedTx.HookIdsToDelete);
            Assert.Contains(123, deserializedTx.HookIdsToDelete);
        }

        public virtual void ShouldHandleEmptyHooks()
        {
            var tx = new ContractUpdateTransaction();
            Assert.Empty(tx.HookCreationDetails_);
            Assert.Empty(tx.HookIdsToDelete);
            var builder = tx.ToProtobuf();

            Assert.Empty(builder.HookCreationDetails);
            Assert.Empty(builder.HookIdsToDelete);
        }

        public virtual void ShouldSupportMultipleHooks()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, lambdaHook);
            
            tx.HookCreationDetails_.Add(hookDetails1);
            tx.HookCreationDetails_.Add(hookDetails2);
            tx.HookIdsToDelete.Add(100);
            tx.HookIdsToDelete.Add(200);

            //Assert.Equal(2, tx.HbarTransfers.Count);

            Assert.Contains(100, tx.HookIdsToDelete);
            Assert.Contains(200, tx.HookIdsToDelete);
        }
    }
}