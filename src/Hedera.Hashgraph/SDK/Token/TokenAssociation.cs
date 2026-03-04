// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenAssociation.cs.xml" path='docs/member[@name="T:TokenAssociation"]/*' />
    public class TokenAssociation
    {
        /// <include file="TokenAssociation.cs.xml" path='docs/member[@name="F:TokenAssociation.TokenId"]/*' />
        public readonly TokenId TokenId;
        /// <include file="TokenAssociation.cs.xml" path='docs/member[@name="F:TokenAssociation.AccountId"]/*' />
        public readonly AccountId AccountId;

        /// <include file="TokenAssociation.cs.xml" path='docs/member[@name="M:TokenAssociation.TokenAssociation(TokenId,AccountId)"]/*' />
        TokenAssociation(TokenId tokenId, AccountId accountId)
        {
            this.TokenId = tokenId;
            this.AccountId = accountId;
        }

		/// <include file="TokenAssociation.cs.xml" path='docs/member[@name="M:TokenAssociation.FromBytes(System.Byte[])"]/*' />
		public static TokenAssociation FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TokenAssociation.Parser.ParseFrom(bytes));
		}
		/// <include file="TokenAssociation.cs.xml" path='docs/member[@name="M:TokenAssociation.FromProtobuf(Proto.TokenAssociation)"]/*' />
		public static TokenAssociation FromProtobuf(Proto.TokenAssociation tokenAssociation)
        {
            return new TokenAssociation(
				tokenAssociation.TokenId is not null 
                    ? TokenId.FromProtobuf(tokenAssociation.TokenId) 
                    : new TokenId(0, 0, 0), 
                tokenAssociation.AccountId is not null 
                    ? AccountId.FromProtobuf(tokenAssociation.AccountId) 
                    : new AccountId(0, 0, 0));
        }

        /// <include file="TokenAssociation.cs.xml" path='docs/member[@name="M:TokenAssociation.ToBytes"]/*' />
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <include file="TokenAssociation.cs.xml" path='docs/member[@name="M:TokenAssociation.ToProtobuf"]/*' />
		public virtual Proto.TokenAssociation ToProtobuf()
		{
			return new Proto.TokenAssociation
			{
				TokenId = TokenId.ToProtobuf(),
				AccountId = AccountId.ToProtobuf(),
			};
		}
	}
}