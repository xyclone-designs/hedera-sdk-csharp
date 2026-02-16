// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Queries;
using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// Get information about a smart contract instance.
    /// <p>
    /// This includes the account that it uses, the file containing its bytecode,
    /// and the time when it will expire.
    /// </summary>
    public sealed class ContractInfoQuery : Query<ContractInfo, ContractInfoQuery>
    {
		/// <summary>
		/// Sets the contract ID for which information is requested.
		/// </summary>
		/// <param name="contractId">The ContractId to be set</param>
		/// <returns>{@code this}</returns>
		public ContractId? ContractId { get; set; }

		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
			// if you set that as the query payment; 25 tinybar seems to be enough to get
			// `Token_DELETED` back instead.

			Hbar cost = await base.GetCostAsync(client);

			return Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25));
		}
		public override void ValidateChecksums(Client client)
        {
			ContractId?.ValidateChecksum(client);
        }

        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.ContractGetInfoQuery()
            {
                Header = header
			};

            if (ContractId != null)
                builder.ContractID = ContractId.ToProtobuf();

            queryBuilder.ContractGetInfo = builder;
        }
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.ContractGetInfo.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.ContractGetInfo.Header;
        }
		public override ContractInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
		{
			return ContractInfo.FromProtobuf(response.ContractGetInfo.ContractInfo);
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.getContractInfo);

			return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
		}
	}
}