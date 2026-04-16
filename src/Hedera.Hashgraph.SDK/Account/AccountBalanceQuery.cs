// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Queries;

namespace Hedera.Hashgraph.SDK.Account
{
	/// <include file="AccountBalanceQuery.cs.xml" path='docs/member[@name="T:AccountBalanceQuery"]/*' />
	public sealed class AccountBalanceQuery : Query<AccountBalance, AccountBalanceQuery>
    {
        /// <include file="AccountBalanceQuery.cs.xml" path='docs/member[@name="P:AccountBalanceQuery.AccountId"]/*' />
        public AccountId? AccountId { get; set; }
		/// <include file="AccountBalanceQuery.cs.xml" path='docs/member[@name="P:AccountBalanceQuery.ContractId"]/*' />
		public ContractId? ContractId { get; set; }

		public override bool IsPaymentRequired
		{
			get => false;
		}

		public override void ValidateChecksums(Client client)
        {
            AccountId?.ValidateChecksum(client);
            ContractId?.ValidateChecksum(client);
        }
        public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
        {
			Proto.Services.CryptoGetAccountBalanceQuery builder = new ()
            {
                Header = header
            };

            if (AccountId != null)
                builder.AccountId = AccountId.ToProtobuf();

            if (ContractId != null)
                builder.ContractId = ContractId.ToProtobuf();

            queryBuilder.CryptogetAccountBalance = builder;
        }
        public override AccountBalance MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
        {
            return AccountBalance.FromProtobuf(response.CryptogetAccountBalance);
        }
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.CryptogetAccountBalance.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.CryptogetAccountBalance.Header;
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.CryptoService.CryptoServiceClient.cryptoGetBalance);

			return Proto.Services.CryptoService.Descriptor.FindMethodByName(methodname);
		}
	}
}
