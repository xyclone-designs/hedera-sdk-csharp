// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Initializes the TokenInfoQuery object.
    /// </summary>
    public class TokenInfoQuery : Query<TokenInfo, TokenInfoQuery>
    {
        TokenId tokenId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenInfoQuery()
        {
        }

        /// <summary>
        /// Extract the token id.
        /// </summary>
        /// <returns>                         the token id</returns>
        public virtual TokenId GetTokenId()
        {
            return tokenId;
        }

        /// <summary>
        /// Sets the Token ID for which information is requested.
        /// </summary>
        /// <param name="tokenId">The TokenId to be set</param>
        /// <returns>{@code this}</returns>
        public virtual TokenInfoQuery SetTokenId(TokenId tokenId)
        {
            ArgumentNullException.ThrowIfNull(tokenId);
            tokenId = tokenId;
            return this;
        }

        override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.TokenGetInfoQuery()
            {
                Header = header
            };

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            queryBuilder.TokenGetInfo = builder;
        }

        override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.TokenGetInfo.Header;
        }

        override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.TokenGetInfo.Header;
        }

        override TokenInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return TokenInfo.FromProtobuf(response.TokenGetInfo);
        }

        override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetGetTokenInfoMethod();
        }

        public override Task<Hbar> GetCostAsync(Client client)
        {

            // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
            // if you set that as the query payment; 25 tinybar seems to be enough to get
            // `Token_DELETED` back instead.
            return base.GetCostAsync(client).ThenApply((cost) => Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25)));
        }
    }
}