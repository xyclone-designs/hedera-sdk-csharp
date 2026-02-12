// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class HookStoreTransactionIntegrationTest
    {
        private static readonly string SMART_CONTRACT_BYTECODE = "6080604052348015600e575f5ffd5b506107d18061001c5f395ff3fe608060405260043610610033575f3560e01c8063124d8b301461003757806394112e2f14610067578063bd0dd0b614610097575b5f5ffd5b610051600480360381019061004c91906106f2565b6100c7565b60405161005e9190610782565b60405180910390f35b610081600480360381019061007c91906106f2565b6100d2565b60405161008e9190610782565b60405180910390f35b6100b160048036038101906100ac91906106f2565b6100dd565b6040516100be9190610782565b60405180910390f35b5f6001905092915050565b5f6001905092915050565b5f6001905092915050565b5f604051905090565b5f5ffd5b5f5ffd5b5f5ffd5b5f60a08284031215610112576101116100f9565b5b81905092915050565b5f5ffd5b5f601f19601f8301169050919050565b7f4e487b71000000000000000000000000000000000000000000000000000000005f52604160045260245ffd5b6101658261011f565b810181811067ffffffffffffffff821117156101845761018361012f565b5b80604052505050565b5f6101966100e8565b90506101a2828261015c565b919050565b5f5ffd5b5f5ffd5b5f67ffffffffffffffff8211156101c9576101c861012f565b5b602082029050602081019050919050565b5f5ffd5b5f73ffffffffffffffffffffffffffffffffffffffff82169050919050565b5f610207826101de565b9050919050565b610217816101fd565b8114610221575f5ffd5b50565b5f813590506102328161020e565b92915050565b5f8160070b9050919050565b61024d81610238565b8114610257575f5ffd5b50565b5f8135905061026881610244565b92915050565b5f604082840312156102835761028261011b565b5b61028d604061018d565b90505f61029c84828501610224565b5f8301525060206102af8482850161025a565b60208301525092915050565b5f6102cd6102c8846101af565b61018d565b905080838252602082019050604084028301858111156102f0576102ef6101da565b5b835b818110156103195780610305888261026e565b8452602084019350506040810190506102f2565b5050509392505050565b5f82601f830112610337576103366101ab565b5b81356103478482602086016102bb565b91505092915050565b5f67ffffffffffffffff82111561036a5761036961012f565b5b602082029050602081019050919050565b5f67ffffffffffffffff8211156103955761039461012f565b5b602082029050602081019050919050565b5f606082840312156103bb576103ba61011b565b5b6103c5606061018d565b90505f6103d484828501610224565b5f8301525060206103e784828501610224565b60208301525060406103fb8482850161025a565b60408301525092915050565b5f6104196104148461037b565b61018d565b9050808382526020820190506060840283018581111561043c5761043b6101da565b5b835b81811015610465578061045188826103a6565b84526020840193505060608101905061043e565b5050509392505050565b5f82601f830112610483576104826101ab565b5b8135610493848260208601610407565b91505092915050565b5f606082840312156104b1576104b061011b565b5b6104bb606061018d565b90505f6104ca84828501610224565b5f83015250602082013567ffffffffffffffff8111156104ed576104ec6101a7565b5b6104f984828501610323565b602083015250604082013567ffffffffffffffff81111561051d5761051c6101a7565b5b6105298482850161046f565b60408301525092915050565b5f61054761054284610350565b61018d565b9050808382526020820190506020840283018581111561056a576105696101da565b5b835b818110156105b157803567ffffffffffffffff81111561058f5761058e6101ab565b5b80860161059c898261049c565b8552602085019450505060208101905061056c565b5050509392505050565b5f82601f8301126105cf576105ce6101ab565b5b81356105df848260208601610535565b91505092915050565b5f604082840312156105fd576105fc61011b565b5b610607604061018d565b90505f82013567ffffffffffffffff811115610626576106256101a7565b5b61063284828501610323565b5f83015250602082013567ffffffffffffffff811115610655576106546101a7565b5b610661848285016105bb565b60208301525092915050565b5f604082840312156106825761068161011b565b5b61068c604061018d565b90505f82013567ffffffffffffffff8111156106ab576106aa6101a7565b5b6106b7848285016105e8565b5f83015250602082013567ffffffffffffffff8111156106da576106d96101a7565b5b6106e6848285016105e8565b60208301525092915050565b5f5f60408385031215610708576107076100f1565b5b5f83013567ffffffffffffffff811115610725576107246100f5565b5b610731858286016100fd565b925050602083013567ffffffffffffffff811115610752576107516100f5565b5b61075e8582860161066d565b9150509250929050565b5f8115159050919050565b61077c81610768565b82525050565b5f6020820190506107955f830184610773565b9291505056fea26469706673582212207dfe7723f6d6869419b1cb0619758b439da0cf4ffd9520997c40a3946299d4dc64736f6c634300081e0033";
        virtual void LambdaSStoreUpdatesStorageWithValidSignature()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Deploy hook contract
                var hookContractId = CreateContractId(testEnv);

                // Create an account that will own the hook and sign updates
                var adminKey = PrivateKey.GenerateED25519();
                var ownerId = new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Attach a lambda hook (id 3) to the owner with optional initial storage
                var initialSlot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x01 });
                var lambdaHook = new EvmHook(hookContractId, List.Of(initialSlot));
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 3, lambdaHook, adminKey.GetPublicKey());
                new AccountUpdateTransaction().SetAccountId(ownerId).AddHookToCreate(hookDetails).SetMaxTransactionFee(Hbar.From(10)).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // Build full HookId for the lambda owned by ownerId with id 3
                var hookId = new HookId(new HookEntityId(ownerId), 3);

                // Prepare a storage update to set key=0x01 -> value=0x02
                var update = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
                var resp = new HookStoreTransaction().SetNodeAccountIds(new List(testEnv.client.GetNetwork().Values())).SetHookId(hookId).AddStorageUpdate(update).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client);
                var receipt = resp.GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }

        virtual void LambdaSStoreFailsWithoutProperSignature()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Deploy hook contract
                var hookContractId = CreateContractId(testEnv);

                // Create an account that will own the hook
                var adminKey = PrivateKey.GenerateED25519();
                var ownerId = new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Attach a lambda hook (id 3) to the owner
                var initialSlot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x01 });
                var lambdaHook = new EvmHook(hookContractId, List.Of(initialSlot));
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 3, lambdaHook, adminKey.GetPublicKey());
                new AccountUpdateTransaction().SetAccountId(ownerId).SetMaxTransactionFee(Hbar.From(10)).AddHookToCreate(hookDetails).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // Build full HookId for the lambda owned by ownerId with id 3
                var hookId = new HookId(new HookEntityId(ownerId), 3);

                // Prepare a storage update
                var update = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });

                // Create a different key that doesn't have permission to update the hook
                var unauthorizedKey = PrivateKey.GenerateED25519();
                var tx = new HookStoreTransaction().SetNodeAccountIds(new List(testEnv.client.GetNetwork().Values())).SetHookId(hookId).AddStorageUpdate(update).FreezeWith(testEnv.client);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => tx.Sign(unauthorizedKey).Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void LambdaSStoreFailsWithNonExistentHookId()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Create a signer account (no hook actually created)
                var signerKey = PrivateKey.GenerateED25519();
                var signerId = new AccountCreateTransaction().SetKeyWithoutAlias(signerKey).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Build a HookId that does not exist (id 9999 for the signer)
                var hookId = new HookId(new HookEntityId(signerId), 9999);
                var update = new EvmHookStorageSlot(new byte[] { 0x0A }, new byte[] { 0x0B });
                var tx = new HookStoreTransaction().SetNodeAccountIds(new List(testEnv.client.GetNetwork().Values())).SetHookId(hookId).AddStorageUpdate(update).FreezeWith(testEnv.client).Sign(signerKey);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => tx.Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.HOOK_NOT_FOUND.ToString());
            }
        }

        virtual void LambdaSStoreTooManyStorageUpdatesFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // Deploy hook contract
                var hookContractId = CreateContractId(testEnv);

                // Create an account that will own the hook and sign updates
                var adminKey = PrivateKey.GenerateED25519();
                var ownerId = new AccountCreateTransaction().SetKeyWithoutAlias(adminKey).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;

                // Attach a lambda hook (id 7) to the owner
                var lambdaHook = new EvmHook(hookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, lambdaHook, adminKey.GetPublicKey());
                new AccountUpdateTransaction().SetAccountId(ownerId).AddHookToCreate(hookDetails).SetMaxTransactionFee(Hbar.From(10)).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // Build full HookId
                var hookId = new HookId(new HookEntityId(ownerId), 1);

                // Create 256 identical storage slots
                var slot = new EvmHookStorageSlot(new byte[] { 0x01, 0x02, 0x03, 0x04 }, new byte[] { 0x05, 0x06, 0x07, 0x08 });
                var updates = new List<EvmHookStorageUpdate>(256);
                for (int i = 0; i < 256; i++)
                {
                    updates.Add(slot);
                }

                var tx = new HookStoreTransaction().SetNodeAccountIds(new List(testEnv.client.GetNetwork().Values())).SetHookId(hookId).SetStorageUpdates(updates).FreezeWith(testEnv.client).Sign(adminKey);

                // Expect TOO_MANY_LAMBDA_STORAGE_UPDATES
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() => tx.Execute(testEnv.client).GetReceipt(testEnv.client)).WithMessageContaining(Status.TOO_MANY_LAMBDA_STORAGE_UPDATES.ToString());
            }
        }

        private FileId CreateBytecodeFile(IntegrationTestEnv testEnv)
        {
            var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents(SMART_CONTRACT_BYTECODE).Execute(testEnv.client);
            return Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
        }

        private ContractId CreateContractId(IntegrationTestEnv testEnv)
        {
            var fileId = CreateBytecodeFile(testEnv);
            var response = new ContractCreateTransaction().SetAdminKey(testEnv.operatorKey).SetGas(1000000).SetBytecodeFileId(fileId).Execute(testEnv.client);
            var receipt = response.GetReceipt(testEnv.client);
            return receipt.contractId;
        }
    }
}