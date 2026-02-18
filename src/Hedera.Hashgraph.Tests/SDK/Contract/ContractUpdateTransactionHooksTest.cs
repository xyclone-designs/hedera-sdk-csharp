// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
            var result = tx.AddHookToCreate(hookDetails);
            Assert.Equal(result, tx);
            Assert.Single(tx.GetHooksToCreate());
            Assert.Equal(tx.GetHooksToCreate()[0], hookDetails);
        }

        public virtual void ShouldSetHooksToCreate()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, lambdaHook);
            var hooks = List.Of(hookDetails1, hookDetails2);
            var result = tx.SetHooksToCreate(hooks);
            Assert.Equal(result, tx);
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            AssertThat(tx.GetHooksToCreate()).ContainsExactlyInAnyOrder(hookDetails1, hookDetails2);
        }

        public virtual void ShouldAddHookToDelete()
        {
            var tx = new ContractUpdateTransaction();
            var hookId = 123;
            var result = tx.AddHookToDelete(hookId);
            Assert.Equal(result, tx);
            Assert.Single(tx.GetHooksToDelete());
            Assert.Contains(tx.GetHooksToDelete(), hookId);
        }

        public virtual void ShouldAddHooksToDelete()
        {
            var tx = new ContractUpdateTransaction();
            var hookIds = List.Of(123, 456, 789);
            var result = tx.SetHooksToDelete(hookIds);
            Assert.Equal(result, tx);
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            AssertThat(tx.GetHooksToDelete()).ContainsExactlyInAnyOrder(123, 456, 789);
        }

        public virtual void ShouldGetHooksToCreate()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            tx.AddHookToCreate(hookDetails);
            var result = tx.GetHooksToCreate();
            Assert.Single(result);
            Assert.Equal(result[0], hookDetails);

            // Verify it returns a copy
            result.Clear();
            Assert.Single(tx.GetHooksToCreate());
        }

        public virtual void ShouldGetHooksToDelete()
        {
            var tx = new ContractUpdateTransaction();
            tx.AddHookToDelete(123);
            var result = tx.GetHooksToDelete();
            Assert.Single(result);
            Assert.Contains(result, 123);

            // Verify it returns a copy
            result.Clear();
            Assert.Single(tx.GetHooksToDelete());
        }

        public virtual void ShouldThrowWhenAddingHookAfterFreeze()
        {
            var tx = new ContractUpdateTransaction();
            tx.SetNodeAccountIds(java.util.Arrays.AsList(AccountId.FromString("0.0.5005")));
            tx.SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), java.time.DateTimeOffset.FromUnixTimeMilliseconds(1554158542)));
            tx.Freeze();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            AssertThatThrownBy(() => tx.AddHookToCreate(hookDetails)).IsInstanceOf(typeof(InvalidOperationException)).HasMessageContaining("transaction is immutable");
        }

        public virtual void ShouldThrowWhenSettingHooksAfterFreeze()
        {
            var tx = new ContractUpdateTransaction();
            tx.SetNodeAccountIds(java.util.Arrays.AsList(AccountId.FromString("0.0.5005")));
            tx.SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), java.time.DateTimeOffset.FromUnixTimeMilliseconds(1554158542)));
            tx.Freeze();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            AssertThatThrownBy(() => tx.SetHooksToCreate(List.Of(hookDetails))).IsInstanceOf(typeof(InvalidOperationException)).HasMessageContaining("transaction is immutable");
        }

        public virtual void ShouldThrowWhenDeletingHookAfterFreeze()
        {
            var tx = new ContractUpdateTransaction();
            tx.SetNodeAccountIds(java.util.Arrays.AsList(AccountId.FromString("0.0.5005")));
            tx.SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), java.time.DateTimeOffset.FromUnixTimeMilliseconds(1554158542)));
            tx.Freeze();
            AssertThatThrownBy(() => tx.AddHookToDelete(123)).IsInstanceOf(typeof(InvalidOperationException)).HasMessageContaining("transaction is immutable");
        }

        public virtual void ShouldThrowWhenDeletingHooksAfterFreeze()
        {
            var tx = new ContractUpdateTransaction();
            tx.SetNodeAccountIds(java.util.Arrays.AsList(AccountId.FromString("0.0.5005")));
            tx.SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), java.time.DateTimeOffset.FromUnixTimeMilliseconds(1554158542)));
            tx.Freeze();
            AssertThatThrownBy(() => tx.SetHooksToDelete(List.Of(123, 456))).IsInstanceOf(typeof(InvalidOperationException)).HasMessageContaining("transaction is immutable");
        }

        public virtual void ShouldThrowWhenAddingNullHook()
        {
            var tx = new ContractUpdateTransaction();
            AssertThatThrownBy(() => tx.AddHookToCreate(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessageContaining("hookDetails cannot be null");
        }

        public virtual void ShouldThrowWhenSettingNullHooks()
        {
            var tx = new ContractUpdateTransaction();
            AssertThatThrownBy(() => tx.SetHooksToCreate(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessageContaining("hookDetails cannot be null");
        }

        public virtual void ShouldThrowWhenDeletingNullHook()
        {
            var tx = new ContractUpdateTransaction();
            AssertThatThrownBy(() => tx.AddHookToDelete(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessageContaining("hookId cannot be null");
        }

        public virtual void ShouldThrowWhenDeletingNullHooks()
        {
            var tx = new ContractUpdateTransaction();
            AssertThatThrownBy(() => tx.SetHooksToDelete(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessageContaining("hookIds cannot be null");
        }

        public virtual void ShouldSerializeHooksInBuild()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            tx.AddHookToCreate(hookDetails);
            tx.AddHookToDelete(123);
            var builder = tx.Build();
            Assert.Single(builder.GetHookCreationDetailsList());
            Assert.Single(builder.GetHookIdsToDeleteList());
            Assert.Contains(builder.GetHookIdsToDeleteList(), 123);
        }

        public virtual void ShouldDeserializeHooksFromTransactionBody()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            tx.AddHookToCreate(hookDetails);
            tx.AddHookToDelete(123);
            var bytes = tx.ToBytes();
            var deserializedTx = (ContractUpdateTransaction)Transaction.FromBytes(bytes);
            Assert.Single(deserializedTx.GetHooksToCreate());
            Assert.Single(deserializedTx.GetHooksToDelete());
            Assert.Contains(deserializedTx.GetHooksToDelete(), 123);
        }

        public virtual void ShouldHandleEmptyHooks()
        {
            var tx = new ContractUpdateTransaction();
            Assert.Empty(tx.GetHooksToCreate());
            Assert.Empty(tx.GetHooksToDelete());
            var builder = tx.Build();
            Assert.Empty(builder.GetHookCreationDetailsList());
            Assert.Empty(builder.GetHookIdsToDeleteList());
        }

        public virtual void ShouldSupportMultipleHooks()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, lambdaHook);
            tx.AddHookToCreate(hookDetails1);
            tx.AddHookToCreate(hookDetails2);
            tx.AddHookToDelete(100);
            tx.AddHookToDelete(200);
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            AssertThat(tx.GetHooksToDelete()).ContainsExactlyInAnyOrder(100, 200);
        }
    }
}