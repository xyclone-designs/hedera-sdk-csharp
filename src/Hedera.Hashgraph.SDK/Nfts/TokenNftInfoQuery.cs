// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK.Token;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Nfts
{
	/// <include file="TokenNftInfoQuery.cs.xml" path='docs/member[@name="T:TokenNftInfoQuery"]/*' />
	public class TokenNftInfoQuery : Query<IList<TokenNftInfo>, TokenNftInfoQuery>
    {
        /// <include file="TokenNftInfoQuery.cs.xml" path='docs/member[@name="P:TokenNftInfoQuery.NftId"]/*' />
        public virtual NftId? NftId { get; set; }
		/// <include file="TokenNftInfoQuery.cs.xml" path='docs/member[@name="P:TokenNftInfoQuery.TokenId"]/*' />
		public virtual TokenId? TokenId { get; set; }
		/// <include file="TokenNftInfoQuery.cs.xml" path='docs/member[@name="P:TokenNftInfoQuery.AccountId"]/*' />
		public virtual AccountId? AccountId { get; set; }

		/// <include file="TokenNftInfoQuery.cs.xml" path='docs/member[@name="P:TokenNftInfoQuery.Start"]/*' />
		public virtual long Start { get; set; }

		/// <include file="TokenNftInfoQuery.cs.xml" path='docs/member[@name="P:TokenNftInfoQuery.End"]/*' />
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
        

        public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
        {
            var builder = new Proto.Services.TokenGetNftInfoQuery
            {
                Header = header
            };

            if (NftId != null)
            {
                builder.NftId = NftId.ToProtobuf();
            }

            queryBuilder.TokenGetNftInfo = builder;
        }
		public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
		{
			return request.TokenGetInfo.Header;
		}
		public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.TokenGetNftInfo.Header;
        }
        public override IList<TokenNftInfo> MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
        {
            return [TokenNftInfo.FromProtobuf(response.TokenGetNftInfo.Nft)];
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.getTokenNftInfo);

			return Proto.Services.TokenService.Descriptor.FindMethodByName(methodname);
		}
	}
}
