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
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;

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
            Objects.RequireNonNull(tokenId);
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

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = TokenGetInfoQuery.NewBuilder();
            if (tokenId != null)
            {
                builder.SetToken(tokenId.ToProtobuf());
            }

            queryBuilder.SetTokenGetInfo(builder.SetHeader(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetTokenGetInfo().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetTokenGetInfo().GetHeader();
        }

        override TokenInfo MapResponse(Response response, AccountId nodeId, Query request)
        {
            return TokenInfo.FromProtobuf(response.GetTokenGetInfo());
        }

        override MethodDescriptor<Query, Response> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetGetTokenInfoMethod();
        }

        public override CompletableFuture<Hbar> GetCostAsync(Client client)
        {

            // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
            // if you set that as the query payment; 25 tinybar seems to be enough to get
            // `Token_DELETED` back instead.
            return base.GetCostAsync(client).ThenApply((cost) => Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25)));
        }
    }
}