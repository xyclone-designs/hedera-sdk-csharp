// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Initializes the TokenInfoQuery object.
    /// </summary>
    public class TokenInfoQuery : Query<TokenInfo, TokenInfoQuery>
    {
        /// <summary>
        /// Sets the Token ID for which information is requested.
        /// </summary>
        /// <param name="TokenId">The TokenId to be set</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set; }

		public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);
		}

        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.TokenGetInfoQuery()
            {
                Header = header
            };

            if (TokenId != null)
            {
                builder.Token = TokenId.ToProtobuf();
            }

            queryBuilder.TokenGetInfo = builder;
        }

        public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.TokenGetInfo.Header;
        }

        public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.TokenGetInfo.Header;
        }

        public override TokenInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return TokenInfo.FromProtobuf(response.TokenGetInfo);
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.getTokenInfo);

			return Proto.TokenService.Descriptor.FindMethodByName(methodname);
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