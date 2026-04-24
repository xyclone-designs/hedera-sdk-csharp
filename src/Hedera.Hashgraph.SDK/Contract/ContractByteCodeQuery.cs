// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Contract;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <include file="ContractByteCodeQuery.cs.xml" path='docs/member[@name="T:ContractByteCodeQuery"]/*' />
    public sealed class ContractByteCodeQuery : Query<ByteString, ContractByteCodeQuery>
    {
        public ContractId? ContractId { get; set; }

        public override void ValidateChecksums(Client client)
        {
            ContractId?.ValidateChecksum(client);
        }
		public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
		{
			var builder = new Proto.Services.ContractGetBytecodeQuery
			{
				Header = header
			};

			if (ContractId != null)
				builder.ContractId = ContractId.ToProtobuf();

			queryBuilder.ContractGetBytecode = builder;
		}
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.ContractGetBytecode.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.ContractGetBytecodeResponse.Header;
        }
        public override ByteString MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
        {
            return response.ContractGetBytecodeResponse.Bytecode;
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.ContractGetBytecode);

			return Proto.Services.SmartContractService.Descriptor.FindMethodByName(methodname);
		}
	}
}
