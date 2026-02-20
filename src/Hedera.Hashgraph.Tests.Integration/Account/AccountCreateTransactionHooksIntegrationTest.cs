// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountCreateTransactionHooksIntegrationTest
    {
        public virtual void AccountCreateWithBasicLambdaHookSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Deploy a simple contract to act as the lambda hook target
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);

                // Build a basic EVM hook (no admin key, no storage updates)
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                var response = new AccountCreateTransaction
                {
                    InitialBalance = new Hbar(1),
                    MaxTransactionFee = Hbar.From(10),
					Key = PrivateKey.GenerateED25519(),
				}
                .AddHook(hookDetails)
                .Execute(testEnv.Client);

                var receipt = response.GetReceipt(testEnv.Client);

                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }
        public virtual void AccountCreateWithLambdaHookAndStorageUpdatesSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var storageSlot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
                var mappingEntries = new EvmHookMappingEntries(new byte[] { 0x10 }, [EvmHookMappingEntry.OfKey(new byte[] { 0x11 }, new byte[] { 0x12 })]);
                var lambdaHook = new EvmHook(hookContractId, [storageSlot, mappingEntries]);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, lambdaHook);
                var response = new AccountCreateTransaction
                {
                    MaxTransactionFee = Hbar.From(10),
                    InitialBalance = new Hbar(1),
                    Key = PrivateKey.GenerateED25519(),
                }
                .AddHook(hookDetails)
                .Execute(testEnv.Client);

                var receipt = response.GetReceipt(testEnv.Client);

                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }
        public virtual void AccountCreateWithDuplicateHookIdsFailsPrecheck()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 4, lambdaHook);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 4, lambdaHook);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new AccountCreateTransaction
                    {
                        InitialBalance = new Hbar(1),
                        MaxTransactionFee = Hbar.From(10),
                        Key = PrivateKey.GenerateED25519(),
                    }
                    .AddHook(hookDetails1)
                    .AddHook(hookDetails2)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

				}); Assert.Contains(ResponseStatus.HookIdRepeatedInCreationDetails.ToString(), exception.Message);
            }
        }
        public virtual void AccountCreateWithLambdaHookAndAdminKeySucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var adminKey = PrivateKey.GenerateED25519();
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 5, lambdaHook, adminKey.GetPublicKey());
                var tx = new AccountCreateTransaction
                {
					MaxTransactionFee = Hbar.From(10),
					InitialBalance = new Hbar(1),
                    Key = PrivateKey.GenerateED25519(),
                }
                .AddHook(hookDetails)
                .FreezeWith(testEnv.Client)
                .Sign(adminKey);

                var receipt = tx.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }
    }
}