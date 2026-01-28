// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using Java.Util.Concurrent;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

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

        public override Task<Hbar> GetCostAsync(Client client)
        {

            // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
            // if you set that as the query payment; 25 tinybar seems to be enough to get
            // `CONTRACT_DELETED` back instead.
            return base.GetCostAsync(client).ThenApply((cost) => Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25)));
        }

        override void ValidateChecksums(Client client)
        {
            if (contractId != null)
            {
                contractId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = ContractGetInfoQuery.NewBuilder();
            if (contractId != null)
            {
                builder.ContractID(contractId.ToProtobuf());
            }

            queryBuilder.SetContractGetInfo(builder.Header(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetContractGetInfo().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetContractGetInfo().GetHeader();
        }

        override ContractInfo MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            return ContractInfo.FromProtobuf(response.GetContractGetInfo().GetContractInfo());
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetGetContractInfoMethod();
        }
    }
}