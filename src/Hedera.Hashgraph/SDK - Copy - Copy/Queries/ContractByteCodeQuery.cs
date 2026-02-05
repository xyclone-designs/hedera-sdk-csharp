// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ids;

using System;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Get the bytecode for a smart contract instance.
    /// </summary>
    public sealed class ContractByteCodeQuery : Query<ByteString, ContractByteCodeQuery>
    {
        private ContractId contractId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContractByteCodeQuery()
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
        public ContractByteCodeQuery SetContractId(ContractId contractId)
        {
            ArgumentNullException.ThrowIfNull(contractId);
            contractId = contractId;
            return this;
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
            var builder = new Proto.ContractGetBytecodeQuery
            {
                Header = header
            };

            if (contractId != null)
            {
                builder.ContractID = contractId.ToProtobuf();
            }

            queryBuilder.ContractGetBytecode = builder;
        }

        public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.ContractGetBytecodeResponse.Header;
        }

        public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.ContractGetBytecode.Header;
        }

        override ByteString MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return response.ContractGetBytecodeResponse.Bytecode;
        }

        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetContractGetBytecodeMethod();
        }
    }
}