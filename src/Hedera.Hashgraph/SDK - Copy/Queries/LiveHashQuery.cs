// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
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
        private AccountId accountId = null;
        private byte[] hash = [];
        /// <summary>
        /// Constructor.
        /// </summary>
        public LiveHashQuery()
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
        /// The account to which the livehash is associated
        /// </summary>
        /// <param name="accountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public LiveHashQuery SetAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Extract the hash.
        /// </summary>
        /// <returns>                         the hash</returns>
        public ByteString GetHash()
        {
            return ByteString.CopyFrom(hash);
        }

        /// <summary>
        /// The SHA-384 data in the livehash
        /// </summary>
        /// <param name="hash">The array of bytes to be set as hash</param>
        /// <returns>{@code this}</returns>
        public LiveHashQuery SetHash(byte[] hash)
        {
            hash = hash.CopyArray();
            return this;
        }

        override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.CryptoGetLiveHashQuery
            {
                Header = header
            };

            if (accountId != null)
            {
                builder.AccountID = accountId.ToProtobuf();
            }

            builder.Hash = ByteString.CopyFrom(hash);
            
            queryBuilder.CryptoGetLiveHash = builder;
        }

        override LiveHash MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return LiveHash.FromProtobuf(response.CryptoGetLiveHash.LiveHash);
        }

        override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.CryptoGetLiveHash.Header;
        }

        override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.CryptoGetLiveHash.Header;
        }

        override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetCryptoGetBalanceMethod();
        }
    }
}