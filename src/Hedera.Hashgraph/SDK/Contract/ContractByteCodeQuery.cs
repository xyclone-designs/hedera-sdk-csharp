// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Ids;

using System;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Get the bytecode for a smart contract instance.
    /// </summary>
    public sealed class ContractByteCodeQuery : Query<ByteString, ContractByteCodeQuery>
    {
        public ContractId? ContractId { get; set; }

        public override void ValidateChecksums(Client client)
        {
            ContractId?.ValidateChecksum(client);
        }
		public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
		{
			var builder = new Proto.ContractGetBytecodeQuery
			{
				Header = header
			};

			if (ContractId != null)
				builder.ContractID = ContractId.ToProtobuf();

			queryBuilder.ContractGetBytecode = builder;
		}
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.ContractGetBytecode.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.ContractGetBytecodeResponse.Header;
        }
        public override ByteString MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return response.ContractGetBytecodeResponse.Bytecode;
        }

        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetContractGetBytecodeMethod();
        }
    }
}