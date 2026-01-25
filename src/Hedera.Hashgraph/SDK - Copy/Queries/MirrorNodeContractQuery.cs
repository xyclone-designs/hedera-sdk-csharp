// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.EntityIdHelper;
using Com.Google.Gson;
using Google.Protobuf;
using Java.Util;
using Java.Util.Concurrent;
using Org.Bouncycastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// MirrorNodeContractQuery returns a result from EVM execution such as cost-free execution of read-only smart contract
    /// queries, gas estimation, and transient simulation of read-write operations.
    /// </summary>
    public abstract class MirrorNodeContractQuery<T>
        where T : MirrorNodeContractQuery<T>
    {
        // The contract we are sending the transaction to
        private ContractId contractId = null;
        private string contractEvmAddress = null;
        // The account we are sending the transaction from
        private AccountId sender = null;
        private string senderEvmAddress = null;
        // The transaction callData
        private byte[] callData;
        // The amount we are sending to the contract
        private long value;
        // The gas limit
        private long gasLimit;
        // The gas price
        private long gasPrice;
        // The block number for the simulation
        private long blockNumber;
        protected virtual T Self()
        {
            return (T)this;
        }

        public virtual ContractId GetContractId()
        {
            return contractId;
        }

        /// <summary>
        /// Sets the contract instance to call.
        /// </summary>
        /// <param name="contractId">The ContractId to be set</param>
        /// <returns>{@code this}</returns>
        public virtual T SetContractId(ContractId contractId)
        {
            Objects.RequireNonNull(contractId);
            contractId = contractId;
            return Self();
        }

        public virtual string GetContractEvmAddress()
        {
            return contractEvmAddress;
        }

        /// <summary>
        /// Set the 20-byte EVM address of the contract to call.
        /// </summary>
        /// <param name="contractEvmAddress"></param>
        /// <returns>{@code this}</returns>
        public virtual T SetContractEvmAddress(string contractEvmAddress)
        {
            Objects.RequireNonNull(contractEvmAddress);
            contractEvmAddress = contractEvmAddress;
            contractId = null;
            return Self();
        }

        public virtual AccountId GetSender()
        {
            return sender;
        }

        /// <summary>
        /// Sets the sender of the transaction simulation.
        /// </summary>
        /// <param name="sender">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public virtual T SetSender(AccountId sender)
        {
            Objects.RequireNonNull(sender);
            sender = sender;
            return Self();
        }

        public virtual string GetSenderEvmAddress()
        {
            return senderEvmAddress;
        }

        /// <summary>
        /// Set the 20-byte EVM address of the sender.
        /// </summary>
        /// <param name="senderEvmAddress"></param>
        /// <returns>{@code this}</returns>
        public virtual T SetSenderEvmAddress(string senderEvmAddress)
        {
            Objects.RequireNonNull(senderEvmAddress);
            senderEvmAddress = senderEvmAddress;
            sender = null;
            return Self();
        }

        public virtual byte[] GetCallData()
        {
            return callData;
        }

        /// <summary>
        /// Sets the function to call, and the parameters to pass to the function.
        /// </summary>
        /// <param name="name">The String to be set as the function name</param>
        /// <param name="params">The function parameters to be set</param>
        /// <returns>{@code this}</returns>
        public virtual T SetFunction(string name, ContractFunctionParameters @params)
        {
            Objects.RequireNonNull(@params);
            return SetFunctionParameters(@params.ToBytes(name));
        }

        /// <summary>
        /// Sets the function name to call.
        /// <p>
        /// The function will be called with no parameters. Use {@link #setFunction(String, ContractFunctionParameters)} to
        /// call a function with parameters.
        /// </summary>
        /// <param name="name">The String to be set as the function name</param>
        /// <returns>{@code this}</returns>
        public virtual T SetFunction(string name)
        {
            return SetFunction(name, new ContractFunctionParameters());
        }

        /// <summary>
        /// Sets the function parameters as their raw bytes.
        /// <p>
        /// Use this instead of {@link #setFunction(String, ContractFunctionParameters)} if you have already pre-encoded a
        /// solidity function call.
        /// </summary>
        /// <param name="functionParameters">The function parameters to be set</param>
        /// <returns>{@code this}</returns>
        public virtual T SetFunctionParameters(ByteString functionParameters)
        {
            Objects.RequireNonNull(functionParameters);
            callData = functionParameters.ToByteArray();
            return Self();
        }

        public virtual long GetValue()
        {
            return value;
        }

        /// <summary>
        /// Sets the amount of value (in tinybars or wei) to be sent to the contract in the transaction.
        /// <p>
        /// Use this to specify an amount for a payable function call.
        /// </summary>
        /// <param name="value">the amount of value to send, in tinybars or wei</param>
        /// <returns>{@code this}</returns>
        public virtual T SetValue(long value)
        {
            value = value;
            return Self();
        }

        public virtual long GetGasLimit()
        {
            return gasLimit;
        }

        /// <summary>
        /// Sets the gas limit for the contract call.
        /// <p>
        /// This specifies the maximum amount of gas that the transaction can consume.
        /// </summary>
        /// <param name="gasLimit">the maximum gas allowed for the transaction</param>
        /// <returns>{@code this}</returns>
        public virtual T SetGasLimit(long gasLimit)
        {
            gasLimit = gasLimit;
            return Self();
        }

        public virtual long GetGasPrice()
        {
            return gasPrice;
        }

        /// <summary>
        /// Sets the gas price to be used for the contract call.
        /// <p>
        /// This specifies the price of each unit of gas used in the transaction.
        /// </summary>
        /// <param name="gasPrice">the gas price, in tinybars or wei, for each unit of gas</param>
        /// <returns>{@code this}</returns>
        public virtual T SetGasPrice(long gasPrice)
        {
            gasPrice = gasPrice;
            return Self();
        }

        public virtual long GetBlockNumber()
        {
            return blockNumber;
        }

        /// <summary>
        /// Sets the block number for the simulation of the contract call.
        /// <p>
        /// The block number determines the context of the contract call simulation within the blockchain.
        /// </summary>
        /// <param name="blockNumber">the block number at which to simulate the contract call</param>
        /// <returns>{@code this}</returns>
        public virtual T SetBlockNumber(long blockNumber)
        {
            blockNumber = blockNumber;
            return Self();
        }

        /// <summary>
        /// Returns gas estimation for the EVM execution
        /// </summary>
        /// <param name="client"></param>
        /// <exception cref="ExecutionException"></exception>
        /// <exception cref="InterruptedException"></exception>
        protected virtual long Estimate(Client client)
        {
            FillEvmAddresses();
            return GetEstimateGasFromMirrorNodeAsync(client).Get();
        }

        /// <summary>
        /// Does transient simulation of read-write operations and returns the result in hexadecimal string format. The
        /// result can be any solidity type.
        /// </summary>
        /// <param name="client"></param>
        /// <exception cref="ExecutionException"></exception>
        /// <exception cref="InterruptedException"></exception>
        protected virtual string Call(Client client)
        {
            FillEvmAddresses();
            var blockNum = blockNumber == 0 ? "latest" : String.ValueOf(blockNumber);
            return GetContractCallResultFromMirrorNodeAsync(client, blockNum).Get();
        }

        private void FillEvmAddresses()
        {
            if (contractEvmAddress == null)
            {
                Objects.RequireNonNull(contractId);
                contractEvmAddress = contractId.ToEvmAddress();
            }

            if (senderEvmAddress == null && sender != null)
            {
                senderEvmAddress = sender.ToEvmAddress();
            }
        }

        private Task<string> GetContractCallResultFromMirrorNodeAsync(Client client, string blockNumber)
        {
            return ExecuteMirrorNodeRequest(client, blockNumber, false).ThenApply(MirrorNodeContractQuery.ParseContractCallResult());
        }

        private Task<long> GetEstimateGasFromMirrorNodeAsync(Client client)
        {
            return ExecuteMirrorNodeRequest(client, "latest", true).ThenApply(MirrorNodeContractQuery.ParseHexEstimateToLong());
        }

        private Task<string> ExecuteMirrorNodeRequest(Client client, string blockNumber, bool estimate)
        {
            string apiEndpoint = "/contracts/call";
            string jsonPayload = CreateJsonPayload(callData, senderEvmAddress, contractEvmAddress, gasLimit, gasPrice, value, blockNumber, estimate);
            string baseUrl = client.GetMirrorRestBaseUrl();

            // For localhost contract calls, override to use port 8545 unless system property overrides
            if (baseUrl.Contains("localhost:5551") || baseUrl.Contains("127.0.0.1:5551"))
            {
                string contractPort = System.GetProperty("hedera.mirror.contract.port");
                if (contractPort != null && !contractPort.IsEmpty())
                {
                    baseUrl = baseUrl.Replace(":5551", ":" + contractPort);
                }
                else
                {
                    baseUrl = baseUrl.Replace(":5551", ":8545");
                }
            }

            return PerformQueryToMirrorNodeAsync(baseUrl, apiEndpoint, jsonPayload).Exceptionally((ex) =>
            {
                client.GetLogger().Error("Error while performing post request to Mirror Node: " + ex.GetMessage());
                throw new CompletionException(ex);
            });
        }

        static string CreateJsonPayload(byte[] data, string senderAddress, string contractAddress, long gas, long gasPrice, long value, string blockNumber, bool estimate)
        {
            string hexData = Hex.ToHexString(data);
            JsonObject jsonObject = new JsonObject();
            jsonObject.AddProperty("data", hexData);
            jsonObject.AddProperty("to", contractAddress);
            jsonObject.AddProperty("estimate", estimate);
            jsonObject.AddProperty("blockNumber", blockNumber);

            // Conditionally add fields if they are set to non-default values
            if (senderAddress != null && !senderAddress.IsEmpty())
            {
                jsonObject.AddProperty("from", senderAddress);
            }

            if (gas > 0)
            {
                jsonObject.AddProperty("gas", gas);
            }

            if (gasPrice > 0)
            {
                jsonObject.AddProperty("gasPrice", gasPrice);
            }

            if (value > 0)
            {
                jsonObject.AddProperty("value", value);
            }

            return jsonObject.ToString();
        }

        static string ParseContractCallResult(string responseBody)
        {
            JsonObject jsonObject = JsonParser.ParseString(responseBody).GetAsJsonObject();
            return jsonObject["result"].GetAsString();
        }

        static long ParseHexEstimateToLong(string responseBody)
        {
            return int.Parse(ParseContractCallResult(responseBody).Substring(2), 16);
        }

        public override string ToString()
        {
            return "{" + "contractId=" + contractId + ", contractEvmAddress='" + contractEvmAddress + '\'' + ", sender=" + sender + ", senderEvmAddress='" + senderEvmAddress + '\'' + ", callData=" + Array.ToString(callData) + ", value=" + value + ", gasLimit=" + gasLimit + ", gasPrice=" + gasPrice + ", blockNumber=" + blockNumber + '}';
        }
    }
}