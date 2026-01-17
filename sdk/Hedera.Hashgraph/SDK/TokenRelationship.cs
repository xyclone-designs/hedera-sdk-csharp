using System;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Token's information related to the given Account.
 *
 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/tokenrelationship">Hedera Documentation</a>
 */
	public class TokenRelationship
	{
		/**
		 * A unique token id
		 */
		public readonly TokenId tokenId;
    /**
     * The Symbol of the token
     */
    public readonly string symbol;
    /**
     * For token of type FUNGIBLE_COMMON - the balance that the Account holds
     * in the smallest denomination.
     *
     * For token of type NON_FUNGIBLE_UNIQUE - the number of NFTs held by the
     * account
     */
    public readonly long balance;
		/**
		 * The KYC status of the account (KycNotApplicable, Granted or Revoked).
		 *
		 * If the token does not have KYC key, KycNotApplicable is returned
		 */
		@Nullable
		public readonly Boolean kycStatus;
    /**
     * The Freeze status of the account (FreezeNotApplicable, Frozen or
     * Unfrozen). If the token does not have Freeze key,
     * FreezeNotApplicable is returned
     */
    @Nullable
		public readonly Boolean freezeStatus;
    /**
     * The amount of decimal places that this token supports.
     */
    public readonly int decimals;
		/**
		 * Specifies if the relationship is created implicitly.
		 * False : explicitly associated,
		 * True : implicitly associated.
		 */
		public readonly bool automaticAssociation;

    TokenRelationship(
			TokenId tokenId,
			string symbol,
			long balance,
			@Nullable Boolean kycStatus,
			@Nullable Boolean freezeStatus,
			int decimals,
			bool automaticAssociation)
		{
			this.tokenId = tokenId;
			this.symbol = symbol;
			this.balance = balance;
			this.kycStatus = kycStatus;
			this.freezeStatus = freezeStatus;
			this.decimals = decimals;
			this.automaticAssociation = automaticAssociation;
		}

		/**
		 * Retrieve freeze status from a protobuf.
		 *
		 * @param freezeStatus              the protobuf
		 * @return                          the freeze status
		 */
		@Nullable
		static Boolean freezeStatusFromProtobuf(TokenFreezeStatus freezeStatus)
		{
			return freezeStatus == TokenFreezeStatus.FreezeNotApplicable ? null : freezeStatus == TokenFreezeStatus.Frozen;
		}

		/**
		 * Retrieve the kyc status from a protobuf.
		 *
		 * @param kycStatus                 the protobuf
		 * @return                          the kyc status
		 */
		@Nullable
		static Boolean kycStatusFromProtobuf(TokenKycStatus kycStatus)
		{
			return kycStatus == TokenKycStatus.KycNotApplicable ? null : kycStatus == TokenKycStatus.Granted;
		}

		/**
		 * Create a token relationship object from a protobuf.
		 *
		 * @param tokenRelationship         the protobuf
		 * @return                          the new token relationship
		 */
		static TokenRelationship FromProtobuf(Proto.TokenRelationship tokenRelationship)
		{
			return new TokenRelationship(
					TokenId.FromProtobuf(tokenRelationship.getTokenId()),
					tokenRelationship.getSymbol(),
					tokenRelationship.getBalance(),
					kycStatusFromProtobuf(tokenRelationship.getKycStatus()),
					freezeStatusFromProtobuf(tokenRelationship.getFreezeStatus()),
					tokenRelationship.getDecimals(),
					tokenRelationship.getAutomaticAssociation());
		}

		/**
		 * Create a token relationship from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new token relationship
		 * @       when there is an issue with the protobuf
		 */
		public static TokenRelationship FromBytes(byte[] bytes) 
		{
        return FromProtobuf(Proto.TokenRelationship.Parser.ParseFrom(bytes).toBuilder()
	                .build());
    }

    /**
     * Retrieve the freeze status from a protobuf.
     *
     * @param freezeStatus              the protobuf
     * @return                          the freeze status
     */
    static TokenFreezeStatus freezeStatusToProtobuf(@Nullable Boolean freezeStatus)
		{
			return freezeStatus == null
					? TokenFreezeStatus.FreezeNotApplicable
					: freezeStatus ? TokenFreezeStatus.Frozen : TokenFreezeStatus.Unfrozen;
		}

		/**
		 * Retrieve the kyc status from a protobuf.
		 *
		 * @param kycStatus                 the protobuf
		 * @return                          the kyc status
		 */
		static TokenKycStatus kycStatusToProtobuf(@Nullable Boolean kycStatus)
		{
			return kycStatus == null
					? TokenKycStatus.KycNotApplicable
					: kycStatus ? TokenKycStatus.Granted : TokenKycStatus.Revoked;
		}

		/**
		 * Create the protobuf.
		 *
		 * @return                          the protobuf representation
		 */
		Proto.TokenRelationship ToProtobuf()
		{
			return Proto.TokenRelationship.newBuilder()
					.setTokenId(tokenId.ToProtobuf())
					.setSymbol(symbol)
					.setBalance(balance)
					.setKycStatus(kycStatusToProtobuf(kycStatus))
					.setFreezeStatus(freezeStatusToProtobuf(freezeStatus))
					.setDecimals(decimals)
					.setAutomaticAssociation(automaticAssociation)
					.build();
		}


	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("tokenId", tokenId)
				.Add("symbol", symbol)
				.Add("balance", balance)
				.Add("kycStatus", kycStatus)
				.Add("freezeStatus", freezeStatus)
				.Add("decimals", decimals)
				.Add("automaticAssociation", automaticAssociation)
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