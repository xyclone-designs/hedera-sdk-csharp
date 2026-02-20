// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.File;

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
                var response = new FileCreateTransaction
                {
					Keys = [testEnv.OperatorKey],
					Contents = Encoding.UTF8.GetBytes(SMART_CONTRACT_BYTECODE),
				}
                .Execute(testEnv.Client);

                var fileId = response.GetReceipt(testEnv.Client).FileId;
                response = new ContractCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    Gas = 300000,
                    BytecodeFileId = fileId,
                    ContractMemo = "[e2e::ContractADeploysContractBInConstructor]"
                
                }.Execute(testEnv.Client);

                var contractFunctionResult = response.GetRecord(testEnv.Client).ContractFunctionResult;
                ContractId contractA = contractFunctionResult.contractId;
                ContractId contractB = contractFunctionResult.ContractNonces.Where((contractNonce) => !contractNonce.ContractId.Equals(contractA)).First().ContractId;
                ContractNonceInfo contractANonceInfo = contractFunctionResult.ContractNonces.Where((contractNonce) => contractNonce.ContractId.Equals(contractA)).First();
                ContractNonceInfo contractBNonceInfo = contractFunctionResult.ContractNonces.Where((contractNonce) => contractNonce.ContractId.Equals(contractB)).First();

                // A.nonce = 2
                Assert.Equal(contractANonceInfo.Nonce, 2);

                // B.nonce = 1
                Assert.Equal(contractBNonceInfo.Nonce, 1);

                // validate HIP-844 case - signer nonce should be set only for Ethereum transactions
                Assert.Equal(contractFunctionResult.SignerNonce, 0);
                
                var contractId = response.GetReceipt(testEnv.Client).ContractId;
                
                new ContractDeleteTransaction
                {
					TransferAccountId = testEnv.OperatorId,
					ContractId = contractId

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                new FileDeleteTransaction
                {
                    FileId = fileId

                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }
    }
}