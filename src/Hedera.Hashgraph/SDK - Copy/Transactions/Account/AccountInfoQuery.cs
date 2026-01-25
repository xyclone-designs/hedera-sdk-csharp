// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using Java.Util.Concurrent;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Transactions.Account
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
            Objects.RequireNonNull(accountId);
            accountId = accountId;
            return this;
        }

        override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = CryptoGetInfoQuery.NewBuilder();
            if (accountId != null)
            {
                builder.SetAccountID(accountId.ToProtobuf());
            }

            queryBuilder.SetCryptoGetInfo(builder.SetHeader(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetCryptoGetInfo().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetCryptoGetInfo().GetHeader();
        }

        override AccountInfo MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            return AccountInfo.FromProtobuf(response.GetCryptoGetInfo().GetAccountInfo());
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetGetAccountInfoMethod();
        }

        public override Task<Hbar> GetCostAsync(Client client)
        {

            // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
            // if you set that as the query payment; 25 tinybar seems to be enough to get
            // `ACCOUNT_DELETED` back instead.
            return base.GetCostAsync(client).ThenApply((cost) => Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25)));
        }
    }
}