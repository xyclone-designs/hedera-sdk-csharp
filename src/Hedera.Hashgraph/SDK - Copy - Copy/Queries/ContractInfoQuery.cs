// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Get information about a smart contract instance.
    /// <p>
    /// This includes the account that it uses, the file containing its bytecode,
    /// and the time when it will expire.
    /// </summary>
    public sealed class ContractInfoQuery : Query<ContractInfo, ContractInfoQuery>
    {
        private ContractId contractId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContractInfoQuery()
        {
        }

        /// <summary>
        /// Extract the contract id.
        /// </summary>
        /// <returns>                         the contract id</returns>
        public ContractId GetContractId()
        {
            return contractId;
        }

        /// <summary>
        /// Sets the contract ID for which information is requested.
        /// </summary>
        /// <param name="contractId">The ContractId to be set</param>
        /// <returns>{@code this}</returns>
        public ContractInfoQuery SetContractId(ContractId contractId)
        {
            ArgumentNullException.ThrowIfNull(contractId);
            contractId = contractId;
            return this;
        }

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
            if (contractId != null)
            {
                contractId.ValidateChecksum(client);
            }
        }

        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.ContractGetInfoQuery()
            {
                Header = header
			};

            if (contractId != null)
            {
                builder.ContractID = contractId.ToProtobuf();
            }

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

        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetGetContractInfoMethod();
        }
    }
}