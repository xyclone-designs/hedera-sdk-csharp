// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// ContractHelper declutters PrecompileExample.java
    ///
    /// When we instantiate a ContractHelper, we provide it with the JSON of a compiled solidity contract
    /// which is assumed to have functions named "step0()" through "stepN()".
    ///
    /// Each of these step functions is assumed to take no function parameters, and to return a Hedera ResponseCode
    /// which ought to be SUCCESS -- in other words, an int32 with value 22.
    /// See resources/org/hiero/sdk/java/examples/contracts/precompile/HederaResponseCodes.sol
    ///
    /// If a step takes function parameters, or if its ContractFunctionResult should be validated with a different method,
    /// the user can specify a supplier for a particular step with setParameterSupplier(stepIndex, parametersSupplier),
    /// and can specify an alternative validation method with setResultValidator(stepIndex, validateFunction)
    ///
    /// The contract is created on the Hedera network in the ContractHelper constructor, and when the user is ready to
    /// execute the step functions in the contract, they should call executeSteps(firstStepToExecute, lastStepToExecute).
    /// </summary>
    public class ContractHelper
    {
        public readonly ContractId ContractId;
        public readonly Dictionary<int, Func<ContractFunctionResult, bool>> StepResultValidators = [];
        public readonly Dictionary<int, Func<ContractFunctionParameters>> StepParameterSuppliers = [];
        public readonly Dictionary<int, Hbar> StepPayableAmounts = [];
        public readonly Dictionary<int, IList<PrivateKey>> StepSigners = [];
        public readonly Dictionary<int, AccountId> StepFeePayers = [];
        public readonly Dictionary<int, Action<string>> StepLogic = [];

        public static string GetBytecodeHex(string filename)
        {
            using Stream stream = File.OpenRead(filename);

            if (JsonNode.Parse(stream) is not JsonNode json)
                throw new ArgumentException($"'{filename}' could not be parsed to json");

            JsonNode bytecode = json["object"] ?? json["bytecode"] ?? throw new Exception("No bytecode or object found in json.");

            return bytecode.ToString();
        }

        public ContractHelper(string filename, ContractFunctionParameters constructorParameters, Client client)
        {
            ContractId = new ContractCreateFlow
            {
                Bytecode = GetBytecodeHex(filename),
                MaxChunks = 30,
                Gas = 8000000,
                ConstructorParameters = constructorParameters.ToBytes(null).ToByteArray(),
            }
            .Execute(client)
            .GetReceipt(client)
            .ContractId;
        }

        private Hbar GetPayableAmount(int stepIndex)
        {
            return StepPayableAmounts[stepIndex];
        }
        private IList<PrivateKey> GetSigners(int stepIndex)
        {
            return StepSigners.TryGetValue(stepIndex, out IList<PrivateKey>? result) ? result ?? [] : [];
        }
        private Func<ContractFunctionResult, bool> GetResultValidator(int stepIndex)
        {
            return StepResultValidators.TryGetValue(stepIndex, out Func<ContractFunctionResult, bool>? result) ? result : (contractFunctionResult) =>
            {
                ResponseStatus responseStatus = (ResponseStatus)contractFunctionResult.GetInt32(0);
                bool isValid = responseStatus == ResponseStatus.Success;
                if (!isValid)
                {
                    Console.WriteLine("Encountered invalid response status " + responseStatus);
                }

                return isValid;
            };
        }
        private Func<ContractFunctionParameters>? GetParameterSupplier(int stepIndex)
        {
            return StepParameterSuppliers.TryGetValue(stepIndex, out Func<ContractFunctionParameters>? result) ? result : null;
        }

        public virtual ContractHelper AddSignerForStep(int stepIndex, PrivateKey signer)
        {
            if (StepSigners.ContainsKey(stepIndex))
            {
                StepSigners[stepIndex].Add(signer);
            }
            else
            {
                IList<PrivateKey> signerList = [signer];
                StepSigners.Add(stepIndex, signerList);
            }

            return this;
        }
        public virtual ContractHelper ExecuteSteps(int firstStepToExecute, int lastStepToExecute, Client client)
        {
            for (int stepIndex = firstStepToExecute; stepIndex <= lastStepToExecute; stepIndex++)
            {
                Console.WriteLine("Attempting to execute step " + stepIndex);
                ContractExecuteTransaction tx = new()
                {
                    ContractId = ContractId,
                    Gas = 10000000,
                };

                if (GetPayableAmount(stepIndex) is Hbar payableAmount)
                {
                    tx.PayableAmount = payableAmount;
                }

                string functionName = "step" + stepIndex;
                ContractFunctionParameters? parameters = GetParameterSupplier(stepIndex)?.Invoke();

                if (parameters != null)
                    tx.SetFunction(functionName, parameters);
                else tx.SetFunction(functionName);

                if (StepFeePayers[stepIndex] != null)
                    tx.TransactionId = TransactionId.Generate(StepFeePayers[stepIndex]);

                tx.FreezeWith(client);

                foreach (PrivateKey signer in GetSigners(stepIndex))
                {
                    tx.Sign(signer);
                }

                TransactionRecord record = tx.Execute(client, client =>
                {
                    client.ValidateStatus = false;

                }).GetRecord(client);

                try
                {
                    if (record.Receipt.Status != ResponseStatus.Success)
                    {
                        throw new Exception("transaction receipt yielded unsuccessful response code " + record.Receipt.Status);
                    }

                    if (record.ContractFunctionResult is null)
                        throw new ArgumentNullException(nameof(record.ContractFunctionResult));

                    Console.WriteLine("gas used: " + record.ContractFunctionResult.GasUsed);

                    StepLogic[stepIndex]?.Invoke(record.ContractFunctionResult.GetAddress(1));

                    if (GetResultValidator(stepIndex).Invoke(record.ContractFunctionResult))
                    {
                        Console.WriteLine("step " + stepIndex + " completed, and returned valid result. (TransactionId \"" + record.TransactionId + "\")");
                    }
                    else
                    {
                        throw new Exception("returned invalid result");
                    }
                }
                catch (Exception error)
                {
                    throw new Exception("Error occurred in step " + stepIndex + ": " + error.Message + "\n" + "Transaction record: " + record);
                }

                // otherwise will meet local-node throttle
                Thread.Sleep(500);
            }

            return this;
        }
        public virtual ContractHelper SetResultValidatorForStep(int stepIndex, Func<ContractFunctionResult, bool> validator)
        {
            StepResultValidators.Add(stepIndex, validator);
            return this;
        }
        public virtual ContractHelper SetParameterSupplierForStep(int stepIndex, Func<ContractFunctionParameters> supplier)
        {
            StepParameterSuppliers.Add(stepIndex, supplier);
            return this;
        }
        public virtual ContractHelper SetPayableAmountForStep(int stepIndex, Hbar amount)
        {
            StepPayableAmounts.Add(stepIndex, amount);
            return this;
        }
        public virtual ContractHelper SetFeePayerForStep(int stepIndex, AccountId feePayerAccount, PrivateKey feePayerKey)
        {
            StepFeePayers.Add(stepIndex, feePayerAccount);
            return AddSignerForStep(stepIndex, feePayerKey);
        }
        public virtual ContractHelper SetStepLogic(int stepIndex, Action<string> stepLogic)
        {
            this.StepLogic.Add(stepIndex, stepLogic);
            return this;
        }
    }
}