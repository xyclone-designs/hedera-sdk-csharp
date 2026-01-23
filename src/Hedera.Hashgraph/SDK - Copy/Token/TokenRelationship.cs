// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
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
    /// Token's information related to the given Account.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/tokenrelationship">Hedera Documentation</a>
    /// </summary>
    public class TokenRelationship
    {
        /// <summary>
        /// A unique token id
        /// </summary>
        public readonly TokenId tokenId;
        /// <summary>
        /// The Symbol of the token
        /// </summary>
        public readonly string symbol;
        /// <summary>
        /// For token of type FUNGIBLE_COMMON - the balance that the Account holds
        /// in the smallest denomination.
        /// 
        /// For token of type NON_FUNGIBLE_UNIQUE - the number of NFTs held by the
        /// account
        /// </summary>
        public readonly long balance;
        /// <summary>
        /// The KYC status of the account (KycNotApplicable, Granted or Revoked).
        /// 
        /// If the token does not have KYC key, KycNotApplicable is returned
        /// </summary>
        public readonly bool kycStatus;
        /// <summary>
        /// The Freeze status of the account (FreezeNotApplicable, Frozen or
        /// Unfrozen). If the token does not have Freeze key,
        /// FreezeNotApplicable is returned
        /// </summary>
        public readonly bool freezeStatus;
        /// <summary>
        /// The amount of decimal places that this token supports.
        /// </summary>
        public readonly int decimals;
        /// <summary>
        /// Specifies if the relationship is created implicitly.
        /// False : explicitly associated,
        /// True : implicitly associated.
        /// </summary>
        public readonly bool automaticAssociation;
        TokenRelationship(TokenId tokenId, string symbol, long balance, bool kycStatus, bool freezeStatus, int decimals, bool automaticAssociation)
        {
            tokenId = tokenId;
            symbol = symbol;
            balance = balance;
            kycStatus = kycStatus;
            freezeStatus = freezeStatus;
            decimals = decimals;
            automaticAssociation = automaticAssociation;
        }

        /// <summary>
        /// Retrieve freeze status from a protobuf.
        /// </summary>
        /// <param name="freezeStatus">the protobuf</param>
        /// <returns>                         the freeze status</returns>
        static bool FreezeStatusFromProtobuf(TokenFreezeStatus freezeStatus)
        {
            return freezeStatus == TokenFreezeStatus.FreezeNotApplicable ? null : freezeStatus == TokenFreezeStatus.Frozen;
        }

        /// <summary>
        /// Retrieve the kyc status from a protobuf.
        /// </summary>
        /// <param name="kycStatus">the protobuf</param>
        /// <returns>                         the kyc status</returns>
        static bool KycStatusFromProtobuf(TokenKycStatus kycStatus)
        {
            return kycStatus == TokenKycStatus.KycNotApplicable ? null : kycStatus == TokenKycStatus.Granted;
        }

        /// <summary>
        /// Create a token relationship object from a protobuf.
        /// </summary>
        /// <param name="tokenRelationship">the protobuf</param>
        /// <returns>                         the new token relationship</returns>
        static TokenRelationship FromProtobuf(Proto.TokenRelationship tokenRelationship)
        {
            return new TokenRelationship(TokenId.FromProtobuf(tokenRelationship.GetTokenId()), tokenRelationship.GetSymbol(), tokenRelationship.GetBalance(), KycStatusFromProtobuf(tokenRelationship.GetKycStatus()), FreezeStatusFromProtobuf(tokenRelationship.GetFreezeStatus()), tokenRelationship.GetDecimals(), tokenRelationship.GetAutomaticAssociation());
        }

        /// <summary>
        /// Create a token relationship from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new token relationship</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TokenRelationship FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.TokenRelationship.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Retrieve the freeze status from a protobuf.
        /// </summary>
        /// <param name="freezeStatus">the protobuf</param>
        /// <returns>                         the freeze status</returns>
        static TokenFreezeStatus FreezeStatusToProtobuf(bool freezeStatus)
        {
            return freezeStatus == null ? TokenFreezeStatus.FreezeNotApplicable : freezeStatus ? TokenFreezeStatus.Frozen : TokenFreezeStatus.Unfrozen;
        }

        /// <summary>
        /// Retrieve the kyc status from a protobuf.
        /// </summary>
        /// <param name="kycStatus">the protobuf</param>
        /// <returns>                         the kyc status</returns>
        static TokenKycStatus KycStatusToProtobuf(bool kycStatus)
        {
            return kycStatus == null ? TokenKycStatus.KycNotApplicable : kycStatus ? TokenKycStatus.Granted : TokenKycStatus.Revoked;
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        virtual Proto.TokenRelationship ToProtobuf()
        {
            return Proto.TokenRelationship.NewBuilder().SetTokenId(tokenId.ToProtobuf()).SetSymbol(symbol).SetBalance(balance).SetKycStatus(KycStatusToProtobuf(kycStatus)).SetFreezeStatus(FreezeStatusToProtobuf(freezeStatus)).SetDecimals(decimals).SetAutomaticAssociation(automaticAssociation).Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("tokenId", tokenId).Add("symbol", symbol).Add("balance", balance).Add("kycStatus", kycStatus).Add("freezeStatus", freezeStatus).Add("decimals", decimals).Add("automaticAssociation", automaticAssociation).ToString();
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