// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// Get all the information about an account, including the balance.
    /// This does not get the list of account records.
    /// </summary>
    public sealed class AccountInfoQuery : Query<AccountInfo, AccountInfoQuery>
    {
        /// <summary>
        /// Sets the account ID for which information is requested.
        /// </summary>
        /// <param name="accountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountId? AccountId { get; set; }

        public override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }
		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
			// if you set that as the query payment; 25 tinybar seems to be enough to get
			// `Token_DELETED` back instead.

			Hbar cost = await base.GetCostAsync(client);

			return Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25));
		}
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.CryptoGetInfo.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.CryptoGetInfo.Header;
        }
		public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
		{
			var builder = new Proto.CryptoGetInfoQuery
			{
				Header = header,
			};

			if (accountId != null)
			{
				builder.AccountID = accountId.ToProtobuf();
			}

			queryBuilder.CryptoGetInfo = builder;
		}
		public override AccountInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return AccountInfo.FromProtobuf(response.CryptoGetInfo.AccountInfo);
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.getAccountInfo);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}
	}
}