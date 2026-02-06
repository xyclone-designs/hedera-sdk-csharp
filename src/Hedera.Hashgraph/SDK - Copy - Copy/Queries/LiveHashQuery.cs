// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Queries;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// @deprecatedThis query is no longer supported.
    /// Requests a livehash associated to an account.
    /// </remarks>
    [Obsolete("Obsolete")]
    public sealed class LiveHashQuery : Query<LiveHash, LiveHashQuery>
    {
        /// <summary>
        /// The account to which the livehash is associated
        /// </summary>
        /// <param name="AccountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountId? AccountId { get; set; }
        /// <summary>
        /// The SHA-384 data in the livehash
        /// </summary>
        /// <param name="hash">The array of bytes to be set as hash</param>
        /// <returns>{@code this}</returns>
        public byte[] Hash 
        { 
            get => field.CopyArray(); 
            set => value.CopyArray(); 

        } = [];

        public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);
		}
        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.CryptoGetLiveHashQuery
            {
                Header = header
            };

            if (AccountId != null)
            {
                builder.AccountID = AccountId.ToProtobuf();
            }

            builder.Hash = ByteString.CopyFrom(Hash);
            
            queryBuilder.CryptoGetLiveHash = builder;
        }
        public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.CryptoGetLiveHash.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.CryptoGetLiveHash.Header;
        }
        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetCryptoGetBalanceMethod();
        }
		public override LiveHash MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
		{
			return LiveHash.FromProtobuf(response.CryptoGetLiveHash.LiveHash);
		}
	}
}