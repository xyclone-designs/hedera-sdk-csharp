// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountUpdateTransactionHooksIntegrationTest
    {
        public virtual void AccountUpdateWithBasicLambdaHookSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account without hooks first
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // Deploy a simple contract to act as the EVM hook target
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);

                // Build a basic EVM hook (no admin key, no storage updates)
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);

                // Update the account to add the hook
                var response = new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToCreate(hookDetails).FreezeWith(testEnv.Client).Sign(accountKey);
                var receipt = response.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }

        public virtual void AccountUpdateWithDuplicateHookIdsInSameTransactionFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account without hooks first
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                Assert.Throws(typeof(PrecheckStatusException), () => new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToCreate(hookDetails1).AddHookToCreate(hookDetails2).FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client)).WithMessageContaining(Status.HOOK_ID_REPEATED_IN_CREATION_DETAILS.ToString());
            }
        }

        public virtual void AccountUpdateWithExistingHookIdFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId1 = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook1 = new EvmHook(hookContractId1);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook1);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails1).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // Try to add another hook with the same ID
                ContractId hookContractId2 = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook2 = new EvmHook(hookContractId2);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook2);
                Assert.Throws(typeof(ReceiptStatusException), () => new AccountUpdateTransaction().SetAccountId(accountId).AddHookToCreate(hookDetails2).SetMaxTransactionFee(Hbar.From(10)).FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_ID_IN_USE));
            }
        }

        public virtual void AccountUpdateWithLambdaHookAndStorageUpdatesSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account without hooks first
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var storageSlot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
                var mappingEntries = new EvmHookMappingEntries(new byte[] { 0x10 }, java.util.List.Of(EvmHookMappingEntry.OfKey(new byte[] { 0x11 }, new byte[] { 0x12 })));
                var lambdaHook = new EvmHook(hookContractId, java.util.List.Of(storageSlot, mappingEntries));
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);

                // Update the account to add the hook with storage updates
                var response = new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToCreate(hookDetails).FreezeWith(testEnv.Client).Sign(accountKey);
                var receipt = response.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }

        public virtual void AccountUpdateWithHookIdAlreadyInUseFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId1 = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook1 = new EvmHook(hookContractId1);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook1);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails1).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // Try to add another hook with the same ID (1L)
                ContractId hookContractId2 = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook2 = new EvmHook(hookContractId2);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook2);
                Assert.Throws(typeof(ReceiptStatusException), () => new AccountUpdateTransaction().SetAccountId(accountId).AddHookToCreate(hookDetails2).SetMaxTransactionFee(Hbar.From(10)).FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_ID_IN_USE));
            }
        }

        public virtual void AccountUpdateWithHookDeletionSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // Update the account to delete the hook
                var response = new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToDelete(1).FreezeWith(testEnv.Client).Sign(accountKey);
                var receipt = response.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }

        public virtual void AccountUpdateWithNonExistentHookIdDeletionFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // Try to delete a hook that doesn't exist (ID 999)
                Assert.Throws(typeof(ReceiptStatusException), () => new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToDelete(999).FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_NOT_FOUND));
            }
        }

        public virtual void AccountUpdateWithAddAndDeleteSameHookIdFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account without hooks first
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);

                // Try to add and delete the same hook ID in the same transaction
                Assert.Throws(typeof(ReceiptStatusException), () => new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(20)).AddHookToCreate(hookDetails).AddHookToDelete(1).FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_NOT_FOUND));
            }
        }

        public virtual void AccountUpdateWithAlreadyDeletedHookFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetMaxTransactionFee(Hbar.From(10)).SetInitialBalance(new Hbar(1)).AddHook(hookDetails).Execute(testEnv.Client).GetReceipt(testEnv.Client).AccountId;

                // First delete the hook
                var deleteResponse = new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToDelete(1).FreezeWith(testEnv.Client).Sign(accountKey);
                var deleteReceipt = deleteResponse.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                Assert.Equal(deleteReceipt.status, ResponseStatus.Success);

                // Try to delete the same hook again
                Assert.Throws(typeof(ReceiptStatusException), () => new AccountUpdateTransaction().SetAccountId(accountId).AddHookToDelete(1).SetMaxTransactionFee(Hbar.From(10)).FreezeWith(testEnv.Client).Sign(accountKey).Execute(testEnv.Client).GetReceipt(testEnv.Client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_NOT_FOUND));
            }
        }
    }
}