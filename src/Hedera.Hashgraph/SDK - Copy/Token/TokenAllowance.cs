// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
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
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// An approved allowance of token transfers for a spender.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/tokenallowance">Hedera Documentation</a>
    /// </summary>
    public class TokenAllowance
    {
        /// <summary>
        /// The token that the allowance pertains to
        /// </summary>
        public readonly TokenId tokenId;
        /// <summary>
        /// The account ID of the hbar owner (ie. the grantor of the allowance)
        /// </summary>
        public readonly AccountId ownerAccountId;
        /// <summary>
        /// The account ID of the spender of the hbar allowance
        /// </summary>
        public readonly AccountId spenderAccountId;
        /// <summary>
        /// The amount of the spender's token allowance
        /// </summary>
        public readonly long amount;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="ownerAccountId">the grantor account id</param>
        /// <param name="spenderAccountId">the spender account id</param>
        /// <param name="amount">the token allowance</param>
        TokenAllowance(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId, long amount)
        {
            tokenId = tokenId;
            ownerAccountId = ownerAccountId;
            spenderAccountId = spenderAccountId;
            amount = amount;
        }

        /// <summary>
        /// Create a token allowance from a protobuf.
        /// </summary>
        /// <param name="allowanceProto">the protobuf</param>
        /// <returns>                         the new token allowance</returns>
        static TokenAllowance FromProtobuf(Proto.TokenAllowance allowanceProto)
        {
            return new TokenAllowance(allowanceProto.HasTokenId() ? TokenId.FromProtobuf(allowanceProto.GetTokenId()) : null, allowanceProto.HasOwner() ? AccountId.FromProtobuf(allowanceProto.GetOwner()) : null, allowanceProto.HasSpender() ? AccountId.FromProtobuf(allowanceProto.GetSpender()) : null, allowanceProto.GetAmount());
        }

        /// <summary>
        /// Create a token allowance from a protobuf.
        /// </summary>
        /// <param name="allowanceProto">the protobuf</param>
        /// <returns>                         the new token allowance</returns>
        static TokenAllowance FromProtobuf(GrantedTokenAllowance allowanceProto)
        {
            return new TokenAllowance(allowanceProto.HasTokenId() ? TokenId.FromProtobuf(allowanceProto.GetTokenId()) : null, null, allowanceProto.HasSpender() ? AccountId.FromProtobuf(allowanceProto.GetSpender()) : null, allowanceProto.GetAmount());
        }

        /// <summary>
        /// Create a token allowance from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new token allowance</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TokenAllowance FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.TokenAllowance.ParseFrom(Objects.RequireNonNull(bytes)));
        }

        /// <summary>
        /// Validate the configured client.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        virtual void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }

            if (ownerAccountId != null)
            {
                ownerAccountId.ValidateChecksum(client);
            }

            if (spenderAccountId != null)
            {
                spenderAccountId.ValidateChecksum(client);
            }
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        virtual Proto.TokenAllowance ToProtobuf()
        {
            var builder = Proto.TokenAllowance.NewBuilder().SetAmount(amount);
            if (tokenId != null)
            {
                builder.SetTokenId(tokenId.ToProtobuf());
            }

            if (ownerAccountId != null)
            {
                builder.SetOwner(ownerAccountId.ToProtobuf());
            }

            if (spenderAccountId != null)
            {
                builder.SetSpender(spenderAccountId.ToProtobuf());
            }

            return proto;
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        virtual GrantedTokenAllowance ToGrantedProtobuf()
        {
            var builder = GrantedTokenAllowance.NewBuilder().SetAmount(amount);
            if (tokenId != null)
            {
                builder.SetTokenId(tokenId.ToProtobuf());
            }

            if (spenderAccountId != null)
            {
                builder.SetSpender(spenderAccountId.ToProtobuf());
            }

            return proto;
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("tokenId", tokenId).Add("ownerAccountId", ownerAccountId).Add("spenderAccountId", spenderAccountId).Add("amount", amount).ToString();
        }
    }
}