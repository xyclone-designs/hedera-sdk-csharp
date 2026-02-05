// SPDX-License-Identifier: Apache-2.0
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
        private AccountId accountId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountInfoQuery()
        {
        }

        /// <summary>
        /// Extract the account id.
        /// </summary>
        /// <returns>                         the account id</returns>
        public AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// Sets the account ID for which information is requested.
        /// </summary>
        /// <param name="accountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountInfoQuery SetAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            accountId = accountId;
            return this;
        }

        public override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
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

        public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.CryptoGetInfo.Header;
        }

        public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.CryptoGetInfo.Header;
        }

        override AccountInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return AccountInfo.FromProtobuf(response.CryptoGetInfo.AccountInfo);
        }

        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetGetAccountInfoMethod();
        }

		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
			// if you set that as the query payment; 25 tinybar seems to be enough to get
			// `Token_DELETED` back instead.

			Hbar cost = await base.GetCostAsync(client);

			return Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25));
		}
	}
}