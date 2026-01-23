// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
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
    /// Associates the provided Hedera account with the provided Hedera token(s).
    /// Hedera accounts must be associated with a fungible or non-fungible token
    /// first before you can transfer tokens to that account. In the case of
    /// NON_FUNGIBLE Type, once an account is associated, it can hold any number
    /// of NFTs (serial numbers) of that token type. The Hedera account that is
    /// being associated with a token is required to sign the transaction.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/associate-tokens-to-an-account">Hedera Documentation</a>
    /// </summary>
    public class TokenAssociation
    {
        /// <summary>
        /// The token involved in the association
        /// </summary>
        public readonly TokenId tokenId;
        /// <summary>
        /// The account involved in the association
        /// </summary>
        public readonly AccountId accountId;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="accountId">the account id</param>
        TokenAssociation(TokenId tokenId, AccountId accountId)
        {
            tokenId = tokenId;
            accountId = accountId;
        }

        /// <summary>
        /// Create a token association from a protobuf.
        /// </summary>
        /// <param name="tokenAssociation">the protobuf</param>
        /// <returns>                         the new token association</returns>
        static TokenAssociation FromProtobuf(Proto.TokenAssociation tokenAssociation)
        {
            return new TokenAssociation(tokenAssociation.HasTokenId() ? TokenId.FromProtobuf(tokenAssociation.GetTokenId()) : new TokenId(0, 0, 0), tokenAssociation.HasAccountId() ? AccountId.FromProtobuf(tokenAssociation.GetAccountId()) : new AccountId(0, 0, 0));
        }

        /// <summary>
        /// Create a token association from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new token association</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TokenAssociation FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.TokenAssociation.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        virtual Proto.TokenAssociation ToProtobuf()
        {
            return Proto.TokenAssociation.NewBuilder().SetTokenId(tokenId.ToProtobuf()).SetAccountId(accountId.ToProtobuf()).Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("tokenId", tokenId).Add("accountId", accountId).ToString();
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