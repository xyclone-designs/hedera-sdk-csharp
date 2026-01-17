namespace Hedera.Hashgraph.SDK
{
	/**
 * Gets information about a fungible or non-fungible token instance.
 *
 * See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/get-token-info">Hedera Documentation</a>
 */
	public class TokenInfo
	{
		/**
		 * The ID of the token for which information is requested.
		 */
		public readonly TokenId tokenId;

    /**
     * Name of token.
     */
    public readonly string name;

    /**
     * Symbol of token.
     */
    public readonly string symbol;

    /**
     * The amount of decimal places that this token supports.
     */
    public readonly int decimals;

		/**
		 * Total Supply of token.
		 */
		public readonly long totalSupply;

		/**
		 * The ID of the account which is set as Treasury
		 */
		public readonly AccountId treasuryAccountId;

    /**
     * The key which can perform update/delete operations on the token. If empty, the token can be perceived as immutable (not being able to be updated/deleted)
     */
    @Nullable
		public readonly Key adminKey;

    /**
     * The key which can grant or revoke KYC of an account for the token's transactions. If empty, KYC is not required, and KYC grant or revoke operations are not possible.
     */
    @Nullable
		public readonly Key kycKey;

    /**
     * The key which can freeze or unfreeze an account for token transactions. If empty, freezing is not possible
     */
    @Nullable
		public readonly Key freezeKey;

    /**
     * The key which can wipe token balance of an account. If empty, wipe is not possible
     */
    @Nullable
		public readonly Key wipeKey;

    /**
     * The key which can change the supply of a token. The key is used to sign Token Mint/Burn operations
     */
    @Nullable
		public readonly Key supplyKey;

    /**
     * The key which can change the custom fees of the token; if not set, the fees are immutable
     */
    @Nullable
		public readonly Key feeScheduleKey;

    /**
     * The default Freeze status (not applicable, frozen or unfrozen) of Hedera accounts relative to this token. FreezeNotApplicable is returned if Token Freeze Key is empty. Frozen is returned if Token Freeze Key is set and defaultFreeze is set to true. Unfrozen is returned if Token Freeze Key is set and defaultFreeze is set to false
     */
    @Nullable
		public readonly Boolean defaultFreezeStatus;

    /**
     * The default KYC status (KycNotApplicable or Revoked) of Hedera accounts relative to this token. KycNotApplicable is returned if KYC key is not set, otherwise Revoked
     */
    @Nullable
		public readonly Boolean defaultKycStatus;

    /**
     * Specifies whether the token was deleted or not
     */
    public readonly bool isDeleted;

    /**
     * An account which will be automatically charged to renew the token's expiration, at autoRenewPeriod interval
     */
    @Nullable
		public readonly AccountId autoRenewAccount;

    /**
     * The interval at which the auto-renew account will be charged to extend the token's expiry
     */
    @Nullable
		public readonly Duration autoRenewPeriod;

    /**
     * The epoch second at which the token will expire
     */
    @Nullable
		public readonly DateTimeOffset expirationTime;

    /**
     * The memo associated with the token
     */
    public readonly string tokenMemo;

    /**
     * The custom fees to be assessed during a CryptoTransfer that transfers units of this token
     */
    public readonly List<CustomFee> customFees;

		/**
		 * The token type
		 */
		public readonly TokenType tokenType;

    /**
     * The token supply type
     */
    public readonly TokenSupplyType supplyType;

    /**
     * For tokens of type FUNGIBLE_COMMON - The Maximum number of fungible tokens that can be in
     * circulation. For tokens of type NON_FUNGIBLE_UNIQUE - the maximum number of NFTs (serial
     * numbers) that can be in circulation
     */
    public readonly long maxSupply;

		/**
		 * The Key which can pause and unpause the Token.
		 */
		@Nullable
		public readonly Key pauseKey;

    /**
     * Specifies whether the token is paused or not. Null if pauseKey is not set.
     */
    @Nullable
		public readonly Boolean pauseStatus;

    /**
     * Represents the metadata of the token definition.
     */
    public byte[] metadata = { };

		/**
		 * The key which can change the metadata of a token
		 * (token definition and individual NFTs).
		 */
		@Nullable
		public readonly Key metadataKey;

    /**
     * The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
     */
    public readonly LedgerId ledgerId;

    TokenInfo(
			TokenId tokenId,
			string name,
			string symbol,
			int decimals,
			long totalSupply,
			AccountId treasuryAccountId,
			@Nullable Key adminKey,
			@Nullable Key kycKey,
			@Nullable Key freezeKey,
			@Nullable Key wipeKey,
			@Nullable Key supplyKey,
			@Nullable Key feeScheduleKey,
			@Nullable Boolean defaultFreezeStatus,
			@Nullable Boolean defaultKycStatus,
			bool isDeleted,
			@Nullable AccountId autoRenewAccount,
			@Nullable Duration autoRenewPeriod,
			@Nullable DateTimeOffset expirationTime,
			string tokenMemo,
			List<CustomFee> customFees,
			TokenType tokenType,
			TokenSupplyType supplyType,
			long maxSupply,
			@Nullable Key pauseKey,
			@Nullable Boolean pauseStatus,
			byte[] metadata,
			@Nullable Key metadataKey,
			LedgerId ledgerId)
		{
			this.tokenId = tokenId;
			this.name = name;
			this.symbol = symbol;
			this.decimals = decimals;
			this.totalSupply = totalSupply;
			this.treasuryAccountId = treasuryAccountId;
			this.adminKey = adminKey;
			this.kycKey = kycKey;
			this.freezeKey = freezeKey;
			this.wipeKey = wipeKey;
			this.supplyKey = supplyKey;
			this.feeScheduleKey = feeScheduleKey;
			this.defaultFreezeStatus = defaultFreezeStatus;
			this.defaultKycStatus = defaultKycStatus;
			this.isDeleted = isDeleted;
			this.autoRenewAccount = autoRenewAccount;
			this.autoRenewPeriod = autoRenewPeriod;
			this.expirationTime = expirationTime;
			this.tokenMemo = tokenMemo;
			this.customFees = customFees;
			this.tokenType = tokenType;
			this.supplyType = supplyType;
			this.maxSupply = maxSupply;
			this.pauseKey = pauseKey;
			this.pauseStatus = pauseStatus;
			this.metadata = metadata;
			this.metadataKey = metadataKey;
			this.ledgerId = ledgerId;
		}

		/**
		 * Are we frozen?
		 *
		 * @param freezeStatus              the freeze status
		 * @return                          true / false / null
		 */
		@Nullable
		static Boolean freezeStatusFromProtobuf(TokenFreezeStatus freezeStatus)
		{
			return freezeStatus == TokenFreezeStatus.FreezeNotApplicable ? null : freezeStatus == TokenFreezeStatus.Frozen;
		}

		/**
		 * Is kyc required?
		 *
		 * @param kycStatus                 the kyc status
		 * @return                          true / false / null
		 */
		@Nullable
		static Boolean kycStatusFromProtobuf(TokenKycStatus kycStatus)
		{
			return kycStatus == TokenKycStatus.KycNotApplicable ? null : kycStatus == TokenKycStatus.Granted;
		}

		/**
		 * Are we paused?
		 *
		 * @param pauseStatus               the paused status
		 * @return                          true / false / null
		 */
		@Nullable
		static Boolean pauseStatusFromProtobuf(TokenPauseStatus pauseStatus)
		{
			return pauseStatus == TokenPauseStatus.PauseNotApplicable ? null : pauseStatus == TokenPauseStatus.Paused;
		}

		/**
		 * Create a token info object from a protobuf.
		 *
		 * @param response                  the protobuf
		 * @return                          new token info object
		 */
		static TokenInfo FromProtobuf(TokenGetInfoResponse response)
		{
			var info = response.getTokenInfo();

			return new TokenInfo(
					TokenId.FromProtobuf(info.getTokenId()),
					info.getName(),
					info.getSymbol(),
					info.getDecimals(),
					info.getTotalSupply(),
					AccountId.FromProtobuf(info.getTreasury()),
					info.hasAdminKey() ? Key.FromProtobufKey(info.getAdminKey()) : null,
					info.hasKycKey() ? Key.FromProtobufKey(info.getKycKey()) : null,
					info.hasFreezeKey() ? Key.FromProtobufKey(info.getFreezeKey()) : null,
					info.hasWipeKey() ? Key.FromProtobufKey(info.getWipeKey()) : null,
					info.hasSupplyKey() ? Key.FromProtobufKey(info.getSupplyKey()) : null,
					info.hasFeeScheduleKey() ? Key.FromProtobufKey(info.getFeeScheduleKey()) : null,
					freezeStatusFromProtobuf(info.getDefaultFreezeStatus()),
					kycStatusFromProtobuf(info.getDefaultKycStatus()),
					info.getDeleted(),
					info.hasAutoRenewAccount() ? AccountId.FromProtobuf(info.getAutoRenewAccount()) : null,
					info.hasAutoRenewPeriod() ? DurationConverter.FromProtobuf(info.getAutoRenewPeriod()) : null,
					info.hasExpiry() ? DateTimeOffsetConverter.FromProtobuf(info.getExpiry()) : null,
					info.getMemo(),
					customFeesFromProto(info),
					TokenType.valueOf(info.getTokenType()),
					TokenSupplyType.valueOf(info.getSupplyType()),
					info.getMaxSupply(),
					info.hasPauseKey() ? Key.FromProtobufKey(info.getPauseKey()) : null,
					pauseStatusFromProtobuf(info.getPauseStatus()),
					info.getMetadata().ToByteArray(),
					info.hasMetadataKey() ? Key.FromProtobufKey(info.getMetadataKey()) : null,
					LedgerId.FromByteString(info.getLedgerId()));
		}

		/**
		 * Create a token info object from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new token info object
		 * @       when there is an issue with the protobuf
		 */
		public static TokenInfo FromBytes(byte[] bytes) 
		{
        return FromProtobuf(TokenGetInfoResponse.Parser.ParseFrom(bytes));
    }

    /**
     * Create custom fee list from protobuf.
     *
     * @param info                      the protobuf
     * @return                          the list of custom fee's
     */
    private static List<CustomFee> customFeesFromProto(Proto.TokenInfo info)
		{
			var returnCustomFees = new ArrayList<CustomFee>(info.getCustomFeesCount());
			for (var feeProto : info.getCustomFeesList())
			{
				returnCustomFees.Add(CustomFee.FromProtobuf(feeProto));
			}
			return returnCustomFees;
		}

		/**
		 * Create a token freeze status protobuf.
		 *
		 * @param freezeStatus              the freeze status
		 * @return                          the protobuf
		 */
		static TokenFreezeStatus freezeStatusToProtobuf(@Nullable Boolean freezeStatus)
		{
			return freezeStatus == null
					? TokenFreezeStatus.FreezeNotApplicable
					: freezeStatus ? TokenFreezeStatus.Frozen : TokenFreezeStatus.Unfrozen;
		}

		/**
		 * Create a kyc status protobuf.
		 *
		 * @param kycStatus                 the kyc status
		 * @return                          the protobuf
		 */
		static TokenKycStatus kycStatusToProtobuf(@Nullable Boolean kycStatus)
		{
			return kycStatus == null
					? TokenKycStatus.KycNotApplicable
					: kycStatus ? TokenKycStatus.Granted : TokenKycStatus.Revoked;
		}

		/**
		 * Create a pause status protobuf.
		 *
		 * @param pauseStatus               the pause status
		 * @return                          the protobuf
		 */
		static TokenPauseStatus pauseStatusToProtobuf(@Nullable Boolean pauseStatus)
		{
			return pauseStatus == null
					? TokenPauseStatus.PauseNotApplicable
					: pauseStatus ? TokenPauseStatus.Paused : TokenPauseStatus.Unpaused;
		}

		/**
		 * Create the protobuf.
		 *
		 * @return                          the protobuf representation
		 */
		TokenGetInfoResponse ToProtobuf()
		{
			var tokenInfoBuilder = Proto.TokenInfo.newBuilder()
					.setTokenId(tokenId.ToProtobuf())
					.setName(name)
					.setSymbol(symbol)
					.setDecimals(decimals)
					.setTotalSupply(totalSupply)
					.setTreasury(treasuryAccountId.ToProtobuf())
					.setDefaultFreezeStatus(freezeStatusToProtobuf(defaultFreezeStatus))
					.setDefaultKycStatus(kycStatusToProtobuf(defaultKycStatus))
					.setDeleted(isDeleted)
					.setMemo(tokenMemo)
					.setTokenType(tokenType.code)
					.setSupplyType(supplyType.code)
					.setMaxSupply(maxSupply)
					.setPauseStatus(pauseStatusToProtobuf(pauseStatus))
					.setLedgerId(ledgerId.toByteString());
			if (adminKey != null)
			{
				tokenInfoBuilder.setAdminKey(adminKey.ToProtobufKey());
			}
			if (kycKey != null)
			{
				tokenInfoBuilder.setKycKey(kycKey.ToProtobufKey());
			}
			if (freezeKey != null)
			{
				tokenInfoBuilder.setFreezeKey(freezeKey.ToProtobufKey());
			}
			if (wipeKey != null)
			{
				tokenInfoBuilder.setWipeKey(wipeKey.ToProtobufKey());
			}
			if (supplyKey != null)
			{
				tokenInfoBuilder.setSupplyKey(supplyKey.ToProtobufKey());
			}
			if (feeScheduleKey != null)
			{
				tokenInfoBuilder.setFeeScheduleKey(feeScheduleKey.ToProtobufKey());
			}
			if (pauseKey != null)
			{
				tokenInfoBuilder.setPauseKey(pauseKey.ToProtobufKey());
			}
			if (metadata != null)
			{
				tokenInfoBuilder.setMetadata(ByteString.copyFrom(metadata));
			}
			if (metadataKey != null)
			{
				tokenInfoBuilder.setMetadataKey(metadataKey.ToProtobufKey());
			}
			if (autoRenewAccount != null)
			{
				tokenInfoBuilder.setAutoRenewAccount(autoRenewAccount.ToProtobuf());
			}
			if (autoRenewPeriod != null)
			{
				tokenInfoBuilder.setAutoRenewPeriod(DurationConverter.ToProtobuf(autoRenewPeriod));
			}
			if (expirationTime != null)
			{
				tokenInfoBuilder.setExpiry(DateTimeOffsetConverter.ToProtobuf(expirationTime));
			}
			for (var fee : customFees)
			{
				tokenInfoBuilder.AddCustomFees(fee.ToProtobuf());
			}
			return TokenGetInfoResponse.newBuilder().setTokenInfo(tokenInfoBuilder).build();
		}


	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("tokenId", tokenId)
				.Add("name", name)
				.Add("symbol", symbol)
				.Add("decimals", decimals)
				.Add("totalSupply", totalSupply)
				.Add("treasuryAccountId", treasuryAccountId)
				.Add("adminKey", adminKey)
				.Add("kycKey", kycKey)
				.Add("freezeKey", freezeKey)
				.Add("wipeKey", wipeKey)
				.Add("supplyKey", supplyKey)
				.Add("feeScheduleKey", feeScheduleKey)
				.Add("defaultFreezeStatus", defaultFreezeStatus)
				.Add("defaultKycStatus", defaultKycStatus)
				.Add("isDeleted", isDeleted)
				.Add("autoRenewAccount", autoRenewAccount)
				.Add("autoRenewPeriod", autoRenewPeriod)
				.Add("expirationTime", expirationTime)
				.Add("tokenMemo", tokenMemo)
				.Add("customFees", customFees)
				.Add("tokenType", tokenType)
				.Add("supplyType", supplyType)
				.Add("maxSupply", maxSupply)
				.Add("pauseKey", pauseKey)
				.Add("pauseStatus", pauseStatus)
				.Add("metadata", metadata)
				.Add("metadataKey", metadataKey)
				.Add("ledgerId", ledgerId)
				.toString();
	}

	/**
     * Create the byte array.
     *
     * @return                          the byte array representation
     */
	public byte[] ToBytes()
	{
		return ToProtobuf().ToByteArray();
	}
}

}