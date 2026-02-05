// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ContractUpdateTransactionHooksTest
    {
        virtual void ShouldAddHookToCreate()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            var result = tx.AddHookToCreate(hookDetails);
            AssertThat(result).IsSameAs(tx);
            AssertThat(tx.GetHooksToCreate()).HasSize(1);
            Assert.Equal(tx.GetHooksToCreate()[0], hookDetails);
        }

        virtual void ShouldSetHooksToCreate()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails1 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            var hookDetails2 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 2, lambdaHook);
            var hooks = List.Of(hookDetails1, hookDetails2);
            var result = tx.SetHooksToCreate(hooks);
            AssertThat(result).IsSameAs(tx);
            AssertThat(tx.GetHooksToCreate()).HasSize(2);
            AssertThat(tx.GetHooksToCreate()).ContainsExactlyInAnyOrder(hookDetails1, hookDetails2);
        }

        virtual void ShouldAddHookToDelete()
        {
            var tx = new ContractUpdateTransaction();
            var hookId = 123;
            var result = tx.AddHookToDelete(hookId);
            AssertThat(result).IsSameAs(tx);
            AssertThat(tx.GetHooksToDelete()).HasSize(1);
            AssertThat(tx.GetHooksToDelete()).Contains(hookId);
        }

        virtual void ShouldAddHooksToDelete()
        {
            var tx = new ContractUpdateTransaction();
            var hookIds = List.Of(123, 456, 789);
            var result = tx.SetHooksToDelete(hookIds);
            AssertThat(result).IsSameAs(tx);
            AssertThat(tx.GetHooksToDelete()).HasSize(3);
            AssertThat(tx.GetHooksToDelete()).ContainsExactlyInAnyOrder(123, 456, 789);
        }

        virtual void ShouldGetHooksToCreate()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            tx.AddHookToCreate(hookDetails);
            var result = tx.GetHooksToCreate();
            AssertThat(result).HasSize(1);
            Assert.Equal(result[0], hookDetails);

            // Verify it returns a copy
            result.Clear();
            AssertThat(tx.GetHooksToCreate()).HasSize(1);
        }

        virtual void ShouldGetHooksToDelete()
        {
            var tx = new ContractUpdateTransaction();
            tx.AddHookToDelete(123);
            var result = tx.GetHooksToDelete();
            AssertThat(result).HasSize(1);
            AssertThat(result).Contains(123);

            // Verify it returns a copy
            result.Clear();
            AssertThat(tx.GetHooksToDelete()).HasSize(1);
        }

        virtual void ShouldThrowWhenAddingHookAfterFreeze()
        {
            var tx = new ContractUpdateTransaction();
            tx.SetNodeAccountIds(java.util.Arrays.AsList(AccountId.FromString("0.0.5005")));
            tx.SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), java.time.Instant.OfEpochSecond(1554158542)));
            tx.Freeze();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            AssertThatThrownBy(() => tx.AddHookToCreate(hookDetails)).IsInstanceOf(typeof(InvalidOperationException)).HasMessageContaining("transaction is immutable");
        }

        virtual void ShouldThrowWhenSettingHooksAfterFreeze()
        {
            var tx = new ContractUpdateTransaction();
            tx.SetNodeAccountIds(java.util.Arrays.AsList(AccountId.FromString("0.0.5005")));
            tx.SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), java.time.Instant.OfEpochSecond(1554158542)));
            tx.Freeze();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            AssertThatThrownBy(() => tx.SetHooksToCreate(List.Of(hookDetails))).IsInstanceOf(typeof(InvalidOperationException)).HasMessageContaining("transaction is immutable");
        }

        virtual void ShouldThrowWhenDeletingHookAfterFreeze()
        {
            var tx = new ContractUpdateTransaction();
            tx.SetNodeAccountIds(java.util.Arrays.AsList(AccountId.FromString("0.0.5005")));
            tx.SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), java.time.Instant.OfEpochSecond(1554158542)));
            tx.Freeze();
            AssertThatThrownBy(() => tx.AddHookToDelete(123)).IsInstanceOf(typeof(InvalidOperationException)).HasMessageContaining("transaction is immutable");
        }

        virtual void ShouldThrowWhenDeletingHooksAfterFreeze()
        {
            var tx = new ContractUpdateTransaction();
            tx.SetNodeAccountIds(java.util.Arrays.AsList(AccountId.FromString("0.0.5005")));
            tx.SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), java.time.Instant.OfEpochSecond(1554158542)));
            tx.Freeze();
            AssertThatThrownBy(() => tx.SetHooksToDelete(List.Of(123, 456))).IsInstanceOf(typeof(InvalidOperationException)).HasMessageContaining("transaction is immutable");
        }

        virtual void ShouldThrowWhenAddingNullHook()
        {
            var tx = new ContractUpdateTransaction();
            AssertThatThrownBy(() => tx.AddHookToCreate(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessageContaining("hookDetails cannot be null");
        }

        virtual void ShouldThrowWhenSettingNullHooks()
        {
            var tx = new ContractUpdateTransaction();
            AssertThatThrownBy(() => tx.SetHooksToCreate(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessageContaining("hookDetails cannot be null");
        }

        virtual void ShouldThrowWhenDeletingNullHook()
        {
            var tx = new ContractUpdateTransaction();
            AssertThatThrownBy(() => tx.AddHookToDelete(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessageContaining("hookId cannot be null");
        }

        virtual void ShouldThrowWhenDeletingNullHooks()
        {
            var tx = new ContractUpdateTransaction();
            AssertThatThrownBy(() => tx.SetHooksToDelete(null)).IsInstanceOf(typeof(NullReferenceException)).HasMessageContaining("hookIds cannot be null");
        }

        virtual void ShouldSerializeHooksInBuild()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            tx.AddHookToCreate(hookDetails);
            tx.AddHookToDelete(123);
            var builder = tx.Build();
            AssertThat(builder.GetHookCreationDetailsList()).HasSize(1);
            AssertThat(builder.GetHookIdsToDeleteList()).HasSize(1);
            AssertThat(builder.GetHookIdsToDeleteList()).Contains(123);
        }

        virtual void ShouldDeserializeHooksFromTransactionBody()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            tx.AddHookToCreate(hookDetails);
            tx.AddHookToDelete(123);
            var bytes = tx.ToBytes();
            var deserializedTx = (ContractUpdateTransaction)Transaction.FromBytes(bytes);
            AssertThat(deserializedTx.GetHooksToCreate()).HasSize(1);
            AssertThat(deserializedTx.GetHooksToDelete()).HasSize(1);
            AssertThat(deserializedTx.GetHooksToDelete()).Contains(123);
        }

        virtual void ShouldHandleEmptyHooks()
        {
            var tx = new ContractUpdateTransaction();
            Assert.Empty(tx.GetHooksToCreate());
            Assert.Empty(tx.GetHooksToDelete());
            var builder = tx.Build();
            Assert.Empty(builder.GetHookCreationDetailsList());
            Assert.Empty(builder.GetHookIdsToDeleteList());
        }

        virtual void ShouldSupportMultipleHooks()
        {
            var tx = new ContractUpdateTransaction();
            var contractId = new ContractId(0, 0, 1);
            var lambdaHook = new EvmHook(contractId);
            var hookDetails1 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
            var hookDetails2 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 2, lambdaHook);
            tx.AddHookToCreate(hookDetails1);
            tx.AddHookToCreate(hookDetails2);
            tx.AddHookToDelete(100);
            tx.AddHookToDelete(200);
            AssertThat(tx.GetHooksToCreate()).HasSize(2);
            AssertThat(tx.GetHooksToDelete()).HasSize(2);
            AssertThat(tx.GetHooksToDelete()).ContainsExactlyInAnyOrder(100, 200);
        }
    }
}