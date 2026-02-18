// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Utils;

using System.Collections.Generic;
using System.Linq;

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
        public readonly TokenId TokenId;
        /// <summary>
        /// Name of token.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Symbol of token.
        /// </summary>
        public readonly string Symbol;
        /// <summary>
        /// The amount of decimal places that this token supports.
        /// </summary>
        public readonly uint Decimals;
        /// <summary>
        /// Total Supply of token.
        /// </summary>
        public readonly ulong TotalSupply;
        /// <summary>
        /// The ID of the account which is set as Treasury
        /// </summary>
        public readonly AccountId TreasuryAccountId;
        /// <summary>
        /// The key which can perform update/delete operations on the token. If empty, the token can be perceived as immutable (not being able to be updated/deleted)
        /// </summary>
        public readonly Key? AdminKey;
        /// <summary>
        /// The key which can grant or revoke KYC of an account for the token's transactions. If empty, KYC is not required, and KYC grant or revoke operations are not possible.
        /// </summary>
        public readonly Key? KycKey;
        /// <summary>
        /// The key which can freeze or unfreeze an account for token transactions. If empty, freezing is not possible
        /// </summary>
        public readonly Key? FreezeKey;
        /// <summary>
        /// The key which can wipe token balance of an account. If empty, wipe is not possible
        /// </summary>
        public readonly Key? WipeKey;
        /// <summary>
        /// The key which can change the supply of a token. The key is used to sign Token Mint/Burn operations
        /// </summary>
        public readonly Key? SupplyKey;
        /// <summary>
        /// The key which can change the custom fees of the token; if not set, the fees are immutable
        /// </summary>
        public readonly Key? FeeScheduleKey;
        /// <summary>
        /// The default Freeze status (not applicable, frozen or unfrozen) of Hedera accounts relative to this token. FreezeNotApplicable is returned if Token Freeze Key is empty. Frozen is returned if Token Freeze Key is set and defaultFreeze is set to true. Unfrozen is returned if Token Freeze Key is set and defaultFreeze is set to false
        /// </summary>
        public readonly bool DefaultFreezeStatus;
        /// <summary>
        /// The default KYC status (KycNotApplicable or Revoked) of Hedera accounts relative to this token. KycNotApplicable is returned if KYC key is not set, otherwise Revoked
        /// </summary>
        public readonly bool DefaultKycStatus;
        /// <summary>
        /// Specifies whether the token was deleted or not
        /// </summary>
        public readonly bool IsDeleted;
        /// <summary>
        /// An account which will be automatically charged to renew the token's expiration, at autoRenewPeriod interval
        /// </summary>
        public readonly AccountId AutoRenewAccount;
        /// <summary>
        /// The interval at which the auto-renew account will be charged to extend the token's expiry
        /// </summary>
        public readonly Duration AutoRenewPeriod;
        /// <summary>
        /// The epoch second at which the token will expire
        /// </summary>
        public readonly Timestamp ExpirationTime;
        /// <summary>
        /// The memo associated with the token
        /// </summary>
        public readonly string TokenMemo;
        /// <summary>
        /// The custom fees to be assessed during a CryptoTransfer that transfers units of this token
        /// </summary>
        public readonly List<CustomFee> CustomFees;
        /// <summary>
        /// The token type
        /// </summary>
        public readonly TokenType TokenType;
        /// <summary>
        /// The token supply type
        /// </summary>
        public readonly TokenSupplyType SupplyType;
        /// <summary>
        /// For tokens of type FUNGIBLE_COMMON - The Maximum number of fungible tokens that can be in
        /// circulation. For tokens of type NonFungibleUnique - the maximum number of NFTs (serial
        /// numbers) that can be in circulation
        /// </summary>
        public readonly long MaxSupply;
        /// <summary>
        /// The Key which can pause and unpause the Token.
        /// </summary>
        public readonly Key? PauseKey;
        /// <summary>
        /// Specifies whether the token is paused or not. Null if pauseKey is not set.
        /// </summary>
        public readonly bool PauseStatus;
        /// <summary>
        /// Represents the metadata of the token definition.
        /// </summary>
        public byte[] Metadata = [];
        /// <summary>
        /// The key which can change the metadata of a token
        /// (token definition and individual NFTs).
        /// </summary>
        public readonly Key? MetadataKey;
        /// <summary>
        /// The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
        /// </summary>
        public readonly LedgerId LedgerId;
        
        TokenInfo(TokenId tokenId, string name, string symbol, uint decimals, ulong totalSupply, AccountId treasuryAccountId, Key? adminKey, Key? kycKey, Key? freezeKey, Key? wipeKey, Key? supplyKey, Key? feeScheduleKey, bool defaultFreezeStatus, bool defaultKycStatus, bool isDeleted, AccountId autoRenewAccount, Duration autoRenewPeriod, Timestamp expirationTime, string tokenMemo, IList<CustomFee> customFees, TokenType tokenType, TokenSupplyType supplyType, long maxSupply, Key? pauseKey, bool pauseStatus, byte[] metadata, Key? metadataKey, LedgerId ledgerId)
        {
            TokenId = tokenId;
            Name = name;
            Symbol = symbol;
            Decimals = decimals;
            TotalSupply = totalSupply;
            TreasuryAccountId = treasuryAccountId;
            AdminKey = adminKey;
            KycKey = kycKey;
            FreezeKey = freezeKey;
            WipeKey = wipeKey;
            SupplyKey = supplyKey;
            FeeScheduleKey = feeScheduleKey;
            DefaultFreezeStatus = defaultFreezeStatus;
            DefaultKycStatus = defaultKycStatus;
            IsDeleted = isDeleted;
            AutoRenewAccount = autoRenewAccount;
            AutoRenewPeriod = autoRenewPeriod;
            ExpirationTime = expirationTime;
            TokenMemo = tokenMemo;
            CustomFees = customFees;
            TokenType = tokenType;
            SupplyType = supplyType;
            MaxSupply = maxSupply;
            PauseKey = pauseKey;
            PauseStatus = pauseStatus;
            Metadata = metadata;
            MetadataKey = metadataKey;
            LedgerId = ledgerId;
        }

		/// <summary>
		/// Is kyc required?
		/// </summary>
		/// <param name="kycStatus">the kyc status</param>
		/// <returns>                         true / false / null</returns>
		public static bool KycStatusFromProtobuf(Proto.TokenKycStatus kycStatus)
        {
            return kycStatus == Proto.TokenKycStatus.Granted;
        }
		/// <summary>
		/// Are we paused?
		/// </summary>
		/// <param name="pauseStatus">the paused status</param>
		/// <returns>                         true / false / null</returns>
		public static bool PauseStatusFromProtobuf(Proto.TokenPauseStatus pauseStatus)
		{
			return pauseStatus == Proto.TokenPauseStatus.Paused;
		}
		/// <summary>
		/// Are we frozen?
		/// </summary>
		/// <param name="freezeStatus">the freeze status</param>
		/// <returns>                         true / false / null</returns>
		public static bool FreezeStatusFromProtobuf(Proto.TokenFreezeStatus freezeStatus)
		{
			return freezeStatus == Proto.TokenFreezeStatus.Frozen;
		}

		/// <summary>
		/// Create a token info object from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new token info object</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static TokenInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TokenGetInfoResponse.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a token info object from a protobuf.
		/// </summary>
		/// <param name="response">the protobuf</param>
		/// <returns>                         new token info object</returns>
		public static TokenInfo FromProtobuf(Proto.TokenGetInfoResponse response)
        {
			return new TokenInfo(
                TokenId.FromProtobuf(response.TokenInfo.TokenId),
				response.TokenInfo.Name,
				response.TokenInfo.Symbol,
				response.TokenInfo.Decimals,
				response.TokenInfo.TotalSupply, 
                AccountId.FromProtobuf(response.TokenInfo.Treasury),
				Key.FromProtobufKey(response.TokenInfo.AdminKey),
				Key.FromProtobufKey(response.TokenInfo.KycKey),
				Key.FromProtobufKey(response.TokenInfo.FreezeKey),
				Key.FromProtobufKey(response.TokenInfo.WipeKey),
				Key.FromProtobufKey(response.TokenInfo.SupplyKey),
				Key.FromProtobufKey(response.TokenInfo.FeeScheduleKey), 
                FreezeStatusFromProtobuf(response.TokenInfo.DefaultFreezeStatus), 
                KycStatusFromProtobuf(response.TokenInfo.DefaultKycStatus),
				response.TokenInfo.Deleted,
				AccountId.FromProtobuf(response.TokenInfo.AutoRenewAccount),
				DurationConverter.FromProtobuf(response.TokenInfo.AutoRenewPeriod),
				TimestampConverter.FromProtobuf(response.TokenInfo.Expiry),
				response.TokenInfo.Memo, 
                CustomFeesFromProto(response.TokenInfo), 
                (TokenType)response.TokenInfo.TokenType, 
                (TokenSupplyType)response.TokenInfo.SupplyType,
				response.TokenInfo.MaxSupply,
				Key.FromProtobufKey(response.TokenInfo.PauseKey), 
                PauseStatusFromProtobuf(response.TokenInfo.PauseStatus),
				response.TokenInfo.Metadata.ToByteArray(),
				Key.FromProtobufKey(response.TokenInfo.MetadataKey), 
                LedgerId.FromByteString(response.TokenInfo.LedgerId));
		}

        /// <summary>
        /// Create custom fee list from protobuf.
        /// </summary>
        /// <param name="info">the protobuf</param>
        /// <returns>                         the list of custom fee's</returns>
        private static IList<CustomFee> CustomFeesFromProto(Proto.TokenInfo info)
        {
            return [.. info.CustomFees.Select(_ => CustomFee.FromProtobuf(_))];
        }

		/// <summary>
		/// Create the byte array.
		/// </summary>
		/// <returns>                         the byte array representation</returns>
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <summary>
		/// Create the protobuf.
		/// </summary>
		/// <returns>                         the protobuf representation</returns>
		public virtual Proto.TokenGetInfoResponse ToProtobuf()
		{
            Proto.TokenInfo proto = new()
            {
				AutoRenewAccount = AutoRenewAccount.ToProtobuf(),
				AutoRenewPeriod = DurationConverter.ToProtobuf(AutoRenewPeriod),
				Decimals = Decimals,
				DefaultFreezeStatus = FreezeStatusToProtobuf(DefaultFreezeStatus),
				DefaultKycStatus = KycStatusToProtobuf(DefaultKycStatus),
				Deleted = IsDeleted,
				Expiry = TimestampConverter.ToProtobuf(ExpirationTime),
				LedgerId = LedgerId.ToByteString(),
				PauseStatus = PauseStatusToProtobuf(PauseStatus),
				MaxSupply = MaxSupply,
				Memo = TokenMemo,
				Metadata = ByteString.CopyFrom(Metadata),
				Name = Name,
				TokenId = TokenId.ToProtobuf(),
				TotalSupply = TotalSupply,
				TokenType = (Proto.TokenType)TokenType,
				Treasury = TreasuryAccountId.ToProtobuf(),
				SupplyType = (Proto.TokenSupplyType)SupplyType,
				Symbol = Symbol,
			};

            if (AdminKey is not null) 
                proto.AdminKey = AdminKey.ToProtobufKey();
            if (FeeScheduleKey is not null) 
                proto.FeeScheduleKey = FeeScheduleKey.ToProtobufKey();
            if (FreezeKey is not null) 
                proto.FreezeKey = FreezeKey.ToProtobufKey();
            if (KycKey is not null) 
                proto.KycKey = KycKey.ToProtobufKey();
            if (PauseKey is not null) 
                proto.PauseKey = PauseKey.ToProtobufKey();
            if (MetadataKey is not null) 
                proto.MetadataKey = MetadataKey.ToProtobufKey();
            if (SupplyKey is not null) 
                proto.SupplyKey = SupplyKey.ToProtobufKey();
            if (WipeKey is not null) 
                proto.WipeKey = WipeKey.ToProtobufKey();

			proto.CustomFees.AddRange(CustomFees.Select(_ => _.ToProtobuf()));

			return new Proto.TokenGetInfoResponse
            {
                TokenInfo = proto
            };
		}
		/// <summary>
		/// Create a kyc status protobuf.
		/// </summary>
		/// <param name="kycStatus">the kyc status</param>
		/// <returns>                         the protobuf</returns>
		public static Proto.TokenKycStatus KycStatusToProtobuf(bool kycStatus)
        {
            return kycStatus ? Proto.TokenKycStatus.Granted : Proto.TokenKycStatus.Revoked;
        }
        /// <summary>
        /// Create a pause status protobuf.
        /// </summary>
        /// <param name="pauseStatus">the pause status</param>
        /// <returns>                         the protobuf</returns>
        public static Proto.TokenPauseStatus PauseStatusToProtobuf(bool pauseStatus)
        {
            return pauseStatus ? Proto.TokenPauseStatus.Paused : Proto.TokenPauseStatus.Unpaused;
        }
		/// <summary>
		/// Create a token freeze status protobuf.
		/// </summary>
		/// <param name="freezeStatus">the freeze status</param>
		/// <returns>                         the protobuf</returns>
		public static Proto.TokenFreezeStatus FreezeStatusToProtobuf(bool freezeStatus)
		{
			return freezeStatus ? Proto.TokenFreezeStatus.Frozen : Proto.TokenFreezeStatus.Unfrozen;
		}
    }
}