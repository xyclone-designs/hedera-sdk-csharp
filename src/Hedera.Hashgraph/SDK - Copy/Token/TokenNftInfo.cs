// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Java.Time;
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
    /// 
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/token-service/tokengetnftinfo#tokennftinfo">Hedera Documentation</a>
    /// </summary>
    public class TokenNftInfo
    {
        /// <summary>
        /// The ID of the NFT
        /// </summary>
        public readonly NftId nftId;
        /// <summary>
        /// The current owner of the NFT
        /// </summary>
        public readonly AccountId accountId;
        /// <summary>
        /// The effective consensus timestamp at which the NFT was minted
        /// </summary>
        public readonly Timestamp creationTime;
        /// <summary>
        /// Represents the unique metadata of the NFT
        /// </summary>
        public readonly byte[] metadata;
        /// <summary>
        /// The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
        /// </summary>
        public readonly LedgerId ledgerId;
        /// <summary>
        /// If an allowance is granted for the NFT, its corresponding spender account
        /// </summary>
        public readonly AccountId spenderId;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="nftId">the id of the nft</param>
        /// <param name="accountId">the current owner of the nft</param>
        /// <param name="creationTime">the effective consensus time</param>
        /// <param name="metadata">the unique metadata</param>
        /// <param name="ledgerId">the ledger id of the response</param>
        /// <param name="spenderId">the spender of the allowance (null if not an allowance)</param>
        TokenNftInfo(NftId nftId, AccountId accountId, Timestamp creationTime, byte[] metadata, LedgerId ledgerId, AccountId spenderId)
        {
            nftId = nftId;
            accountId = accountId;
            creationTime = Objects.RequireNonNull(creationTime);
            metadata = metadata;
            ledgerId = ledgerId;
            spenderId = spenderId;
        }

        /// <summary>
        /// Create token nft info from a protobuf.
        /// </summary>
        /// <param name="info">the protobuf</param>
        /// <returns>                         the new token nft info</returns>
        static TokenNftInfo FromProtobuf(Proto.TokenNftInfo info)
        {
            return new TokenNftInfo(NftId.FromProtobuf(info.GetNftID()), AccountId.FromProtobuf(info.GetAccountID()), Utils.TimestampConverter.FromProtobuf(info.GetCreationTime()), info.GetMetadata().ToByteArray(), LedgerId.FromByteString(info.GetLedgerId()), info.HasSpenderId() ? AccountId.FromProtobuf(info.GetSpenderId()) : null);
        }

        /// <summary>
        /// Create token nft info from byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new token nft info</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TokenNftInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.TokenNftInfo.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        virtual Proto.TokenNftInfo ToProtobuf()
        {
            var builder = Proto.TokenNftInfo.NewBuilder().SetNftID(nftId.ToProtobuf()).SetAccountID(accountId.ToProtobuf()).SetCreationTime(Utils.TimestampConverter.ToProtobuf(creationTime)).SetMetadata(ByteString.CopyFrom(metadata)).SetLedgerId(ledgerId.ToByteString());
            if (spenderId != null)
            {
                builder.SetSpenderId(spenderId.ToProtobuf());
            }

            return proto;
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("nftId", nftId).Add("accountId", accountId).Add("creationTime", creationTime).Add("metadata", metadata).Add("ledgerId", ledgerId).Add("spenderId", spenderId).ToString();
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}