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
    class AccountCreateTransactionHooksIntegrationTest
    {
        public virtual void AccountCreateWithBasicLambdaHookSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Deploy a simple contract to act as the lambda hook target
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);

                // Build a basic EVM hook (no admin key, no storage updates)
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(PrivateKey.GenerateED25519()).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails).Execute(testEnv.client);
                var receipt = response.GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }

        public virtual void AccountCreateWithLambdaHookAndStorageUpdatesSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var storageSlot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
                var mappingEntries = new EvmHookMappingEntries(new byte[] { 0x10 }, java.util.List.Of(EvmHookMappingEntry.OfKey(new byte[] { 0x11 }, new byte[] { 0x12 })));
                var lambdaHook = new EvmHook(hookContractId, java.util.List.Of(storageSlot, mappingEntries));
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 2, lambdaHook);
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(PrivateKey.GenerateED25519()).SetInitialBalance(new Hbar(1)).AddHook(hookDetails).SetMaxTransactionFee(Hbar.From(10)).Execute(testEnv.client);
                var receipt = response.GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }

        public virtual void AccountCreateWithDuplicateHookIdsFailsPrecheck()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 4, lambdaHook);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 4, lambdaHook);
                Assert.Throws(typeof(PrecheckStatusException), () => new AccountCreateTransaction().SetKeyWithoutAlias(PrivateKey.GenerateED25519()).SetInitialBalance(new Hbar(1)).SetMaxTransactionFee(Hbar.From(10)).AddHook(hookDetails1).AddHook(hookDetails2).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.HOOK_ID_REPEATED_IN_CREATION_DETAILS.ToString());
            }
        }

        public virtual void AccountCreateWithLambdaHookAndAdminKeySucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var adminKey = PrivateKey.GenerateED25519();
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 5, lambdaHook, adminKey.GetPublicKey());
                var tx = new AccountCreateTransaction().SetKeyWithoutAlias(PrivateKey.GenerateED25519()).SetMaxTransactionFee(Hbar.From(10)).SetInitialBalance(new Hbar(1)).AddHook(hookDetails).FreezeWith(testEnv.client).Sign(adminKey);
                var receipt = tx.Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }
    }
}