// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Java.Util.Concurrent;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.File;
using System.Threading;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class MirrorNodeContractQueryIntegrationTest
    {
        private static readonly string SMART_CONTRACT_BYTECODE = "6080604052348015600e575f80fd5b50335f806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055506104a38061005b5f395ff3fe608060405260043610610033575f3560e01c8063607a4427146100375780637065cb4814610053578063893d20e81461007b575b5f80fd5b610051600480360381019061004c919061033c565b6100a5565b005b34801561005e575f80fd5b50610079600480360381019061007491906103a2565b610215565b005b348015610086575f80fd5b5061008f6102b7565b60405161009c91906103dc565b60405180910390f35b3373ffffffffffffffffffffffffffffffffffffffff165f8054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16146100fb575f80fd5b805f806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550600181908060018154018082558091505060019003905f5260205f20015f9091909190916101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055505f8173ffffffffffffffffffffffffffffffffffffffff166108fc3490811502906040515f60405180830381858888f19350505050905080610211576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004016102089061044f565b60405180910390fd5b5050565b805f806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550600181908060018154018082558091505060019003905f5260205f20015f9091909190916101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555050565b5f805f9054906101000a900473ffffffffffffffffffffffffffffffffffffffff16905090565b5f80fd5b5f73ffffffffffffffffffffffffffffffffffffffff82169050919050565b5f61030b826102e2565b9050919050565b61031b81610301565b8114610325575f80fd5b50565b5f8135905061033681610312565b92915050565b5f60208284031215610351576103506102de565b5b5f61035e84828501610328565b91505092915050565b5f610371826102e2565b9050919050565b61038181610367565b811461038b575f80fd5b50565b5f8135905061039c81610378565b92915050565b5f602082840312156103b7576103b66102de565b5b5f6103c48482850161038e565b91505092915050565b6103d681610367565b82525050565b5f6020820190506103ef5f8301846103cd565b92915050565b5f82825260208201905092915050565b7f5472616e73666572206661696c656400000000000000000000000000000000005f82015250565b5f610439600f836103f5565b915061044482610405565b602082019050919050565b5f6020820190508181035f8301526104668161042d565b905091905056fea26469706673582212206c46ddb2acdbcc4290e15be83eb90cd0b2ce5bd82b9bfe58a0709c5aec96305564736f6c634300081a0033";
        private static readonly string ADDRESS = "0x5B38Da6a701c568545dCfcB03FcB875f56beddC4";
        public virtual void CanSimulateTransaction()
        {

            // Clear any system properties to ensure clean state
            System.ClearProperty("hedera.mirror.contract.port");
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction()
                {
					Keys = testEnv.OperatorKey,
					Contents = SMART_CONTRACT_BYTECODE,
				}
                .Execute(testEnv.Client);
                var fileId = response.GetReceipt(testEnv.Client).FileId);
                response = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    Gas = 400000, 
                    BytecodeFileId = fileId
                
                }
                .Execute(testEnv.Client);
                var contractId = response.GetReceipt(testEnv.Client).ContractId;

                // Wait for mirror node to import data
                Thread.Sleep(5000);
                var gas = new MirrorNodeContractEstimateGasQuery()
                    .SetContractId(contractId)
                    .SetFunction("getOwner")
                .Execute(testEnv.Client);
                var result = new ContractCallQuery()
                    .SetContractId(contractId)
                    .SetGas(gas)
                    .SetFunction("getOwner")
                    .SetQueryPayment(new Hbar(1))
                .Execute(testEnv.Client);
                var simulationResult = new MirrorNodeContractCallQuery()
                    .SetContractId(contractId)
                    .SetFunction("getOwner")
                .Execute(testEnv.Client);
                Assert.Equal(result.GetAddress(0), simulationResult.Substring(26));
                gas = new MirrorNodeContractEstimateGasQuery()
                    .SetContractId(contractId)
                    .SetFunction("addOwner", new ContractFunctionParameters().AddAddress(ADDRESS))
                .Execute(testEnv.Client);
                new ContractExecuteTransaction()
                    .SetContractId(contractId)
                    .SetGas(gas)
                    .SetFunction("addOwner", new ContractFunctionParameters().AddAddress(ADDRESS))
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new MirrorNodeContractCallQuery()
                    .SetContractId(contractId)
                    .SetFunction("addOwner", new ContractFunctionParameters().AddAddress(ADDRESS))
                .Execute(testEnv.Client);
                new ContractDeleteTransaction()
                    .SetTransferAccountId(testEnv.OperatorId)
                    .SetContractId(contractId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new FileDeleteTransaction()
                    .SetFileId(fileId)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void ReturnsDefaultValuesWhenContractIsNotDeployed()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var defaultGas = 22892;
                var contractId = new ContractId(0, 0, 1231456);
                var gas = new MirrorNodeContractEstimateGasQuery()
                    .SetContractId(contractId)
                    .SetFunction("getOwner")
                .Execute(testEnv.Client);
                Assert.Equal(gas, defaultGas);
                var result = new MirrorNodeContractCallQuery()
                    .SetContractId(contractId)
                    .SetFunction("getOwner")
                .Execute(testEnv.Client);
                Assert.Equal(result, "0x");
            }
        }

        public virtual void FailsWhenGasLimitIsLow()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction()
                    .SetKeys(testEnv.OperatorKey)
                    .SetContents(SMART_CONTRACT_BYTECODE)
                .Execute(testEnv.Client);
                var fileId = response
                    .GetReceipt(testEnv.Client).FileId);
                response = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    Gas = 400000, 
                    BytecodeFileId = fileId
                
                }
            .Execute(testEnv.Client);
                var contractId = response
                    .GetReceipt(testEnv.Client).ContractId;

                // Wait for mirror node to import data
                Thread.Sleep(5000);
                Assert.Throws(typeof(ExecutionException), () =>
                {
                    new MirrorNodeContractEstimateGasQuery()
                    .SetContractId(contractId)
                    .SetGasLimit(100)
                    .SetFunction("addOwnerAndTransfer", new ContractFunctionParameters().AddAddress(ADDRESS))
                .Execute(testEnv.Client);
                }).WithMessageContaining("Received non-200 response from Mirror Node");
                Assert.Throws(typeof(ExecutionException), () =>
                {
                    new MirrorNodeContractCallQuery()
                    .SetContractId(contractId)
                    .SetGasLimit(100)
                    .SetFunction("addOwnerAndTransfer", new ContractFunctionParameters().AddAddress(ADDRESS))
                .Execute(testEnv.Client);
                }).WithMessageContaining("Received non-200 response from Mirror Node");
            }
        }

        public virtual void FailsWhenSenderIsNotSet()
        {

            // Set system property to use port 5551 for contract calls in this test
            System.SetProperty("hedera.mirror.contract.port", "5551");
            try
            {
                using (var testEnv = new IntegrationTestEnv(1))
                {
                    var response = new FileCreateTransaction()
                        .SetKeys(testEnv.OperatorKey)
                        .SetContents(SMART_CONTRACT_BYTECODE)
                    .Execute(testEnv.Client);
                    var fileId = response
                        .GetReceipt(testEnv.Client).FileId);
                    response = new ContractCreateTransaction
                    {
                        AdminKey = testEnv.OperatorKey,
                        Gas = 400000, 
                        BytecodeFileId = fileId
                    
                    }
                .Execute(testEnv.Client);
                    var contractId = response
                        .GetReceipt(testEnv.Client).ContractId;

                    // Wait for mirror node to import data
                    Thread.Sleep(5000);
                    Assert.Throws(typeof(ExecutionException), () =>
                    {
                        new MirrorNodeContractEstimateGasQuery()
                        .SetContractId(contractId)
                        .SetFunction("addOwnerAndTransfer", new ContractFunctionParameters().AddAddress(ADDRESS))
                    .Execute(testEnv.Client);
                    }).WithMessageContaining("Received non-200 response from Mirror Node");
                    Assert.Throws(typeof(ExecutionException), () =>
                    {
                        new MirrorNodeContractCallQuery()
                        .SetContractId(contractId)
                        .SetFunction("addOwnerAndTransfer", new ContractFunctionParameters().AddAddress(ADDRESS))
                    .Execute(testEnv.Client);
                    }).WithMessageContaining("Received non-200 response from Mirror Node");
                }
            }
            finally
            {

                // Clear the system property after the test
                System.ClearProperty("hedera.mirror.contract.port");
            }
        }

        public virtual void CanSimulateWithSenderSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction()
                    .SetKeys(testEnv.OperatorKey)
                    .SetContents(SMART_CONTRACT_BYTECODE)
                .Execute(testEnv.Client);
                var fileId = response
                    .GetReceipt(testEnv.Client).FileId);
                response = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    Gas = 400000, 
                    BytecodeFileId = fileId
                
                }
            .Execute(testEnv.Client);
                var contractId = response
                    .GetReceipt(testEnv.Client).ContractId;
                var receiverAccountId = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(PrivateKey.GenerateED25519())
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;

                // Wait for mirror node to import data
                Thread.Sleep(5000);
                var receiverEvmAddress = receiverAccountId.ToEvmAddress();
                var owner = new MirrorNodeContractCallQuery()
                    .SetContractId(contractId)
                    .SetFunction("getOwner")
                .Execute(testEnv.Client).Substring(26);
                var gas = new MirrorNodeContractEstimateGasQuery()
                    .SetContractId(contractId)
                    .SetGasLimit(1000000)
                    .SetFunction("addOwnerAndTransfer", new ContractFunctionParameters().AddAddress(receiverEvmAddress))
                    .SetSenderEvmAddress(owner)
                    .SetValue(123)
                .Execute(testEnv.Client);
                new ContractExecuteTransaction()
                    .SetContractId(contractId)
                    .SetGas(300000)
                    .SetPayableAmount(new Hbar(1))
                    .SetFunction("addOwnerAndTransfer", new ContractFunctionParameters().AddAddress(receiverEvmAddress))
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                new MirrorNodeContractCallQuery()
                    .SetContractId(contractId)
                    .SetGasLimit(1000000)
                    .SetFunction("addOwnerAndTransfer", new ContractFunctionParameters().AddAddress(receiverEvmAddress))
                    .SetSenderEvmAddress(owner)
                    .SetValue(123)
                .Execute(testEnv.Client);
            }
        }
    }
}