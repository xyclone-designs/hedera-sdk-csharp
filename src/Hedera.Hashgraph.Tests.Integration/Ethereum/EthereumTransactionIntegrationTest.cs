// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Esaulpaugh.Headlong.Rlp;
using Com.Esaulpaugh.Headlong.Util;
using Com.Hedera.Hashgraph.Sdk;
using Java.Math;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class EthereumTransactionIntegrationTest
    {
        private static readonly string SMART_CONTRACT_BYTECODE = "608060405234801561001057600080fd5b506040516104d73803806104d78339818101604052602081101561003357600080fd5b810190808051604051939291908464010000000082111561005357600080fd5b90830190602082018581111561006857600080fd5b825164010000000081118282018810171561008257600080fd5b82525081516020918201929091019080838360005b838110156100af578181015183820152602001610097565b50505050905090810190601f1680156100dc5780820380516001836020036101000a031916815260200191505b506040525050600080546001600160a01b0319163317905550805161010890600190602084019061010f565b50506101aa565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061015057805160ff191683800117855561017d565b8280016001018555821561017d579182015b8281111561017d578251825591602001919060010190610162565b5061018992915061018d565b5090565b6101a791905b808211156101895760008155600101610193565b90565b61031e806101b96000396000f3fe608060405234801561001057600080fd5b50600436106100415760003560e01c8063368b87721461004657806341c0e1b5146100ee578063ce6d41de146100f6575b600080fd5b6100ec6004803603602081101561005c57600080fd5b81019060208101813564010000000081111561007757600080fd5b82018360208201111561008957600080fd5b803590602001918460018302840111640100000000831117156100ab57600080fd5b91908080601f016020809104026020016040519081016040528093929190818152602001838380828437600092019190915250929550610173945050505050565b005b6100ec6101a2565b6100fe6101ba565b6040805160208082528351818301528351919283929083019185019080838360005b83811015610138578181015183820152602001610120565b50505050905090810190601f1680156101655780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6000546001600160a01b0316331461018a5761019f565b805161019d906001906020840190610250565b505b50565b6000546001600160a01b03163314156101b85733ff5b565b60018054604080516020601f600260001961010087891615020190951694909404938401819004810282018101909252828152606093909290918301828280156102455780601f1061021a57610100808354040283529160200191610245565b820191906000526020600020905b81548152906001019060200180831161022857829003601f168201915b505050505090505b90565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061029157805160ff19168380011785556102be565b828001600101855582156102be579182015b828111156102be5782518255916020019190600101906102a3565b506102ca9291506102ce565b5090565b61024d91905b808211156102ca57600081556001016102d456fea264697066735822122084964d4c3f6bc912a9d20e14e449721012d625aa3c8a12de41ae5519752fc89064736f6c63430006000033";
        private static readonly string SMART_CONTRACT_BYTECODE_JUMBO = "6080604052348015600e575f5ffd5b506101828061001c5f395ff3fe608060405234801561000f575f5ffd5b5060043610610029575f3560e01c80631e0a3f051461002d575b5f5ffd5b610047600480360381019061004291906100d0565b61005d565b6040516100549190610133565b60405180910390f35b5f5f905092915050565b5f5ffd5b5f5ffd5b5f5ffd5b5f5ffd5b5f5ffd5b5f5f83601f8401126100905761008f61006f565b5b8235905067ffffffffffffffff8111156100ad576100ac610073565b5b6020830191508360018202830111156100c9576100c8610077565b5b9250929050565b5f5f602083850312156100e6576100e5610067565b5b5f83013567ffffffffffffffff8111156101035761010261006b565b5b61010f8582860161007b565b92509250509250929050565b5f819050919050565b61012d8161011b565b82525050565b5f6020820190506101465f830184610124565b9291505056fea26469706673582212202829ebd1cf38c443e4fd3770cd4306ac4c6bb9ac2828074ae2b9cd16121fcfea64736f6c634300081e0033";
        public virtual void SignerNonceChangedOnEthereumTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var newAccountAliasId = privateKey.ToAccountId(0, 0);
                new TransferTransaction().AddHbarTransfer(testEnv.operatorId, new Hbar(1).Negated()).AddHbarTransfer(newAccountAliasId, new Hbar(1)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var fileCreateTransactionResponse = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents(SMART_CONTRACT_BYTECODE).Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(fileCreateTransactionResponse.GetReceipt(testEnv.client).fileId);
                var contractCreateTransactionResponse = new ContractCreateTransaction().SetAdminKey(testEnv.operatorKey).SetGas(400000).SetConstructorParameters(new ContractFunctionParameters().AddString("Hello from Hedera.")).SetBytecodeFileId(fileId).SetContractMemo("[e2e::ContractCreateTransaction]").Execute(testEnv.client);
                var contractId = Objects.RequireNonNull(contractCreateTransactionResponse.GetReceipt(testEnv.client).contractId);
                int nonce = 0;
                byte[] chainId = Hex.Decode("012a");
                byte[] maxPriorityGas = Hex.Decode("00");
                byte[] maxGas = Hex.Decode("d1385c7bf0");
                byte[] to = Hex.Decode(contractId.ToEvmAddress());
                byte[] callData = new ContractExecuteTransaction().SetFunction("setMessage", new ContractFunctionParameters().AddString("new message")).GetFunctionParameters().ToByteArray();
                var sequence = RLPEncoder.Sequence(Integers.ToBytes(2), new object[] { chainId, Integers.ToBytes(nonce), maxPriorityGas, maxGas, Integers.ToBytes(150000), to, Integers.ToBytesUnsigned(BigInteger.ZERO), callData, new object[0] });
                byte[] signedBytes = privateKey.Sign(sequence);

                // wrap in signature object
                byte[] r = new byte[32];
                Array.Copy(signedBytes, 0, r, 0, 32);
                byte[] s = new byte[32];
                Array.Copy(signedBytes, 32, s, 0, 32);
                int recId = ((PrivateKeyECDSA)privateKey).GetRecoveryId(r, s, sequence);
                byte[] ethereumData = RLPEncoder.Sequence(Integers.ToBytes(0x02), List.Of(chainId, Integers.ToBytes(nonce), maxPriorityGas, maxGas, Integers.ToBytes(150000), to, Integers.ToBytesUnsigned(BigInteger.ZERO), callData, List.Of(), Integers.ToBytes(recId), r, s));
                EthereumTransaction ethereumTransaction = new EthereumTransaction().SetEthereumData(ethereumData);
                var ethereumTransactionResponse = ethereumTransaction.Execute(testEnv.client);
                var ethereumTransactionRecord = ethereumTransactionResponse.GetRecord(testEnv.client);
                Assert.Equal(ethereumTransactionRecord.contractFunctionResult.signerNonce, 1);
                new ContractDeleteTransaction().SetTransferAccountId(testEnv.operatorId).SetContractId(contractId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void JumboEthereumTransactionWithLargeCalldata()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var newAccountAliasId = privateKey.ToAccountId(0, 0);
                new TransferTransaction().AddHbarTransfer(testEnv.operatorId, new Hbar(100).Negated()).AddHbarTransfer(newAccountAliasId, new Hbar(100)).Execute(testEnv.client).GetReceipt(testEnv.client);
                var fileCreateTransactionResponse = new FileCreateTransaction().SetKeys(testEnv.operatorKey).SetContents(SMART_CONTRACT_BYTECODE_JUMBO).Execute(testEnv.client);
                var fileId = Objects.RequireNonNull(fileCreateTransactionResponse.GetReceipt(testEnv.client).fileId);
                var contractCreateTransactionResponse = new ContractCreateTransaction().SetAdminKey(testEnv.operatorKey).SetGas(300000).SetBytecodeFileId(fileId).SetContractMemo("[e2e::ContractCreateTransaction]").Execute(testEnv.client);
                var contractId = Objects.RequireNonNull(contractCreateTransactionResponse.GetReceipt(testEnv.client).contractId);
                byte[] largeCalldata = new byte[1024 * 120];
                var callData = new ContractExecuteTransaction().SetFunction("consumeLargeCalldata", new ContractFunctionParameters().AddBytes(largeCalldata)).GetFunctionParameters().ToByteArray();
                int nonce = 0;
                byte[] chainId = Hex.Decode("012a");
                byte[] maxPriorityGas = Hex.Decode("00");
                byte[] maxGas = Hex.Decode("d1385c7bf0");
                byte[] gasLimitBytes = Hex.Decode("3567E0");
                byte[] to = Hex.Decode(contractId.ToEvmAddress());
                byte[] value = Hex.Decode("00");
                var sequence = RLPEncoder.Sequence(Integers.ToBytes(2), new object[] { chainId, Integers.ToBytes(nonce), maxPriorityGas, maxGas, gasLimitBytes, to, Integers.ToBytesUnsigned(BigInteger.ZERO), callData, new object[0] });
                byte[] signedBytes = privateKey.Sign(sequence);

                // wrap in signature object
                byte[] r = new byte[32];
                Array.Copy(signedBytes, 0, r, 0, 32);
                byte[] s = new byte[32];
                Array.Copy(signedBytes, 32, s, 0, 32);
                int recId = ((PrivateKeyECDSA)privateKey).GetRecoveryId(r, s, sequence);
                byte[] ethereumData = RLPEncoder.Sequence(Integers.ToBytes(0x02), List.Of(chainId, Integers.ToBytes(nonce), maxPriorityGas, maxGas, gasLimitBytes, to, Integers.ToBytesUnsigned(BigInteger.ZERO), callData, List.Of(), Integers.ToBytes(recId), r, s));
                EthereumTransaction ethereumTransaction = new EthereumTransaction().SetEthereumData(ethereumData);
                var ethereumTransactionResponse = ethereumTransaction.Execute(testEnv.client);
                var ethereumTransactionRecord = ethereumTransactionResponse.GetRecord(testEnv.client);
                Assert.Equal(ethereumTransactionRecord.contractFunctionResult.signerNonce, 1);
                new ContractDeleteTransaction().SetTransferAccountId(testEnv.operatorId).SetContractId(contractId).Execute(testEnv.client).GetReceipt(testEnv.client);
                new FileDeleteTransaction().SetFileId(fileId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }
    }
}