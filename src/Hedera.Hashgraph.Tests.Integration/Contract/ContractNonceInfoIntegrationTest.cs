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
    public class ContractNonceInfoIntegrationTest
    {
        private static readonly string SMART_CONTRACT_BYTECODE = "6080604052348015600f57600080fd5b50604051601a90603b565b604051809103906000f0801580156035573d6000803e3d6000fd5b50506047565b605c8061009483390190565b603f806100556000396000f3fe6080604052600080fdfea2646970667358221220a20122cbad3457fedcc0600363d6e895f17048f5caa4afdab9e655123737567d64736f6c634300081200336080604052348015600f57600080fd5b50603f80601d6000396000f3fe6080604052600080fdfea264697066735822122053dfd8835e3dc6fedfb8b4806460b9b7163f8a7248bac510c6d6808d9da9d6d364736f6c63430008120033";
        public virtual void CanIncrementNonceThroughContractConstructor()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents(SMART_CONTRACT_BYTECODE).Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).fileId);
                response = new ContractCreateTransaction().SetAdminKey(testEnv.operatorKey).SetGas(300000).SetBytecodeFileId(fileId).SetContractMemo("[e2e::ContractADeploysContractBInConstructor]").Execute(testEnv.client);
                var contractFunctionResult = response.GetRecord(testEnv.client).contractFunctionResult;
                ContractId contractA = contractFunctionResult.contractId;
                ContractId contractB = contractFunctionResult.contractNonces.Stream().Filter((contractNonce) => !contractNonce.contractId.Equals(contractA)).FindFirst().Get().contractId;
                ContractNonceInfo contractANonceInfo = contractFunctionResult.contractNonces.Stream().Filter((contractNonce) => contractNonce.contractId.Equals(contractA)).FindFirst().Get();
                ContractNonceInfo contractBNonceInfo = contractFunctionResult.contractNonces.Stream().Filter((contractNonce) => contractNonce.contractId.Equals(contractB)).FindFirst().Get();

                // A.nonce = 2
                Assert.Equal(contractANonceInfo.nonce, 2);

                // B.nonce = 1
                Assert.Equal(contractBNonceInfo.nonce, 1);

                // validate HIP-844 case - signer nonce should be set only for Ethereum transactions
                Assert.Equal(contractFunctionResult.signerNonce, 0);
                var contractId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).contractId);
                new ContractDeleteTransaction().SetTransferAccountId(testEnv.operatorId).SetContractId(contractId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }
    }
}