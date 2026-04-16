// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenRelationship.cs.xml" path='docs/member[@name="T:TokenRelationship"]/*' />
    public class TokenRelationship
    {
        /// <include file="TokenRelationship.cs.xml" path='docs/member[@name="P:TokenRelationship.TokenId"]/*' />
        public TokenId TokenId { get; }
        /// <include file="TokenRelationship.cs.xml" path='docs/member[@name="P:TokenRelationship.Symbol"]/*' />
        public string Symbol { get; }
        /// <include file="TokenRelationship.cs.xml" path='docs/member[@name="P:TokenRelationship.Balance"]/*' />
        public ulong Balance { get; }
        /// <include file="TokenRelationship.cs.xml" path='docs/member[@name="P:TokenRelationship.KycStatus"]/*' />
        public bool KycStatus { get; }
        /// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.#ctor(TokenId,System.String,System.UInt64,System.Boolean,System.Boolean,System.UInt32,System.Boolean)"]/*' />
        public bool FreezeStatus { get; }
        /// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.#ctor(TokenId,System.String,System.UInt64,System.Boolean,System.Boolean,System.UInt32,System.Boolean)_2"]/*' />
        public uint Decimals { get; }
        /// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.#ctor(TokenId,System.String,System.UInt64,System.Boolean,System.Boolean,System.UInt32,System.Boolean)_3"]/*' />
        public bool AutomaticAssociation { get; }

        internal TokenRelationship(TokenId tokenId, string symbol, ulong balance, bool kycStatus, bool freezeStatus, uint decimals, bool automaticAssociation)
        {
            TokenId = tokenId;
            Symbol = symbol;
            Balance = balance;
            KycStatus = kycStatus;
            FreezeStatus = freezeStatus;
            Decimals = decimals;
            AutomaticAssociation = automaticAssociation;
        }

		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.KycStatusFromProtobuf(Proto.Services.TokenKycStatus)"]/*' />
		public static bool KycStatusFromProtobuf(Proto.Services.TokenKycStatus kycStatus)
		{
			return kycStatus == Proto.Services.TokenKycStatus.Granted;
		}
		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.FreezeStatusFromProtobuf(Proto.Services.TokenFreezeStatus)"]/*' />
		public static bool FreezeStatusFromProtobuf(Proto.Services.TokenFreezeStatus freezeStatus)
        {
            return freezeStatus == Proto.Services.TokenFreezeStatus.Frozen;
        }

		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.FromBytes(System.Byte[])"]/*' />
		public static TokenRelationship FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.TokenRelationship.Parser.ParseFrom(bytes));
		}
		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.FromProtobuf(Proto.Services.TokenRelationship)"]/*' />
		public static TokenRelationship FromProtobuf(Proto.Services.TokenRelationship tokenRelationship)
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

		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}

		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.ToProtobuf"]/*' />
		public virtual Proto.Services.TokenRelationship ToProtobuf()
		{
            return new Proto.Services.TokenRelationship
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
		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.KycStatusToProtobuf(System.Boolean)"]/*' />
		public static Proto.Services.TokenKycStatus KycStatusToProtobuf(bool? kycStatus)
		{
			return kycStatus == null ? Proto.Services.TokenKycStatus.KycNotApplicable : kycStatus.Value ? Proto.Services.TokenKycStatus.Granted : Proto.Services.TokenKycStatus.Revoked;
		}
		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.FreezeStatusToProtobuf(System.Boolean)"]/*' />
		public static Proto.Services.TokenFreezeStatus FreezeStatusToProtobuf(bool? freezeStatus)
        {
            return freezeStatus == null ? Proto.Services.TokenFreezeStatus.FreezeNotApplicable : freezeStatus.Value ? Proto.Services.TokenFreezeStatus.Frozen : Proto.Services.TokenFreezeStatus.Unfrozen;
        }
    }
}
