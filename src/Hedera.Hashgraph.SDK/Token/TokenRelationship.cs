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

		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.KycStatusFromProtobuf(Proto.TokenKycStatus)"]/*' />
		public static bool KycStatusFromProtobuf(Proto.TokenKycStatus kycStatus)
		{
			return kycStatus == Proto.TokenKycStatus.Granted;
		}
		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.FreezeStatusFromProtobuf(Proto.TokenFreezeStatus)"]/*' />
		public static bool FreezeStatusFromProtobuf(Proto.TokenFreezeStatus freezeStatus)
        {
            return freezeStatus == Proto.TokenFreezeStatus.Frozen;
        }

		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.FromBytes(System.Byte[])"]/*' />
		public static TokenRelationship FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TokenRelationship.Parser.ParseFrom(bytes));
		}
		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.FromProtobuf(Proto.TokenRelationship)"]/*' />
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

		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}

		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.ToProtobuf"]/*' />
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
		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.KycStatusToProtobuf(System.Boolean)"]/*' />
		public static Proto.TokenKycStatus KycStatusToProtobuf(bool? kycStatus)
		{
			return kycStatus == null ? Proto.TokenKycStatus.KycNotApplicable : kycStatus.Value ? Proto.TokenKycStatus.Granted : Proto.TokenKycStatus.Revoked;
		}
		/// <include file="TokenRelationship.cs.xml" path='docs/member[@name="M:TokenRelationship.FreezeStatusToProtobuf(System.Boolean)"]/*' />
		public static Proto.TokenFreezeStatus FreezeStatusToProtobuf(bool? freezeStatus)
        {
            return freezeStatus == null ? Proto.TokenFreezeStatus.FreezeNotApplicable : freezeStatus.Value ? Proto.TokenFreezeStatus.Frozen : Proto.TokenFreezeStatus.Unfrozen;
        }
    }
}