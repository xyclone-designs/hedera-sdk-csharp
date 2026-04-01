// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountUpdateTransactionHooksTest
    {
        public virtual void ShouldAddHookToCreate()
        {
            var tx = new AccountUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            tx.HookCreationDetails.Add(hookDetails);
            var result = tx;
            Assert.Equal(result, tx);
            Assert.Single(tx.HookCreationDetails);
            Assert.Equal(tx.HookCreationDetails[0], hookDetails);
        }

        public virtual void ShouldSetHooksToCreate()
        {
            var tx = new AccountUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, lambdaHook);
            var hooks = new HookCreationDetails[] { hookDetails1, hookDetails2 };
            tx.HookCreationDetails.ClearAndSet(hooks);
            var result = tx;
            Assert.Equal(result, tx);
            //Assert.Equal(2, tx.GetHbarTransfers().Count);
            //Assert.Contains([hookDetails1, hookDetails2], tx.HookCreationDetails);
        }

        public virtual void ShouldAddHookToDelete()
        {
            var tx = new AccountUpdateTransaction();
            var hookId = 123;
            tx.HookIdsToDelete.Add(hookId);
            var result = tx;
            Assert.Equal(result, tx);
            Assert.Single(tx.HookIdsToDelete);
            Assert.Contains(hookId, tx.HookIdsToDelete);
        }

        public virtual void ShouldAddHooksToDelete()
        {
            var tx = new AccountUpdateTransaction();
            var hookIds = new long[] { 123, 456, 789 };
            tx.HookIdsToDelete.AddRange(hookIds);
            var result = tx;
            Assert.Equal(result, tx);
            //Assert.Equal(2, tx.GetHbarTransfers().Count);
            //Assert.Contains([123, 456, 789], tx.HookIdsToDelete);
        }

        public virtual void HooksToCreate()
        {
            var tx = new AccountUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            tx.HookCreationDetails.Add(hookDetails);
            var result = tx.HookCreationDetails;
            Assert.Single(result);
            Assert.Equal(result[0], hookDetails);

            // Verify it returns a copy
            result.Clear();
            Assert.Single(tx.HookCreationDetails);
        }

        public virtual void ShouldGetHooksToDelete()
        {
            var tx = new AccountUpdateTransaction();
            tx.HookIdsToDelete.Add(123);
            var result = tx.HookIdsToDelete;
            Assert.Single(result);
            Assert.Contains(123, result);

            // Verify it returns a copy
            result.Clear();
            Assert.Single(tx.HookIdsToDelete);
        }

        public virtual void ShouldThrowWhenAddingHookAfterFreeze()
        {
            var tx = new AccountUpdateTransaction();
            
            tx.SetNodeAccountIds([ AccountId.FromString("0.0.5005") ]);
            tx.TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), DateTimeOffset.FromUnixTimeMilliseconds(1554158542));
            tx.Freeze();
            
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            
			InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => tx.HookCreationDetails.Add(hookDetails));

			Assert.Contains(exception.Message, "transaction is immutable");
		}

        public virtual void ShouldThrowWhenSettingHooksAfterFreeze()
        {
            var tx = new AccountUpdateTransaction
            {
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), DateTimeOffset.FromUnixTimeMilliseconds(1554158542))
			}
            .SetNodeAccountIds([ AccountId.FromString("0.0.5005") ])
            .Freeze();
            
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);

			InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => tx.HookCreationDetails.ClearAndSet(hookDetails));

			Assert.Contains(exception.Message, "transaction is immutable");
		}

        public virtual void ShouldThrowWhenDeletingHookAfterFreeze()
        {
            var tx = new AccountUpdateTransaction
            {
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), DateTimeOffset.FromUnixTimeMilliseconds(1554158542))
			}
            .SetNodeAccountIds([ AccountId.FromString("0.0.5005") ])
            .Freeze();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => tx.HookIdsToDelete.Add(123));

			Assert.Contains(exception.Message, "transaction is immutable");
		}

        public virtual void ShouldThrowWhenDeletingHooksAfterFreeze()
        {
            var tx = new AccountUpdateTransaction
            {
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), DateTimeOffset.FromUnixTimeMilliseconds(1554158542))
			}
            .SetNodeAccountIds([ AccountId.FromString("0.0.5005") ])
            .Freeze();

			InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => tx.HookIdsToDelete.AddRange([123, 456]));

			Assert.Contains(exception.Message, "transaction is immutable");
		}

        public virtual void ShouldSerializeHooksInBuild()
        {
            var tx = new AccountUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            
            tx.HookCreationDetails.Add(hookDetails);
            tx.HookIdsToDelete.Add(123);
            
            var builder = tx.ToProtobuf();

            Assert.Single(builder.HookCreationDetails);
            Assert.Single(builder.HookIdsToDelete);
            Assert.Contains(123, builder.HookIdsToDelete);
        }

        public virtual void ShouldDeserializeHooksFromTransactionBody()
        {
            var tx = new AccountUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            
            tx.HookCreationDetails.Add(hookDetails);
            tx.HookIdsToDelete.Add(123);
            
            var bytes = tx.ToBytes();
            var deserializedTx = Transaction.FromBytes<AccountUpdateTransaction>(bytes);
            
            Assert.Single(deserializedTx.HookCreationDetails);
            Assert.Single(deserializedTx.HookIdsToDelete);
            Assert.Contains(123, deserializedTx.HookIdsToDelete);
        }

        public virtual void ShouldHandleEmptyHooks()
        {
            var tx = new AccountUpdateTransaction();
            
            Assert.Empty(tx.HookCreationDetails);
            Assert.Empty(tx.HookIdsToDelete);
            
            var builder = tx.ToProtobuf();
            
            Assert.Empty(builder.HookCreationDetails);
            Assert.Empty(builder.HookIdsToDelete);
        }

        public virtual void ShouldSupportMultipleHooks()
        {
            var tx = new AccountUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, lambdaHook);
            
            tx.HookCreationDetails.Add(hookDetails1);
            tx.HookCreationDetails.Add(hookDetails2);
            tx.HookIdsToDelete.Add(100);
            tx.HookIdsToDelete.Add(200);

            //Assert.Equal(2, tx.GetHbarTransfers().Count);
            //Assert.Equal(2, tx.GetHbarTransfers().Count);
            Assert.Contains(100, tx.HookIdsToDelete);
            Assert.Contains(200, tx.HookIdsToDelete);
        }
    }
}