// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Token;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Nfts
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
        /// Sets the NFT ID for which information is requested.
        /// </summary>
        /// <param name="NftId">The NftId to be set</param>
        /// <returns>{@code this}</returns>
        public virtual NftId? NftId { get; set; }

		/// <summary>
		/// Sets the Token ID and the index range for which information is requested.
		/// </summary>
		/// <param name="TokenId">The ID of the token for which information is requested</param>
		/// <returns>{@code this}</returns>
		/// <remarks>@deprecatedwith no replacement</remarks>
		public virtual TokenId? TokenId { get; set; }

		/// <summary>
		/// Sets the Account ID for which information is requested.
		/// </summary>
		/// <param name="AccountId">The Account ID for which information is requested</param>
		/// <returns>{@code this}</returns>
		/// <remarks>@deprecatedwith no replacement</remarks>
		public virtual AccountId? AccountId { get; set; }

		/// <summary>
		/// Sets the start of the index range for which information is requested.
		/// </summary>
		/// <param name="start">The start index (inclusive) of the range of NFTs to query for. Value must be in the range [0; ownedNFTs-1]</param>
		/// <returns>{@code this}</returns>
		/// <remarks>@deprecatedwith no replacement</remarks>
		public virtual long Start { get; set; }

		/// <summary>
		/// Sets the end of the index range for which information is requested.
		/// </summary>
		/// <param name="end">The end index (exclusive) of the range of NFTs to query for. Value must be in the range (start; ownedNFTs]</param>
		/// <returns>{@code this}</returns>
		/// <remarks>@deprecatedwith no replacement</remarks>
		public virtual long End { get; set; }

		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
			// if you set that as the query payment; 25 tinybar seems to be enough to get
			// `Token_DELETED` back instead.

			Hbar hbar = await base.GetCostAsync(client);

			return Hbar.FromTinybars(Math.Max(hbar.ToTinybars(), 25));
		}

		public override Task OnExecuteAsync(Client client)
		{
			int modesEnabled = (NftId != null ? 1 : 0) + (TokenId != null ? 1 : 0) + (AccountId != null ? 1 : 0);
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
		public override void ValidateChecksums(Client client)
        {
			NftId?.TokenId.ValidateChecksum(client);
		}
        

        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.TokenGetNftInfoQuery
            {
                Header = header
            };

            if (NftId != null)
            {
                builder.NftID = NftId.ToProtobuf();
            }

            queryBuilder.TokenGetNftInfo = builder;
        }
		public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.TokenGetInfo.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.TokenGetNftInfo.Header;
        }
        public override IList<TokenNftInfo> MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return [TokenNftInfo.FromProtobuf(response.TokenGetNftInfo.Nft)];
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.getTokenNftInfo);

			return Proto.TokenService.Descriptor.FindMethodByName(methodname);
		}

	}
}