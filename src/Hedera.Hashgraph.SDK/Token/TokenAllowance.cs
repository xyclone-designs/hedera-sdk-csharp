// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenAllowance.cs.xml" path='docs/member[@name="T:TokenAllowance"]/*' />
    public class TokenAllowance
    {
        /// <include file="TokenAllowance.cs.xml" path='docs/member[@name="F:TokenAllowance.TokenId"]/*' />
        public readonly TokenId TokenId;
        /// <include file="TokenAllowance.cs.xml" path='docs/member[@name="F:TokenAllowance.OwnerAccountId"]/*' />
        public readonly AccountId? OwnerAccountId;
        /// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.#ctor(TokenId,AccountId,AccountId,System.Int64)"]/*' />
        public readonly AccountId? SpenderAccountId;
        /// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.#ctor(TokenId,AccountId,AccountId,System.Int64)_2"]/*' />
        public readonly long Amount;

        /// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.#ctor(TokenId,AccountId,AccountId,System.Int64)_3"]/*' />
        internal TokenAllowance(TokenId tokenId, AccountId? ownerAccountId, AccountId? spenderAccountId, long amount)
        {
            TokenId = tokenId;
            OwnerAccountId = ownerAccountId;
            SpenderAccountId = spenderAccountId;
            Amount = amount;
        }

		/// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.FromBytes(System.Byte[])"]/*' />
		public static TokenAllowance FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.TokenAllowance.Parser.ParseFrom(bytes));
		}
		/// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.FromProtobuf(Proto.Services.TokenAllowance)"]/*' />
		public static TokenAllowance FromProtobuf(Proto.Services.TokenAllowance allowanceProto)
        {
            return new TokenAllowance(
                TokenId.FromProtobuf(allowanceProto.TokenId), 
                AccountId.FromProtobuf(allowanceProto.Owner), 
                AccountId.FromProtobuf(allowanceProto.Spender), 
                allowanceProto.Amount);
        }
        /// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.FromProtobuf(Proto.Services.GrantedTokenAllowance)"]/*' />
        public static TokenAllowance FromProtobuf(Proto.Services.GrantedTokenAllowance allowanceProto)
        {
            return new TokenAllowance(
                TokenId.FromProtobuf(allowanceProto.TokenId), 
                null, 
                AccountId.FromProtobuf(allowanceProto.Spender), 
                allowanceProto.Amount);
        }

		/// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.ValidateChecksums(Client)"]/*' />
		public virtual void ValidateChecksums(Client client)
        {
            TokenId.ValidateChecksum(client);
            OwnerAccountId?.ValidateChecksum(client);
            SpenderAccountId?.ValidateChecksum(client);
        }
		/// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.ToProtobuf"]/*' />
		public virtual Proto.Services.TokenAllowance ToProtobuf()
        {
            Proto.Services.TokenAllowance proto = new()
            {
				Amount = Amount,
                TokenId = TokenId.ToProtobuf()
			};

            if (OwnerAccountId != null)
                proto.Owner = OwnerAccountId.ToProtobuf();

            if (SpenderAccountId != null)
                proto.Spender = SpenderAccountId.ToProtobuf();

            return proto;
        }
        /// <include file="TokenAllowance.cs.xml" path='docs/member[@name="M:TokenAllowance.ToGrantedProtobuf"]/*' />
        public virtual Proto.Services.GrantedTokenAllowance ToGrantedProtobuf()
        {
			Proto.Services.GrantedTokenAllowance proto = new()
            {
				Amount = Amount,
				TokenId = TokenId.ToProtobuf()
			};
                        
            if (SpenderAccountId != null)
                proto.Spender = SpenderAccountId.ToProtobuf();

            return proto;
        }
    }
}
