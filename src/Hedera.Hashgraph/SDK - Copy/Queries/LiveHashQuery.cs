// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
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
        private byte[] hash = new[]
        {
        };
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

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = CryptoGetLiveHashQuery.NewBuilder();
            if (accountId != null)
            {
                builder.AccountID(accountId.ToProtobuf());
            }

            builder.Hash(ByteString.CopyFrom(hash));
            queryBuilder.SetCryptoGetLiveHash(builder.Header(header));
        }

        override LiveHash MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            return LiveHash.FromProtobuf(response.GetCryptoGetLiveHash().GetLiveHash());
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetCryptoGetLiveHash().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetCryptoGetLiveHash().GetHeader();
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetCryptoGetBalanceMethod();
        }
    }
}