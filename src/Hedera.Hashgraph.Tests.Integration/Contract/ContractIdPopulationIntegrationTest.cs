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
    class ContractIdPopulationIntegrationTest
    {
        public virtual void CanPopulateContractIdNumSync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var testContractByteCode = "608060405234801561001057600080fd5b50336000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055506101cb806100606000396000f3fe608060405260043610610046576000357c03000000000000000000000000000000000000000000000000000000009004806341c0e1b51461004b578063cfae321714610062575b600080fd5b34801561005757600080fd5b506100606100f2565b005b34801561006e57600080fd5b50610077610162565b6040518080602001828103825283818151815260200191508051906020019080838360005b838110156100b757808201518184015260208101905061009c565b50505050905090810190601f1680156100e45780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff161415610160573373ffffffffffffffffffffffffffffffffffffffff16ff5b565b60606040805190810160405280600d81526020017f48656c6c6f2c20776f726c64230000000000000000000000000000000000000081525090509056fea165627a7a72305820ae96fb3af7cde9c0abfe365272441894ab717f816f07f41f07b1cbede54e256e0029";
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents(testContractByteCode).Execute(testEnv.Client);
                var receipt = response.SetValidateStatus(true).GetReceipt(testEnv.Client);
                var fileId = receipt.fileId);
                response = new ContractCreateTransaction()AdminKey = testEnv.OperatorKey,.SetGas(300000).SetConstructorParameters(new ContractFunctionParameters().AddString("Hello from Hedera.")).SetBytecodeFileId(fileId).SetContractMemo("[e2e::canPopulateContractIdNum]").Execute(testEnv.Client);
                receipt = response.SetValidateStatus(true).GetReceipt(testEnv.Client);
                var contractId = receipt.ContractId);
                var info = new ContractInfoQuery().SetContractId(contractId).Execute(testEnv.Client);
                var idMirror = ContractId.FromEvmAddress(0, 0, info.contractAccountId);
                Thread.Sleep(5000);
                var newContractId = idMirror.PopulateContractNum(testEnv.Client);
                Assert.Equal(contractId.Num, newContractId.num);
            }
        }

        public virtual void CanPopulateContractIdNumAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var testContractByteCode = "608060405234801561001057600080fd5b50336000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055506101cb806100606000396000f3fe608060405260043610610046576000357c03000000000000000000000000000000000000000000000000000000009004806341c0e1b51461004b578063cfae321714610062575b600080fd5b34801561005757600080fd5b506100606100f2565b005b34801561006e57600080fd5b50610077610162565b6040518080602001828103825283818151815260200191508051906020019080838360005b838110156100b757808201518184015260208101905061009c565b50505050905090810190601f1680156100e45780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff161415610160573373ffffffffffffffffffffffffffffffffffffffff16ff5b565b60606040805190810160405280600d81526020017f48656c6c6f2c20776f726c64230000000000000000000000000000000000000081525090509056fea165627a7a72305820ae96fb3af7cde9c0abfe365272441894ab717f816f07f41f07b1cbede54e256e0029";
                var response = new FileCreateTransaction().SetKeys(testEnv.OperatorKey).SetContents(testContractByteCode).Execute(testEnv.Client);
                var receipt = response.SetValidateStatus(true).GetReceipt(testEnv.Client);
                var fileId = receipt.fileId);
                response = new ContractCreateTransaction()AdminKey = testEnv.OperatorKey,.SetGas(300000).SetConstructorParameters(new ContractFunctionParameters().AddString("Hello from Hedera.")).SetBytecodeFileId(fileId).SetContractMemo("[e2e::canPopulateContractIdNum]").Execute(testEnv.Client);
                receipt = response.SetValidateStatus(true).GetReceipt(testEnv.Client);
                var contractId = receipt.ContractId);
                var info = new ContractInfoQuery().SetContractId(contractId).Execute(testEnv.Client);
                var idMirror = ContractId.FromEvmAddress(0, 0, info.contractAccountId);
                Thread.Sleep(5000);
                var newContractId = idMirror.PopulateContractNumAsync(testEnv.Client).Get();
                Assert.Equal(contractId.Num, newContractId.num);
            }
        }
    }
}