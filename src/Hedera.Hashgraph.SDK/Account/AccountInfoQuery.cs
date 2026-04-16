// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Account
{
	/// <include file="AccountInfoQuery.cs.xml" path='docs/member[@name="T:AccountInfoQuery"]/*' />
	public sealed class AccountInfoQuery : Query<AccountInfo, AccountInfoQuery>
    {
        /// <include file="AccountInfoQuery.cs.xml" path='docs/member[@name="P:AccountInfoQuery.AccountId"]/*' />
        public AccountId? AccountId { get; set; }

        public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);
		}
		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
			// if you set that as the query payment; 25 tinybar seems to be enough to get
			// `Token_DELETED` back instead.

			Hbar cost = await base.GetCostAsync(client);

			return Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25));
		}
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.CryptoGetInfo.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.CryptoGetInfo.Header;
        }
		public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
		{
			var builder = new Proto.Services.CryptoGetInfoQuery
			{
				Header = header,
			};

			if (AccountId != null)
			{
				builder.AccountId = AccountId.ToProtobuf();
			}

			queryBuilder.CryptoGetInfo = builder;
		}
		public override AccountInfo MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
        {
            return AccountInfo.FromProtobuf(response.CryptoGetInfo.AccountInfo);
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.CryptoService.CryptoServiceClient.getAccountInfo);

			return Proto.Services.CryptoService.Descriptor.FindMethodByName(methodname);
		}
	}
}
