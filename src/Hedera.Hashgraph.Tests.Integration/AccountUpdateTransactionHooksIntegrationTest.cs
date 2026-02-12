// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountUpdateTransactionHooksIntegrationTest
    {
        virtual void AccountUpdateWithBasicLambdaHookSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account without hooks first
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Deploy a simple contract to act as the EVM hook target
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);

                // Build a basic EVM hook (no admin key, no storage updates)
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);

                // Update the account to add the hook
                var response = new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToCreate(hookDetails).FreezeWith(testEnv.client).Sign(accountKey);
                var receipt = response.Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }

        virtual void AccountUpdateWithDuplicateHookIdsInSameTransactionFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account without hooks first
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() => new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToCreate(hookDetails1).AddHookToCreate(hookDetails2).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.HOOK_ID_REPEATED_IN_CREATION_DETAILS.ToString());
            }
        }

        virtual void AccountUpdateWithExistingHookIdFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId1 = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook1 = new EvmHook(hookContractId1);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook1);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails1).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Try to add another hook with the same ID
                ContractId hookContractId2 = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook2 = new EvmHook(hookContractId2);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook2);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new AccountUpdateTransaction().SetAccountId(accountId).AddHookToCreate(hookDetails2).SetMaxTransactionFee(Hbar.From(10)).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_ID_IN_USE));
            }
        }

        virtual void AccountUpdateWithLambdaHookAndStorageUpdatesSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account without hooks first
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var storageSlot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
                var mappingEntries = new EvmHookMappingEntries(new byte[] { 0x10 }, java.util.List.Of(EvmHookMappingEntry.OfKey(new byte[] { 0x11 }, new byte[] { 0x12 })));
                var lambdaHook = new EvmHook(hookContractId, java.util.List.Of(storageSlot, mappingEntries));
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);

                // Update the account to add the hook with storage updates
                var response = new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToCreate(hookDetails).FreezeWith(testEnv.client).Sign(accountKey);
                var receipt = response.Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }

        virtual void AccountUpdateWithHookIdAlreadyInUseFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId1 = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook1 = new EvmHook(hookContractId1);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook1);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails1).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Try to add another hook with the same ID (1L)
                ContractId hookContractId2 = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook2 = new EvmHook(hookContractId2);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook2);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new AccountUpdateTransaction().SetAccountId(accountId).AddHookToCreate(hookDetails2).SetMaxTransactionFee(Hbar.From(10)).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_ID_IN_USE));
            }
        }

        virtual void AccountUpdateWithHookDeletionSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Update the account to delete the hook
                var response = new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToDelete(1).FreezeWith(testEnv.client).Sign(accountKey);
                var receipt = response.Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }

        virtual void AccountUpdateWithNonExistentHookIdDeletionFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Try to delete a hook that doesn't exist (ID 999)
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToDelete(999).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_NOT_FOUND));
            }
        }

        virtual void AccountUpdateWithAddAndDeleteSameHookIdFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account without hooks first
                var accountKey = PrivateKey.GenerateED25519();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetInitialBalance(new Hbar(1)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);

                // Try to add and delete the same hook ID in the same transaction
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(20)).AddHookToCreate(hookDetails).AddHookToDelete(1).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_NOT_FOUND));
            }
        }

        virtual void AccountUpdateWithAlreadyDeletedHookFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create an account with a hook first
                var accountKey = PrivateKey.GenerateED25519();
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(accountKey).SetMaxTransactionFee(Hbar.From(10)).SetInitialBalance(new Hbar(1)).AddHook(hookDetails).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // First delete the hook
                var deleteResponse = new AccountUpdateTransaction().SetAccountId(accountId).SetMaxTransactionFee(Hbar.From(10)).AddHookToDelete(1).FreezeWith(testEnv.client).Sign(accountKey);
                var deleteReceipt = deleteResponse.Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(deleteReceipt.status, Status.SUCCESS);

                // Try to delete the same hook again
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => new AccountUpdateTransaction().SetAccountId(accountId).AddHookToDelete(1).SetMaxTransactionFee(Hbar.From(10)).FreezeWith(testEnv.client).Sign(accountKey).Execute(testEnv.client).GetReceipt(testEnv.client)).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_NOT_FOUND));
            }
        }
    }
}