// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.HBar;
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
        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.ContractCallLocalQuery
            {
				Gas = Gas,
				Header = header,
				FunctionParameters = ByteString.CopyFrom(FunctionParameters)
			};

			if (ContractId != null)
                builder.ContractID = ContractId.ToProtobuf();

            if (SenderAccountId != null)
				builder.SenderId = SenderAccountId.ToProtobuf();

			queryBuilder.ContractCallLocal = builder;
        }
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.ContractCallLocal.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.ContractCallLocal.Header;
        }
        public override ContractFunctionResult MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return new ContractFunctionResult(response.ContractCallLocal.FunctionResult);
        }

        public override MethodDescriptor GetMethodDescriptor()
        {
			string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.contractCallLocalMethod);

			return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
		}
    }
}