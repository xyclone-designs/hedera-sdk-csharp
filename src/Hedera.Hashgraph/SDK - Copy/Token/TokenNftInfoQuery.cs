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
using static Hedera.Hashgraph.SDK.TokenKeyValidation;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// A query that returns information about a non-fungible token (NFT).
    /// 
    /// You request the info for an NFT by specifying the NFT ID.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/get-nft-token-info#methods">Hedera Documentation</a>
    /// </summary>
    public class TokenNftInfoQuery : Query<IList<TokenNftInfo>, TokenNftInfoQuery>
    {
        /// <summary>
        /// The ID of the non-fungible token in x.y.z format.
        /// </summary>
        private NftId nftId = null;
        private TokenId tokenId = null;
        /// <summary>
        /// The account ID of the current owner of the NFT
        /// </summary>
        private AccountId accountId = null;
        private long start = 0;
        private long end = 0;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenNftInfoQuery()
        {
        }

        /// <summary>
        /// Sets the NFT ID for which information is requested.
        /// </summary>
        /// <param name="nftId">The NftId to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecateduse {@link TokenNftInfoQuery#setNftId(NftId)} instead</remarks>
        public virtual TokenNftInfoQuery ByNftId(NftId nftId)
        {
            return SetNftId(nftId);
        }

        /// <summary>
        /// Sets the NFT ID for which information is requested.
        /// </summary>
        /// <param name="nftId">The NftId to be set</param>
        /// <returns>{@code this}</returns>
        public virtual TokenNftInfoQuery SetNftId(NftId nftId)
        {
            Objects.RequireNonNull(nftId);
            nftId = nftId;
            return this;
        }

        /// <summary>
        /// Extract the nft id.
        /// </summary>
        /// <returns>                         the nft id</returns>
        public virtual NftId GetNftId()
        {
            return nftId;
        }

        /// <summary>
        /// Sets the Token ID and the index range for which information is requested.
        /// </summary>
        /// <param name="tokenId">The ID of the token for which information is requested</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual TokenNftInfoQuery ByTokenId(TokenId tokenId)
        {
            Objects.RequireNonNull(tokenId);
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the token id
        /// </summary>
        /// <returns>the tokenId</returns>
        public virtual TokenId GetTokenId()
        {
            return tokenId;
        }

        /// <summary>
        /// Sets the Account ID for which information is requested.
        /// </summary>
        /// <param name="accountId">The Account ID for which information is requested</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual TokenNftInfoQuery ByAccountId(AccountId accountId)
        {
            Objects.RequireNonNull(accountId);
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Get the Account ID for which information is requested
        /// </summary>
        /// <returns>the accountId</returns>
        public virtual AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// Get the start of the index range for which information is requested
        /// </summary>
        /// <returns>the start</returns>
        public virtual long GetStart()
        {
            return start;
        }

        /// <summary>
        /// Sets the start of the index range for which information is requested.
        /// </summary>
        /// <param name="start">The start index (inclusive) of the range of NFTs to query for. Value must be in the range [0; ownedNFTs-1]</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual TokenNftInfoQuery SetStart(long start)
        {
            start = start;
            return this;
        }

        /// <summary>
        /// Get the end of the index range for which information is requested
        /// </summary>
        /// <returns>the end</returns>
        public virtual long GetEnd()
        {
            return end;
        }

        /// <summary>
        /// Sets the end of the index range for which information is requested.
        /// </summary>
        /// <param name="end">The end index (exclusive) of the range of NFTs to query for. Value must be in the range (start; ownedNFTs]</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual TokenNftInfoQuery SetEnd(long end)
        {
            end = end;
            return this;
        }

        override void ValidateChecksums(Client client)
        {
            if (nftId != null)
            {
                nftId.TokenId.ValidateChecksum(client);
            }
        }

        override Task OnExecuteAsync(Client client)
        {
            int modesEnabled = (nftId != null ? 1 : 0) + (tokenId != null ? 1 : 0) + (accountId != null ? 1 : 0);
            if (modesEnabled > 1)
            {
                throw new InvalidOperationException("TokenNftInfoQuery must be one of byNftId, byTokenId, or byAccountId, but multiple of these modes have been selected");
            }
            else if (modesEnabled == 0)
            {
                throw new InvalidOperationException("TokenNftInfoQuery must be one of byNftId, byTokenId, or byAccountId, but none of these modes have been selected");
            }

            return base.OnExecuteAsync(client);
        }

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = TokenGetNftInfoQuery.NewBuilder();
            if (nftId != null)
            {
                builder.SetNftID(nftId.ToProtobuf());
            }

            queryBuilder.SetTokenGetNftInfo(builder.SetHeader(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetTokenGetNftInfo().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetTokenGetInfo().GetHeader();
        }

        override IList<TokenNftInfo> MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            return Collections.SingletonList(TokenNftInfo.FromProtobuf(response.GetTokenGetNftInfo().GetNft()));
        }

        override MethodDescriptor<Query, Response> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetGetTokenNftInfoMethod();
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