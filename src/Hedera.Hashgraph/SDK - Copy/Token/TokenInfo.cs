// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
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

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Gets information about a fungible or non-fungible token instance.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/get-token-info">Hedera Documentation</a>
    /// </summary>
    public class TokenInfo
    {
        /// <summary>
        /// The ID of the token for which information is requested.
        /// </summary>
        public readonly TokenId tokenId;
        /// <summary>
        /// Name of token.
        /// </summary>
        public readonly string name;
        /// <summary>
        /// Symbol of token.
        /// </summary>
        public readonly string symbol;
        /// <summary>
        /// The amount of decimal places that this token supports.
        /// </summary>
        public readonly int decimals;
        /// <summary>
        /// Total Supply of token.
        /// </summary>
        public readonly long totalSupply;
        /// <summary>
        /// The ID of the account which is set as Treasury
        /// </summary>
        public readonly AccountId treasuryAccountId;
        /// <summary>
        /// The key which can perform update/delete operations on the token. If empty, the token can be perceived as immutable (not being able to be updated/deleted)
        /// </summary>
        public readonly Key adminKey;
        /// <summary>
        /// The key which can grant or revoke KYC of an account for the token's transactions. If empty, KYC is not required, and KYC grant or revoke operations are not possible.
        /// </summary>
        public readonly Key kycKey;
        /// <summary>
        /// The key which can freeze or unfreeze an account for token transactions. If empty, freezing is not possible
        /// </summary>
        public readonly Key freezeKey;
        /// <summary>
        /// The key which can wipe token balance of an account. If empty, wipe is not possible
        /// </summary>
        public readonly Key wipeKey;
        /// <summary>
        /// The key which can change the supply of a token. The key is used to sign Token Mint/Burn operations
        /// </summary>
        public readonly Key supplyKey;
        /// <summary>
        /// The key which can change the custom fees of the token; if not set, the fees are immutable
        /// </summary>
        public readonly Key feeScheduleKey;
        /// <summary>
        /// The default Freeze status (not applicable, frozen or unfrozen) of Hedera accounts relative to this token. FreezeNotApplicable is returned if Token Freeze Key is empty. Frozen is returned if Token Freeze Key is set and defaultFreeze is set to true. Unfrozen is returned if Token Freeze Key is set and defaultFreeze is set to false
        /// </summary>
        public readonly bool defaultFreezeStatus;
        /// <summary>
        /// The default KYC status (KycNotApplicable or Revoked) of Hedera accounts relative to this token. KycNotApplicable is returned if KYC key is not set, otherwise Revoked
        /// </summary>
        public readonly bool defaultKycStatus;
        /// <summary>
        /// Specifies whether the token was deleted or not
        /// </summary>
        public readonly bool isDeleted;
        /// <summary>
        /// An account which will be automatically charged to renew the token's expiration, at autoRenewPeriod interval
        /// </summary>
        public readonly AccountId autoRenewAccount;
        /// <summary>
        /// The interval at which the auto-renew account will be charged to extend the token's expiry
        /// </summary>
        public readonly Duration autoRenewPeriod;
        /// <summary>
        /// The epoch second at which the token will expire
        /// </summary>
        public readonly Timestamp expirationTime;
        /// <summary>
        /// The memo associated with the token
        /// </summary>
        public readonly string tokenMemo;
        /// <summary>
        /// The custom fees to be assessed during a CryptoTransfer that transfers units of this token
        /// </summary>
        public readonly IList<CustomFee> customFees;
        /// <summary>
        /// The token type
        /// </summary>
        public readonly TokenType tokenType;
        /// <summary>
        /// The token supply type
        /// </summary>
        public readonly TokenSupplyType supplyType;
        /// <summary>
        /// For tokens of type FUNGIBLE_COMMON - The Maximum number of fungible tokens that can be in
        /// circulation. For tokens of type NON_FUNGIBLE_UNIQUE - the maximum number of NFTs (serial
        /// numbers) that can be in circulation
        /// </summary>
        public readonly long maxSupply;
        /// <summary>
        /// The Key which can pause and unpause the Token.
        /// </summary>
        public readonly Key pauseKey;
        /// <summary>
        /// Specifies whether the token is paused or not. Null if pauseKey is not set.
        /// </summary>
        public readonly bool pauseStatus;
        /// <summary>
        /// Represents the metadata of the token definition.
        /// </summary>
        public byte[] metadata = new[]
        {
        };
        /// <summary>
        /// The key which can change the metadata of a token
        /// (token definition and individual NFTs).
        /// </summary>
        public readonly Key metadataKey;
        /// <summary>
        /// The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
        /// </summary>
        public readonly LedgerId ledgerId;
        TokenInfo(TokenId tokenId, string name, string symbol, int decimals, long totalSupply, AccountId treasuryAccountId, Key adminKey, Key kycKey, Key freezeKey, Key wipeKey, Key supplyKey, Key feeScheduleKey, bool defaultFreezeStatus, bool defaultKycStatus, bool isDeleted, AccountId autoRenewAccount, Duration autoRenewPeriod, Timestamp expirationTime, string tokenMemo, IList<CustomFee> customFees, TokenType tokenType, TokenSupplyType supplyType, long maxSupply, Key pauseKey, bool pauseStatus, byte[] metadata, Key metadataKey, LedgerId ledgerId)
        {
            tokenId = tokenId;
            name = name;
            symbol = symbol;
            decimals = decimals;
            totalSupply = totalSupply;
            treasuryAccountId = treasuryAccountId;
            adminKey = adminKey;
            kycKey = kycKey;
            freezeKey = freezeKey;
            wipeKey = wipeKey;
            supplyKey = supplyKey;
            feeScheduleKey = feeScheduleKey;
            defaultFreezeStatus = defaultFreezeStatus;
            defaultKycStatus = defaultKycStatus;
            isDeleted = isDeleted;
            autoRenewAccount = autoRenewAccount;
            autoRenewPeriod = autoRenewPeriod;
            expirationTime = expirationTime;
            tokenMemo = tokenMemo;
            customFees = customFees;
            tokenType = tokenType;
            supplyType = supplyType;
            maxSupply = maxSupply;
            pauseKey = pauseKey;
            pauseStatus = pauseStatus;
            metadata = metadata;
            metadataKey = metadataKey;
            ledgerId = ledgerId;
        }

        /// <summary>
        /// Are we frozen?
        /// </summary>
        /// <param name="freezeStatus">the freeze status</param>
        /// <returns>                         true / false / null</returns>
        static bool FreezeStatusFromProtobuf(TokenFreezeStatus freezeStatus)
        {
            return freezeStatus == TokenFreezeStatus.FreezeNotApplicable ? null : freezeStatus == TokenFreezeStatus.Frozen;
        }

        /// <summary>
        /// Is kyc required?
        /// </summary>
        /// <param name="kycStatus">the kyc status</param>
        /// <returns>                         true / false / null</returns>
        static bool KycStatusFromProtobuf(TokenKycStatus kycStatus)
        {
            return kycStatus == TokenKycStatus.KycNotApplicable ? null : kycStatus == TokenKycStatus.Granted;
        }

        /// <summary>
        /// Are we paused?
        /// </summary>
        /// <param name="pauseStatus">the paused status</param>
        /// <returns>                         true / false / null</returns>
        static bool PauseStatusFromProtobuf(TokenPauseStatus pauseStatus)
        {
            return pauseStatus == TokenPauseStatus.PauseNotApplicable ? null : pauseStatus == TokenPauseStatus.Paused;
        }

        /// <summary>
        /// Create a token info object from a protobuf.
        /// </summary>
        /// <param name="response">the protobuf</param>
        /// <returns>                         new token info object</returns>
        static TokenInfo FromProtobuf(TokenGetInfoResponse response)
        {
            var info = response.GetTokenInfo();
            return new TokenInfo(TokenId.FromProtobuf(info.GetTokenId()), info.GetName(), info.GetSymbol(), info.GetDecimals(), info.GetTotalSupply(), AccountId.FromProtobuf(info.GetTreasury()), info.HasAdminKey() ? Key.FromProtobufKey(info.GetAdminKey()) : null, info.HasKycKey() ? Key.FromProtobufKey(info.GetKycKey()) : null, info.HasFreezeKey() ? Key.FromProtobufKey(info.GetFreezeKey()) : null, info.HasWipeKey() ? Key.FromProtobufKey(info.GetWipeKey()) : null, info.HasSupplyKey() ? Key.FromProtobufKey(info.GetSupplyKey()) : null, info.HasFeeScheduleKey() ? Key.FromProtobufKey(info.GetFeeScheduleKey()) : null, FreezeStatusFromProtobuf(info.GetDefaultFreezeStatus()), KycStatusFromProtobuf(info.GetDefaultKycStatus()), info.GetDeleted(), info.HasAutoRenewAccount() ? AccountId.FromProtobuf(info.GetAutoRenewAccount()) : null, info.HasAutoRenewPeriod() ? Utils.DurationConverter.FromProtobuf(info.GetAutoRenewPeriod()) : null, info.HasExpiry() ? TimestampConverter.FromProtobuf(info.GetExpiry()) : null, info.GetMemo(), CustomFeesFromProto(info), TokenType.ValueOf(info.GetTokenType()), TokenSupplyType.ValueOf(info.GetSupplyType()), info.GetMaxSupply(), info.HasPauseKey() ? Key.FromProtobufKey(info.GetPauseKey()) : null, PauseStatusFromProtobuf(info.GetPauseStatus()), info.GetMetadata().ToByteArray(), info.HasMetadataKey() ? Key.FromProtobufKey(info.GetMetadataKey()) : null, LedgerId.FromByteString(info.GetLedgerId()));
        }

        /// <summary>
        /// Create a token info object from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new token info object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TokenInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(TokenGetInfoResponse.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create custom fee list from protobuf.
        /// </summary>
        /// <param name="info">the protobuf</param>
        /// <returns>                         the list of custom fee's</returns>
        private static IList<CustomFee> CustomFeesFromProto(Proto.TokenInfo info)
        {
            var returnCustomFees = new List<CustomFee>(info.GetCustomFeesCount());
            foreach (var feeProto in info.GetCustomFeesList())
            {
                returnCustomFees.Add(CustomFee.FromProtobuf(feeProto));
            }

            return returnCustomFees;
        }

        /// <summary>
        /// Create a token freeze status protobuf.
        /// </summary>
        /// <param name="freezeStatus">the freeze status</param>
        /// <returns>                         the protobuf</returns>
        static TokenFreezeStatus FreezeStatusToProtobuf(bool freezeStatus)
        {
            return freezeStatus == null ? TokenFreezeStatus.FreezeNotApplicable : freezeStatus ? TokenFreezeStatus.Frozen : TokenFreezeStatus.Unfrozen;
        }

        /// <summary>
        /// Create a kyc status protobuf.
        /// </summary>
        /// <param name="kycStatus">the kyc status</param>
        /// <returns>                         the protobuf</returns>
        static TokenKycStatus KycStatusToProtobuf(bool kycStatus)
        {
            return kycStatus == null ? TokenKycStatus.KycNotApplicable : kycStatus ? TokenKycStatus.Granted : TokenKycStatus.Revoked;
        }

        /// <summary>
        /// Create a pause status protobuf.
        /// </summary>
        /// <param name="pauseStatus">the pause status</param>
        /// <returns>                         the protobuf</returns>
        static TokenPauseStatus PauseStatusToProtobuf(bool pauseStatus)
        {
            return pauseStatus == null ? TokenPauseStatus.PauseNotApplicable : pauseStatus ? TokenPauseStatus.Paused : TokenPauseStatus.Unpaused;
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        virtual TokenGetInfoResponse ToProtobuf()
        {
            var tokenInfoBuilder = Proto.TokenInfo.NewBuilder().SetTokenId(tokenId.ToProtobuf()).SetName(name).SetSymbol(symbol).SetDecimals(decimals).SetTotalSupply(totalSupply).SetTreasury(treasuryAccountId.ToProtobuf()).SetDefaultFreezeStatus(FreezeStatusToProtobuf(defaultFreezeStatus)).SetDefaultKycStatus(KycStatusToProtobuf(defaultKycStatus)).SetDeleted(isDeleted).SetMemo(tokenMemo).SetTokenType(tokenType.code).SetSupplyType(supplyType.code).SetMaxSupply(maxSupply).SetPauseStatus(PauseStatusToProtobuf(pauseStatus)).SetLedgerId(ledgerId.ToByteString());
            if (adminKey != null)
            {
                tokenInfoBuilder.SetAdminKey(adminKey.ToProtobufKey());
            }

            if (kycKey != null)
            {
                tokenInfoBuilder.SetKycKey(kycKey.ToProtobufKey());
            }

            if (freezeKey != null)
            {
                tokenInfoBuilder.SetFreezeKey(freezeKey.ToProtobufKey());
            }

            if (wipeKey != null)
            {
                tokenInfoBuilder.SetWipeKey(wipeKey.ToProtobufKey());
            }

            if (supplyKey != null)
            {
                tokenInfoBuilder.SetSupplyKey(supplyKey.ToProtobufKey());
            }

            if (feeScheduleKey != null)
            {
                tokenInfoBuilder.SetFeeScheduleKey(feeScheduleKey.ToProtobufKey());
            }

            if (pauseKey != null)
            {
                tokenInfoBuilder.SetPauseKey(pauseKey.ToProtobufKey());
            }

            if (metadata != null)
            {
                tokenInfoBuilder.SetMetadata(ByteString.CopyFrom(metadata));
            }

            if (metadataKey != null)
            {
                tokenInfoBuilder.SetMetadataKey(metadataKey.ToProtobufKey());
            }

            if (autoRenewAccount != null)
            {
                tokenInfoBuilder.SetAutoRenewAccount(autoRenewAccount.ToProtobuf());
            }

            if (autoRenewPeriod != null)
            {
                tokenInfoBuilder.SetAutoRenewPeriod(Utils.DurationConverter.ToProtobuf(autoRenewPeriod));
            }

            if (expirationTime != null)
            {
                tokenInfoBuilder.SetExpiry(TimestampConverter.ToProtobuf(expirationTime));
            }

            foreach (var fee in customFees)
            {
                tokenInfoBuilder.AddCustomFees(fee.ToProtobuf());
            }

            return TokenGetInfoResponse.NewBuilder().SetTokenInfo(tokenInfoBuilder).Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("tokenId", tokenId).Add("name", name).Add("symbol", symbol).Add("decimals", decimals).Add("totalSupply", totalSupply).Add("treasuryAccountId", treasuryAccountId).Add("adminKey", adminKey).Add("kycKey", kycKey).Add("freezeKey", freezeKey).Add("wipeKey", wipeKey).Add("supplyKey", supplyKey).Add("feeScheduleKey", feeScheduleKey).Add("defaultFreezeStatus", defaultFreezeStatus).Add("defaultKycStatus", defaultKycStatus).Add("isDeleted", isDeleted).Add("autoRenewAccount", autoRenewAccount).Add("autoRenewPeriod", autoRenewPeriod).Add("expirationTime", expirationTime).Add("tokenMemo", tokenMemo).Add("customFees", customFees).Add("tokenType", tokenType).Add("supplyType", supplyType).Add("maxSupply", maxSupply).Add("pauseKey", pauseKey).Add("pauseStatus", pauseStatus).Add("metadata", metadata).Add("metadataKey", metadataKey).Add("ledgerId", ledgerId).ToString();
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