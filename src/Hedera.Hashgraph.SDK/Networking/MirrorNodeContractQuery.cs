// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Utils;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Networking
{
	public abstract class MirrorNodeContractQuery
	{
		public static long ParseHexEstimateToLong(string responseBody)
		{
			return int.Parse(ParseContractCallResult(responseBody).Substring(2), NumberStyles.HexNumber);
		}
		public static string ParseContractCallResult(string responseBody)
		{
			return JsonObject.Parse(responseBody)?["result"].ToString();
		}
		public static string CreateJsonPayload(byte[] data, string senderaddress, string contractaddress, long gas, long gasprice, long value, string blocknumber, bool estimate)
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

		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="T:Unknown"]/*' />
		public virtual ContractId? ContractId
		{
			get;
			set;
		}
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="T:Unknown_2"]/*' />
		public virtual string? ContractEvmAddress
		{
			get;
			set
			{
				field = value;
				ContractId = null;
			}
		}
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="T:Unknown_3"]/*' />
		public virtual AccountId? Sender
		{
			get;
			set;
		}
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="T:Unknown_4"]/*' />
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
		} = [];
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="T:Unknown_5"]/*' />
		public virtual long Value
		{
			get;
			set;
		}
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="T:Unknown_6"]/*' />
		public virtual long GasLimit
		{
			get;
			set;
		}
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="T:Unknown_7"]/*' />
		public virtual long GasPrice
		{
			get;
			set;
		}
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="M:FillEvmAddresses"]/*' />
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
		private async Task<string> ExecuteMirrorNodeRequest(Client client, string blockNumber, bool estimate)
		{
			string apiEndpoint = "/contracts/call";
			string jsonPayload = CreateJsonPayload(
				CallData,
				SenderEvmAddress,
				ContractEvmAddress,
				GasLimit,
				GasPrice,
				Value,
				blockNumber,
				estimate);

			string baseUrl = client.MirrorRestBaseUrl;

			// For localhost contract calls, override to use port 8545 unless environment variable overrides
			if (baseUrl.Contains("localhost:5551") || baseUrl.Contains("127.0.0.1:5551"))
			{
				string? contractPort =
					Environment.GetEnvironmentVariable("hedera.mirror.Contract.port");

				baseUrl = !string.IsNullOrWhiteSpace(contractPort)
					? baseUrl.Replace(":5551", ":" + contractPort)
					: baseUrl.Replace(":5551", ":8545");
			}

			try
			{
				return await EntityIdHelper.PerformQueryToMirrorNodeAsync(baseUrl, apiEndpoint, jsonPayload);
			}
			catch (Exception ex)
			{
				// client.Logger?.Error($"Error while performing post request to Mirror Node: {ex.Message}");

				throw; // preserves stack trace (VERY important)
			}
		}
		private async Task<string> GetContractCallResultFromMirrorNodeAsync(Client client, string blockNumber)
		{
			string _ = await ExecuteMirrorNodeRequest(client, blockNumber, false);

			return ParseContractCallResult(_);
		}

		internal virtual string Call(Client client)
		{
			FillEvmAddresses();
			var blockNum = BlockNumber == 0 ? "latest" : BlockNumber.ToString();
			return GetContractCallResultFromMirrorNodeAsync(client, blockNum).GetAwaiter().GetResult();
		}
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="M:Estimate(Client)"]/*' />
		internal virtual long Estimate(Client client)
		{
			FillEvmAddresses();
			return GetEstimateGasFromMirrorNodeAsync(client).GetAwaiter().GetResult();
		}
	}

	/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="T:MirrorNodeContractQuery"]/*' />
	public abstract class MirrorNodeContractQuery<T> : MirrorNodeContractQuery where T : MirrorNodeContractQuery<T>
    {
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="M:MirrorNodeContractQuery.SetFunction(System.String)"]/*' />
		public virtual T SetFunction(string name)
		{
			return SetFunction(name, new ContractFunctionParameters());
		}
		/// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="M:MirrorNodeContractQuery.SetFunction(System.String,ContractFunctionParameters @)"]/*' />
		public virtual T SetFunction(string name, ContractFunctionParameters @params)
        {
            ArgumentNullException.ThrowIfNull(@params);
            return SetFunctionParameters(@params.ToBytes(name));
        }
        /// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="M:MirrorNodeContractQuery.SetFunctionParameters(ByteString)"]/*' />
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
    /// <include file="MirrorNodeContractQuery.cs.xml" path='docs/member[@name="T:MirrorNodeContractQuery_2"]/*' />
		
    }
}