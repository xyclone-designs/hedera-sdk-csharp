// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class ContractUpdateTransactionHooksIntegrationTest
    {
        private static readonly string SMART_CONTRACT_BYTECODE = "6080604052348015600e575f80fd5b50335f806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055506104a38061005b5f395ff3fe608060405260043610610033575f3560e01c8063607a4427146100375780637065cb4814610053578063893d20e81461007b575b5f80fd5b610051600480360381019061004c919061033c565b6100a5565b005b34801561005e575f80fd5b50610079600480360381019061007491906103a2565b610215565b005b348015610086575f80fd5b5061008f6102b7565b60405161009c91906103dc565b60405180910390f35b3373ffffffffffffffffffffffffffffffffffffffff165f8054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16146100fb575f80fd5b805f806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550600181908060018154018082558091505060019003905f5260205f20015f9091909190916101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055505f8173ffffffffffffffffffffffffffffffffffffffff166108fc3490811502906040515f60405180830381858888f19350505050905080610211576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004016102089061044f565b60405180910390fd5b5050565b805f806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550600181908060018154018082558091505060019003905f5260205f20015f9091909190916101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555050565b5f805f9054906101000a900473ffffffffffffffffffffffffffffffffffffffff16905090565b5f80fd5b5f73ffffffffffffffffffffffffffffffffffffffff82169050919050565b5f61030b826102e2565b9050919050565b61031b81610301565b8114610325575f80fd5b50565b5f8135905061033681610312565b92915050565b5f60208284031215610351576103506102de565b5b5f61035e84828501610328565b91505092915050565b5f610371826102e2565b9050919050565b61038181610367565b811461038b575f80fd5b50565b5f8135905061039c81610378565b92915050565b5f602082840312156103b7576103b66102de565b5b5f6103c48482850161038e565b91505092915050565b6103d681610367565b82525050565b5f6020820190506103ef5f8301846103cd565b92915050565b5f82825260208201905092915050565b7f5472616e73666572206661696c656400000000000000000000000000000000005f82015250565b5f610439600f836103f5565b915061044482610405565b602082019050919050565b5f6020820190508181035f8301526104668161042d565b905091905056fea26469706673582212206c46ddb2acdbcc4290e15be83eb90cd0b2ce5bd82b9bfe58a0709c5aec96305564736f6c634300081a0033";
        public virtual void ContractUpdateWithBasicLambdaHookSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                ContractId targetHookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var fileId = CreateBytecodeFile(testEnv);
                var createdContractId = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    BytecodeFileId = fileId,
                    Gas = 400000, 
                    InitialBalance = Hbar.FromTinybars(0) 
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ContractId;

                var lambdaHook = new EvmHook(targetHookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                var response = new ContractUpdateTransaction
                {
					ContractId = createdContractId,
					MaxTransactionFee = Hbar.From(20),
				}
                .AddHookToCreate(hookDetails)
                .Execute(testEnv.Client);
                var receipt = response.GetReceipt(testEnv.Client);

                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }
        public virtual void ContractUpdateWithDuplicateHookIdsInSameTransactionFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var fileId = CreateBytecodeFile(testEnv);
                var createdContractId = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    BytecodeFileId = fileId,
                    Gas = 400000, 
                    InitialBalance = Hbar.FromTinybars(0) 
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ContractId;

                ContractId targetHookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(targetHookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);

				PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
				{
					return new ContractUpdateTransaction
					{
						ContractId = createdContractId,
						MaxTransactionFee = Hbar.From(20),
						HooksToCreate = [hookDetails, hookDetails],

					}.Execute(testEnv.Client);
				
                }); Assert.Contains(ResponseStatus.HookIdRepeatedInCreationDetails.ToString(), exception.Message);
            }
        }
        public virtual void ContractUpdateWithExistingHookIdFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var fileId = CreateBytecodeFile(testEnv);
                var createdContractId = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    BytecodeFileId = fileId,
                    Gas = 400000, 
                    InitialBalance = Hbar.FromTinybars(0) 
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ContractId;

                ContractId targetHookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(targetHookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                
                new ContractUpdateTransaction
                {
					ContractId = createdContractId,
					MaxTransactionFee = Hbar.From(20),
				}
				.AddHookToCreate(hookDetails)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    var response = new ContractUpdateTransaction
                    {
						ContractId = createdContractId,
						MaxTransactionFee = Hbar.From(20),
					}
					.AddHookToCreate(hookDetails)
                    .Execute(testEnv.Client);

                    response.GetReceipt(testEnv.Client);

                }).Satisfies((e) => Assert.Equal(e.receipt.status, ResponseStatus.HookIdInUse));
            }
        }
        public virtual void ContractUpdateWithLambdaHookAndStorageUpdatesSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var fileId = CreateBytecodeFile(testEnv);
                var createdContractId = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    BytecodeFileId = fileId,
                    Gas = 400000, 
                    InitialBalance = Hbar.FromTinybars(0) 
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ContractId;

                ContractId targetHookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var storageSlot = new EvmHookStorageSlot(new byte[] { 0x01 }, new byte[] { 0x02 });
                var lambdaHook = new EvmHook(targetHookContractId, [storageSlot]);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                var response = new ContractUpdateTransaction
                {
					ContractId = createdContractId,
					MaxTransactionFee = Hbar.From(20),

				}.AddHookToCreate(hookDetails).Execute(testEnv.Client);
                var receipt = response.GetReceipt(testEnv.Client);

                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }
        public virtual void ContractUpdateWithHookIdAlreadyInUseFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var fileId = CreateBytecodeFile(testEnv);
                var createdContractId = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    BytecodeFileId = fileId,
                    Gas = 400000, 
                    InitialBalance = Hbar.FromTinybars(0) 
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ContractId;

                ContractId targetHookContractId1 = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook1 = new EvmHook(targetHookContractId1);
                var hookDetails1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook1);
                new ContractUpdateTransaction
                {
                    MaxTransactionFee = Hbar.From(20), 
                    ContractId = createdContractId
                }
                .AddHookToCreate(hookDetails1)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                ContractId targetHookContractId2 = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook2 = new EvmHook(targetHookContractId2);
                var hookDetails2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook2);

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    return new ContractUpdateTransaction()
                    {
						ContractId = createdContractId,
						MaxTransactionFee = Hbar.From(20),
					}
					.AddHookToCreate(hookDetails2)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

				}).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_ID_IN_USE));
            }
        }
        public virtual void ContractUpdateWithHookDeletionSucceeds()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var fileId = CreateBytecodeFile(testEnv);
                var createdContractId = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    BytecodeFileId = fileId,
                    Gas = 400000,
                    InitialBalance = Hbar.FromTinybars(0)
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ContractId;

                ContractId targetHookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(targetHookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                new ContractUpdateTransaction
                {
                    MaxTransactionFee = Hbar.From(20), 
                    ContractId = createdContractId
                }
                .AddHookToCreate(hookDetails)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var response = new ContractUpdateTransaction
                {
					ContractId = createdContractId,
					MaxTransactionFee = Hbar.From(20),
				}
                .AddHookToDelete(1)
                .Execute(testEnv.Client);

                var receipt = response.GetReceipt(testEnv.Client);

                Assert.Equal(receipt.status, ResponseStatus.Success);
            }
        }
        public virtual void ContractUpdateWithNonExistentHookIdDeletionFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var fileId = CreateBytecodeFile(testEnv);
                var createdContractId = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    BytecodeFileId = fileId,
                    Gas = 400000, 
                    InitialBalance = Hbar.FromTinybars(0) 
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ContractId;

                ContractId targetHookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(targetHookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                new ContractUpdateTransaction()
                {
					MaxTransactionFee = Hbar.From(20),
					ContractId = createdContractId,
				}
                .AddHookToCreate(hookDetails)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    var response = new ContractUpdateTransaction()
                    {
						ContractId = createdContractId,
						MaxTransactionFee = Hbar.From(20),
					}
                    .AddHookToDelete(123)
                    .Execute(testEnv.Client);

                    response.GetReceipt(testEnv.Client);

                }).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_NOT_FOUND));
            }
        }
        public virtual void ContractUpdateWithAddAndDeleteSameHookIdFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var fileId = CreateBytecodeFile(testEnv);
                var createdContractId = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    BytecodeFileId = fileId,
                    Gas = 400000, 
                    InitialBalance = Hbar.FromTinybars(0) 
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ContractId;

                ContractId targetHookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(targetHookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    var response = new ContractUpdateTransaction
                    {
						ContractId = createdContractId,
						MaxTransactionFee = Hbar.From(20),
						HooksToCreate = [hookDetails],
					}
                    .AddHookToDelete(1)
                    .Execute(testEnv.Client);

                    response.GetReceipt(testEnv.Client);

                }).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_NOT_FOUND));
            }
        }
        public virtual void ContractUpdateWithAlreadyDeletedHookFails()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var fileId = CreateBytecodeFile(testEnv);
                var createdContractId = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    BytecodeFileId = fileId,
                    Gas = 400000, 
                    InitialBalance = Hbar.FromTinybars(0) 
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).ContractId;

                ContractId targetHookContractId = EntityHelper.CreateContract(testEnv, testEnv.OperatorKey);
                var lambdaHook = new EvmHook(targetHookContractId);
                var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);

                // Add the hook
                new ContractUpdateTransaction
                {
                    MaxTransactionFee = Hbar.From(20), 
                    ContractId = createdContractId
                }
                .AddHookToCreate(hookDetails)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // First deletion - should succeed
                var firstDeleteResponse = new ContractUpdateTransaction
                {
					ContractId = createdContractId,
					MaxTransactionFee = Hbar.From(20),
				}
                .AddHookToDelete(1)
                .Execute(testEnv.Client);
                var firstDeleteReceipt = firstDeleteResponse.GetReceipt(testEnv.Client);
                Assert.Equal(firstDeleteReceipt.status, ResponseStatus.Success);

                // Second deletion - should fail with HOOK_DELETED
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    var response = new ContractUpdateTransaction
                    {
                        MaxTransactionFee = Hbar.From(20), 
                        ContractId = createdContractId
                    }
                    .AddHookToDelete(1)
                    .Execute(testEnv.Client);
                    response.GetReceipt(testEnv.Client);

                }).Satisfies((e) => Assert.Equal(e.receipt.status, Status.HOOK_NOT_FOUND));
            }
        }

        private FileId CreateBytecodeFile(IntegrationTestEnv testEnv)
        {
            var response = new FileCreateTransaction
            {
                Keys = [testEnv.OperatorKey],
                Contents = ByteString.CopyFromUtf8(SMART_CONTRACT_BYTECODE).ToByteArray()

            }.Execute(testEnv.Client);

            return response.GetReceipt(testEnv.Client).FileId;
        }
    }
}