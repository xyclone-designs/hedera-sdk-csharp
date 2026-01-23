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
using static Hedera.Hashgraph.SDK.TokenKeyValidation;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Class to encapsulate the nft methods for token allowance's.
    /// </summary>
    public class TokenNftAllowance
    {
        /// <summary>
        /// The NFT token type that the allowance pertains to
        /// </summary>
        public readonly TokenId tokenId;
        /// <summary>
        /// The account ID of the token owner (ie. the grantor of the allowance)
        /// </summary>
        public readonly AccountId ownerAccountId;
        /// <summary>
        /// The account ID of the token allowance spender
        /// </summary>
        public readonly AccountId spenderAccountId;
        /// <summary>
        /// The account ID of the spender who is granted approvedForAll allowance and granting
        /// approval on an NFT serial to another spender.
        /// </summary>
        AccountId delegatingSpender;
        /// <summary>
        /// The list of serial numbers that the spender is permitted to transfer.
        /// </summary>
        public readonly IList<long> serialNumbers;
        /// <summary>
        /// If true, the spender has access to all of the owner's NFT units of type tokenId (currently
        /// owned and any in the future).
        /// </summary>
        public readonly bool allSerials;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="ownerAccountId">the grantor's account id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <param name="delegatingSpender">the delegating spender's account id</param>
        /// <param name="serialNumbers">the list of serial numbers</param>
        /// <param name="allSerials">grant for all serial's</param>
        TokenNftAllowance(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId, AccountId delegatingSpender, Collection<long> serialNumbers, bool allSerials)
        {
            tokenId = tokenId;
            ownerAccountId = ownerAccountId;
            spenderAccountId = spenderAccountId;
            delegatingSpender = delegatingSpender;
            serialNumbers = new List(serialNumbers);
            allSerials = allSerials;
        }

        /// <summary>
        /// Create a copy of a nft token allowance object.
        /// </summary>
        /// <param name="allowance">the nft token allowance to copj</param>
        /// <returns>                         a new copy</returns>
        static TokenNftAllowance CopyFrom(TokenNftAllowance allowance)
        {
            return new TokenNftAllowance(allowance.tokenId, allowance.ownerAccountId, allowance.spenderAccountId, allowance.delegatingSpender, allowance.serialNumbers, allowance.allSerials);
        }

        /// <summary>
        /// Create a nft token allowance from a protobuf.
        /// </summary>
        /// <param name="allowanceProto">the protobuf</param>
        /// <returns>                         the nft token allowance</returns>
        static TokenNftAllowance FromProtobuf(NftAllowance allowanceProto)
        {
            return new TokenNftAllowance(allowanceProto.HasTokenId() ? TokenId.FromProtobuf(allowanceProto.GetTokenId()) : null, allowanceProto.HasOwner() ? AccountId.FromProtobuf(allowanceProto.GetOwner()) : null, allowanceProto.HasSpender() ? AccountId.FromProtobuf(allowanceProto.GetSpender()) : null, allowanceProto.HasDelegatingSpender() ? AccountId.FromProtobuf(allowanceProto.GetDelegatingSpender()) : null, allowanceProto.GetSerialNumbersList(), allowanceProto.HasApprovedForAll() ? allowanceProto.GetApprovedForAll().GetValue() : null);
        }

        /// <summary>
        /// Create a nft token allowance from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the nft token allowance</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TokenNftAllowance FromBytes(byte[] bytes)
        {
            return FromProtobuf(NftAllowance.ParseFrom(Objects.RequireNonNull(bytes)));
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

            if (delegatingSpender != null)
            {
                delegatingSpender.ValidateChecksum(client);
            }
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        virtual NftAllowance ToProtobuf()
        {
            var builder = NftAllowance.NewBuilder();
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

            if (delegatingSpender != null)
            {
                builder.SetDelegatingSpender(delegatingSpender.ToProtobuf());
            }

            builder.AddAllSerialNumbers(serialNumbers);
            if (allSerials != null)
            {
                builder.SetApprovedForAll(BoolValue.NewBuilder().SetValue(allSerials).Build());
            }

            return proto;
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the remove protobuf</returns>
        virtual NftRemoveAllowance ToRemoveProtobuf()
        {
            var builder = NftRemoveAllowance.NewBuilder();
            if (tokenId != null)
            {
                builder.SetTokenId(tokenId.ToProtobuf());
            }

            if (ownerAccountId != null)
            {
                builder.SetOwner(ownerAccountId.ToProtobuf());
            }

            builder.AddAllSerialNumbers(serialNumbers);
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
            var stringHelper = MoreObjects.ToStringHelper(this).Add("tokenId", tokenId).Add("ownerAccountId", ownerAccountId).Add("spenderAccountId", spenderAccountId).Add("delegatingSpender", delegatingSpender);
            if (allSerials != null)
            {
                stringHelper.Add("allSerials", allSerials);
            }
            else
            {
                stringHelper.Add("serials", serialNumbers);
            }

            return stringHelper.ToString();
        }
    }
}