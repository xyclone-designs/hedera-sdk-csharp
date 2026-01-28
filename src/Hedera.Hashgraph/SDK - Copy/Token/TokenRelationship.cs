// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ids;

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
        public TokenId TokenId { get; }
        /// <summary>
        /// The Symbol of the token
        /// </summary>
        public string Symbol { get; }
        /// <summary>
        /// For token of type FUNGIBLE_COMMON - the balance that the Account holds
        /// in the smallest denomination.
        /// 
        /// For token of type NON_FUNGIBLE_UNIQUE - the number of NFTs held by the
        /// account
        /// </summary>
        public ulong Balance { get; }
        /// <summary>
        /// The KYC status of the account (KycNotApplicable, Granted or Revoked).
        /// 
        /// If the token does not have KYC key, KycNotApplicable is returned
        /// </summary>
        public bool KycStatus { get; }
        /// <summary>
        /// The Freeze status of the account (FreezeNotApplicable, Frozen or
        /// Unfrozen). If the token does not have Freeze key,
        /// FreezeNotApplicable is returned
        /// </summary>
        public bool FreezeStatus { get; }
        /// <summary>
        /// The amount of decimal places that this token supports.
        /// </summary>
        public uint Decimals { get; }
        /// <summary>
        /// Specifies if the relationship is created implicitly.
        /// False : explicitly associated,
        /// True : implicitly associated.
        /// </summary>
        public bool AutomaticAssociation { get; }

        TokenRelationship(TokenId tokenId, string symbol, ulong balance, bool kycStatus, bool freezeStatus, uint decimals, bool automaticAssociation)
        {
            TokenId = tokenId;
            Symbol = symbol;
            Balance = balance;
            KycStatus = kycStatus;
            FreezeStatus = freezeStatus;
            Decimals = decimals;
            AutomaticAssociation = automaticAssociation;
        }

		/// <summary>
		/// Retrieve the kyc status from a protobuf.
		/// </summary>
		/// <param name="kycStatus">the protobuf</param>
		/// <returns>                         the kyc status</returns>
		public static bool KycStatusFromProtobuf(Proto.TokenKycStatus kycStatus)
		{
			return kycStatus == Proto.TokenKycStatus.Granted;
		}
		/// <summary>
		/// Retrieve freeze status from a protobuf.
		/// </summary>
		/// <param name="freezeStatus">the protobuf</param>
		/// <returns>                         the freeze status</returns>
		public static bool FreezeStatusFromProtobuf(Proto.TokenFreezeStatus freezeStatus)
        {
            return freezeStatus == Proto.TokenFreezeStatus.Frozen;
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
		/// Create a token relationship object from a protobuf.
		/// </summary>
		/// <param name="tokenRelationship">the protobuf</param>
		/// <returns>                         the new token relationship</returns>
		public static TokenRelationship FromProtobuf(Proto.TokenRelationship tokenRelationship)
        {
            return new TokenRelationship(
                TokenId.FromProtobuf(tokenRelationship.TokenId), 
                tokenRelationship.Symbol, 
                tokenRelationship.Balance, 
                KycStatusFromProtobuf(tokenRelationship.KycStatus), 
                FreezeStatusFromProtobuf(tokenRelationship.FreezeStatus), 
                tokenRelationship.Decimals, 
                tokenRelationship.AutomaticAssociation);
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
		public virtual Proto.TokenRelationship ToProtobuf()
		{
            return new Proto.TokenRelationship
            {
				TokenId = TokenId.ToProtobuf(),
				Symbol = Symbol,
				Balance = Balance,
				KycStatus = KycStatusToProtobuf(KycStatus),
				FreezeStatus = FreezeStatusToProtobuf(FreezeStatus),
				Decimals = Decimals,
				AutomaticAssociation = AutomaticAssociation,
			};
		}
		/// <summary>
		/// Retrieve the kyc status from a protobuf.
		/// </summary>
		/// <param name="kycStatus">the protobuf</param>
		/// <returns>                         the kyc status</returns>
		public static Proto.TokenKycStatus KycStatusToProtobuf(bool? kycStatus)
		{
			return kycStatus == null ? Proto.TokenKycStatus.KycNotApplicable : kycStatus.Value ? Proto.TokenKycStatus.Granted : Proto.TokenKycStatus.Revoked;
		}
		/// <summary>
		/// Retrieve the freeze status from a protobuf.
		/// </summary>
		/// <param name="freezeStatus">the protobuf</param>
		/// <returns>                         the freeze status</returns>
		public static Proto.TokenFreezeStatus FreezeStatusToProtobuf(bool? freezeStatus)
        {
            return freezeStatus == null ? Proto.TokenFreezeStatus.FreezeNotApplicable : freezeStatus.Value ? Proto.TokenFreezeStatus.Frozen : Proto.TokenFreezeStatus.Unfrozen;
        }
    }
}