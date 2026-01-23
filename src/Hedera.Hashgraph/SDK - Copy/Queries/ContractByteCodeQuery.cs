// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
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
            Objects.RequireNonNull(contractId);
            contractId = contractId;
            return this;
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
            var builder = ContractGetBytecodeQuery.NewBuilder();
            if (contractId != null)
            {
                builder.SetContractID(contractId.ToProtobuf());
            }

            queryBuilder.SetContractGetBytecode(builder.SetHeader(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetContractGetBytecodeResponse().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetContractGetBytecode().GetHeader();
        }

        override ByteString MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            return response.GetContractGetBytecodeResponse().GetBytecode();
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetContractGetBytecodeMethod();
        }
    }
}