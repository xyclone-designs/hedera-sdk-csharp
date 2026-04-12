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
    /// <include file="ContractInfoQuery.cs.xml" path='docs/member[@name="T:ContractInfoQuery"]/*' />
    public sealed class ContractInfoQuery : Query<ContractInfo, ContractInfoQuery>
    {
		/// <include file="ContractInfoQuery.cs.xml" path='docs/member[@name="P:ContractInfoQuery.ContractId"]/*' />
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

        public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
        {
            var builder = new Proto.Services.ContractGetInfoQuery()
            {
                Header = header
			};

            if (ContractId != null)
                builder.ContractID = ContractId.ToProtobuf();

            queryBuilder.ContractGetInfo = builder;
        }
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.ContractGetInfo.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.ContractGetInfo.Header;
        }
		public override ContractInfo MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
		{
			return ContractInfo.FromProtobuf(response.ContractGetInfo.ContractInfo);
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.getContractInfo);

			return Proto.Services.SmartContractService.Descriptor.FindMethodByName(methodname);
		}
	}
}
