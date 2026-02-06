// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Ids;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <summary>
    /// MirrorNodeContractQuery returns a result from EVM execution such as cost-free execution of read-only smart Contract
    /// queries, Gas estimation, and transient simulation of read-write operations.
    /// </summary>
    public abstract class MirrorNodeContractQuery<T> where T : MirrorNodeContractQuery<T>
    {
		private static long ParseHexEstimateToLong(string responseBody)
		{
			return int.Parse(ParseContractCallResult(responseBody).Substring(2), NumberStyles.HexNumber);
		}
		private static string ParseContractCallResult(string responseBody)
		{
			return JsonObject.Parse(responseBody)?["result"].ToString();
		}
		private static string CreateJsonPayload(byte[] data, string senderaddress, string contractaddress, long gas, long gasprice, long value, string blocknumber, bool estimate)
		{
			string hexData = Convert.ToHexString(data).ToLowerInvariant();

			var jsonObject = new JsonObject
			{
				["data"] = hexData,
				["to"] = contractaddress,
				["estimate"] = estimate,
				["blockNumber"] = blocknumber
			};

			if (value > 0) jsonObject["value"] = value;
			if (gas > 0) jsonObject["gas"] = gas;
			if (gasprice > 0) jsonObject["gasPrice"] = gasprice;
			if (!string.IsNullOrEmpty(senderaddress) is false) jsonObject["from"] = senderaddress;

			return jsonObject.ToString();
		}

		/// <summary>
		/// Sets the Contract instance to call.
		/// </summary>
		/// <param name="ContractId">The ContractId to be set</param>
		/// <returns>{@code this}</returns>
		public virtual ContractId? ContractId
		{
			get;
			set;
		}
		/// <summary>
		/// Set the 20-byte EVM address of the Contract to call.
		/// </summary>
		/// <param name="ContractEvmAddress"></param>
		/// <returns>{@code this}</returns>
		public virtual string? ContractEvmAddress
		{
			get;
            set
            {
                field = value;
                ContractId = null;
            }
		}
		/// <summary>
		/// Sets the Sender of the transaction simulation.
		/// </summary>
		/// <param name="Sender">The AccountId to be set</param>
		/// <returns>{@code this}</returns>
		public virtual AccountId? Sender
		{
			get;
			set;
		}
		/// <summary>
		/// Set the 20-byte EVM address of the Sender.
		/// </summary>
		/// <param name="SenderEvmAddress"></param>
		/// <returns>{@code this}</returns>
		public virtual string? SenderEvmAddress
		{
			get;
            set
            {
                field = value;
                Sender = null;
            }
		}
		public virtual byte[] CallData
		{
			get;
			set;
		}
		/// <summary>
		/// Sets the amount of Value (in tinybars or wei) to be sent to the Contract in the transaction.
		/// <p>
		/// Use this to specify an amount for a payable function call.
		/// </summary>
		/// <param name="Value">the amount of Value to send, in tinybars or wei</param>
		/// <returns>{@code this}</returns>
		public virtual long Value 
        { 
            get;
            set;
        }
		/// <summary>
		/// Sets the Gas limit for the Contract call.
		/// <p>
		/// This specifies the maximum amount of Gas that the transaction can consume.
		/// </summary>
		/// <param name="GasLimit">the maximum Gas allowed for the transaction</param>
		/// <returns>{@code this}</returns>
		public virtual long GasLimit 
        { 
            get;
            set;
        }
		/// <summary>
		/// Sets the Gas price to be used for the Contract call.
		/// <p>
		/// This specifies the price of each unit of Gas used in the transaction.
		/// </summary>
		/// <param name="GasPrice">the Gas price, in tinybars or wei, for each unit of Gas</param>
		/// <returns>{@code this}</returns>
		public virtual long GasPrice 
        { 
            get;
            set;
        }
		/// <summary>
		/// Sets the block number for the simulation of the Contract call.
		/// <p>
		/// The block number determines the context of the Contract call simulation within the blockchain.
		/// </summary>
		/// <param name="BlockNumber">the block number at which to simulate the Contract call</param>
		/// <returns>{@code this}</returns>
		public virtual long BlockNumber 
        { 
            get;
            set;
        }

		private void FillEvmAddresses()
		{
			if (ContractEvmAddress == null)
			{
				ArgumentNullException.ThrowIfNull(ContractId);
				ContractEvmAddress = ContractId.ToEvmAddress();
			}

			if (SenderEvmAddress == null && Sender != null)
			{
				SenderEvmAddress = Sender.ToEvmAddress();
			}
		}
		private async Task<long> GetEstimateGasFromMirrorNodeAsync(Client client)
		{
			return ParseHexEstimateToLong(await ExecuteMirrorNodeRequest(client, "latest", true));
		}
		private Task<string> ExecuteMirrorNodeRequest(Client client, string blockNumber, bool estimate)
		{
			string apiEndpoint = "/Contracts/call";
			string jsonPayload = CreateJsonPayload(CallData, SenderEvmAddress, ContractEvmAddress, GasLimit, GasPrice, Value, blockNumber, estimate);
			string baseUrl = client.MirrorRestBaseUrl;

			// For localhost Contract calls, override to use port 8545 unless system property overrides
			if (baseUrl.Contains("localhost:5551") || baseUrl.Contains("127.0.0.1:5551"))
			{
				string ContractPort = System.GetProperty("hedera.mirror.Contract.port");
				if (ContractPort != null && ContractPort.Length != 0)
				{
					baseUrl = baseUrl.Replace(":5551", ":" + ContractPort);
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
		private async Task<string> GetContractCallResultFromMirrorNodeAsync(Client client, string blockNumber)
		{
			string _ = await  ExecuteMirrorNodeRequest(client, blockNumber, false);

			return ParseContractCallResult(_);
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
		/// Sets the function to call, and the parameters to pass to the function.
		/// </summary>
		/// <param name="name">The String to be set as the function name</param>
		/// <param name="params">The function parameters to be set</param>
		/// <returns>{@code this}</returns>
		public virtual T SetFunction(string name, ContractFunctionParameters @params)
        {
            ArgumentNullException.ThrowIfNull(@params);
            return SetFunctionParameters(@params.ToBytes(name));
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
            ArgumentNullException.ThrowIfNull(functionParameters);
            CallData = functionParameters.ToByteArray();
            return Self();
        }

		protected virtual T Self()
		{
			return (T)this;
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
			var blockNum = BlockNumber == 0 ? "latest" : BlockNumber.ToString();
			return GetContractCallResultFromMirrorNodeAsync(client, blockNum).GetAwaiter().GetResult();
		}
		/// <summary>
		/// Returns Gas estimation for the EVM execution
		/// </summary>
		/// <param name="client"></param>
		/// <exception cref="ExecutionException"></exception>
		/// <exception cref="InterruptedException"></exception>
		protected virtual long Estimate(Client client)
        {
            FillEvmAddresses();
            return GetEstimateGasFromMirrorNodeAsync(client).GetAwaiter().GetResult();
        }
    }
}