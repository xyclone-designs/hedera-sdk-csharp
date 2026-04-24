// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Queries;
using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractCallQuery.cs.xml" path='docs/member[@name="T:ContractCallQuery"]/*' />
    public sealed class ContractCallQuery : Query<ContractFunctionResult, ContractCallQuery>
    {
		public ContractId? ContractId { get; set; }
		/// <include file="ContractCallQuery.cs.xml" path='docs/member[@name="P:ContractCallQuery.Gas"]/*' />
		public long Gas { get; set; }
        /// <include file="ContractCallQuery.cs.xml" path='docs/member[@name="M:ContractCallQuery.CopyArray"]/*' />
        public byte[] FunctionParameters
        {
            get => field.CopyArray();
            set => field = value.CopyArray();

        } = [];
		/// <include file="ContractCallQuery.cs.xml" path='docs/member[@name="P:ContractCallQuery.MaXResultSize"]/*' />
		public long MaXResultSize { get; set; }
		/// <include file="ContractCallQuery.cs.xml" path='docs/member[@name="P:ContractCallQuery.SenderAccountId"]/*' />
		public AccountId? SenderAccountId { get; set; }

		/// <include file="ContractCallQuery.cs.xml" path='docs/member[@name="M:ContractCallQuery.SetFunction(System.String)"]/*' />
		public ContractCallQuery SetFunction(string name)
		{
			return SetFunction(name, new ContractFunctionParameters());
		}
		/// <include file="ContractCallQuery.cs.xml" path='docs/member[@name="M:ContractCallQuery.SetFunction(System.String,ContractFunctionParameters @)"]/*' />
		public ContractCallQuery SetFunction(string name, ContractFunctionParameters @params)
		{
			FunctionParameters = @params.ToBytes(name).ToByteArray();

			return this;
		}

		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// network bug: ContractCallLocal cost estimate is too low

			Hbar cost = await base.GetCostAsync(client);

			return Hbar.FromTinybars((long)(cost.ToTinybars() * 1.1));

		}

		public override void ValidateChecksums(Client client)
        {
			ContractId?.ValidateChecksum(client);
		}
        public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
        {
            var builder = new Proto.Services.ContractCallLocalQuery
            {
				Gas = Gas,
				Header = header,
				FunctionParameters = ByteString.CopyFrom(FunctionParameters)
			};

			if (ContractId != null)
                builder.ContractId = ContractId.ToProtobuf();

            if (SenderAccountId != null)
				builder.SenderId = SenderAccountId.ToProtobuf();

			queryBuilder.ContractCallLocal = builder;
        }
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.ContractCallLocal.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.ContractCallLocal.Header;
        }
        public override ContractFunctionResult MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
        {
            return new ContractFunctionResult(response.ContractCallLocal.FunctionResult);
        }

        public override MethodDescriptor GetMethodDescriptor()
        {
			string methodname = nameof(Proto.Services.SmartContractService.SmartContractServiceClient.contractCallLocalMethod);

			return Proto.Services.SmartContractService.Descriptor.FindMethodByName(methodname);
		}
    }
}
