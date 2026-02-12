// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class ContractCreateTransactionHooksIntegrationTest
    {
        // Shared bytecode used to create a simple contract for these tests
        private static readonly string SMART_CONTRACT_BYTECODE = "6080604052348015600e575f80fd5b50335f806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055506104a38061005b5f395ff3fe608060405260043610610033575f3560e01c8063607a4427146100375780637065cb4814610053578063893d20e81461007b575b5f80fd5b610051600480360381019061004c919061033c565b6100a5565b005b34801561005e575f80fd5b50610079600480360381019061007491906103a2565b610215565b005b348015610086575f80fd5b5061008f6102b7565b60405161009c91906103dc565b60405180910390f35b3373ffffffffffffffffffffffffffffffffffffffff165f8054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16146100fb575f80fd5b805f806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550600181908060018154018082558091505060019003905f5260205f20015f9091909190916101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055505f8173ffffffffffffffffffffffffffffffffffffffff166108fc3490811502906040515f60405180830381858888f19350505050905080610211576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004016102089061044f565b60405180910390fd5b5050565b805f806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550600181908060018154018082558091505060019003905f5260205f20015f9091909190916101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555050565b5f805f9054906101000a900473ffffffffffffffffffffffffffffffffffffffff16905090565b5f80fd5b5f73ffffffffffffffffffffffffffffffffffffffff82169050919050565b5f61030b826102e2565b9050919050565b61031b81610301565b8114610325575f80fd5b50565b5f8135905061033681610312565b92915050565b5f60208284031215610351576103506102de565b5b5f61035e84828501610328565b91505092915050565b5f610371826102e2565b9050919050565b61038181610367565b811461038b575f80fd5b50565b5f8135905061039c81610378565b92915050565b5f602082840312156103b7576103b66102de565b5b5f6103c48482850161038e565b91505092915050565b6103d681610367565b82525050565b5f6020820190506103ef5f8301846103cd565b92915050565b5f82825260208201905092915050565b7f5472616e73666572206661696c656400000000000000000000000000000000005f82015250565b5f610439600f836103f5565b915061044482610405565b602082019050919050565b5f6020820190508181035f8301526104668161042d565b905091905056fea26469706673582212206c46ddb2acdbcc4290e15be83eb90cd0b2ce5bd82b9bfe58a0709c5aec96305564736f6c634300081a0033";
        virtual void ContractCreateWithBasicLambdaHookSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Deploy a simple contract to act as the EVM hook target
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var fileId = CreateBytecodeFile(testEnv);

                // Build a basic EVM hook (no admin key, no storage updates)
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook);
                var response = new ContractCreateTransaction().SetAdminKey(testEnv.operatorKey).SetGas(400000).SetBytecodeFileId(fileId).AddHook(hookDetails).Execute(testEnv.client);
                var receipt = response.GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
                AssertThat(receipt.contractId).IsNotNull();
            }
        }

        virtual void ContractCreateWithLambdaHookAndStorageUpdatesSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var fileId = CreateBytecodeFile(testEnv);
                var storageSlot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
                var mappingEntries = new EvmHookMappingEntries(new byte[] { 0x10 }, List.Of(EvmHookMappingEntry.OfKey(new byte[] { 0x11 }, new byte[] { 0x12 })));
                var lambdaHook = new EvmHook(hookContractId, List.Of(storageSlot, mappingEntries));
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 2, lambdaHook);
                var response = new ContractCreateTransaction().SetAdminKey(testEnv.operatorKey).SetGas(400000).SetBytecodeFileId(fileId).AddHook(hookDetails).Execute(testEnv.client);
                var receipt = response.GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
                AssertThat(receipt.contractId).IsNotNull();
            }
        }

        virtual void ContractCreateWithDuplicateHookIdsFailsPrecheck()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var fileId = CreateBytecodeFile(testEnv);
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 4, lambdaHook);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 4, lambdaHook);
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() => new ContractCreateTransaction().SetAdminKey(testEnv.operatorKey).SetGas(400000).SetBytecodeFileId(fileId).AddHook(hookDetails1).AddHook(hookDetails2).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.HOOK_ID_REPEATED_IN_CREATION_DETAILS.ToString());
            }
        }

        virtual void ContractCreateWithLambdaHookAndAdminKeySucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId hookContractId = EntityHelper.CreateContract(testEnv, testEnv.operatorKey);
                var fileId = CreateBytecodeFile(testEnv);
                var adminKey = PrivateKey.GenerateED25519();
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 5, lambdaHook, adminKey.GetPublicKey());
                var tx = new ContractCreateTransaction().SetAdminKey(testEnv.operatorKey).SetGas(400000).SetBytecodeFileId(fileId).AddHook(hookDetails).FreezeWith(testEnv.client).Sign(adminKey);
                var receipt = tx.Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
                AssertThat(receipt.contractId).IsNotNull();
            }
        }

        private FileId CreateBytecodeFile(IntegrationTestEnv testEnv)
        {
            var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents(SMART_CONTRACT_BYTECODE).Execute(testEnv.client);
            return Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
        }
    }
}