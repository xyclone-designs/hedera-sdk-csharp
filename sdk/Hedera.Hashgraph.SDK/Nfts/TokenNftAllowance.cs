// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Nfts
{
	/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="T:TokenNftAllowance"]' />
	public class TokenNftAllowance
    {
        /// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.#ctor(TokenId,AccountId,AccountId,AccountId,System.Collections.Generic.IEnumerable{System.Int64},System.Boolean)"]' />
        internal TokenNftAllowance(TokenId tokenId, AccountId? ownerAccountId, AccountId? spenderAccountId, AccountId? delegatingSpender, IEnumerable<long> serialNumbers, bool? allSerials)
        {
            TokenId = tokenId;
            OwnerAccountId = ownerAccountId;
            SpenderAccountId = spenderAccountId;
            DelegatingSpender = delegatingSpender;
            SerialNumbers = [.. serialNumbers];
            AllSerials = allSerials;
        }

		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.TokenId"]' />
		public TokenId TokenId { get; }
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.OwnerAccountId"]' />
		public AccountId? OwnerAccountId { get; }
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.SpenderAccountId"]' />
		public AccountId? SpenderAccountId { get; }
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.DelegatingSpender"]' />
		public AccountId? DelegatingSpender { get; }
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.SerialNumbers"]' />
		public List<long> SerialNumbers { get; }
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.AllSerials"]' />
		public bool? AllSerials { get; }

		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.CopyFrom(TokenNftAllowance)"]' />
		public static TokenNftAllowance CopyFrom(TokenNftAllowance allowance)
        {
            return new TokenNftAllowance(allowance.TokenId, allowance.OwnerAccountId, allowance.SpenderAccountId, allowance.DelegatingSpender, allowance.SerialNumbers, allowance.AllSerials);
        }
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.FromBytes(System.Byte[])"]' />
		public static TokenNftAllowance FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.NftAllowance.Parser.ParseFrom(bytes));
		}
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.FromProtobuf(Proto.Services.NftAllowance)"]' />
		public static TokenNftAllowance FromProtobuf(Proto.Services.NftAllowance allowanceProto)
        {
            return new TokenNftAllowance(
                TokenId.FromProtobuf(allowanceProto.TokenId),
                allowanceProto.Owner is null ? null : AccountId.FromProtobuf(allowanceProto.Owner),
                allowanceProto.Spender is null ? null : AccountId.FromProtobuf(allowanceProto.Spender),
                allowanceProto.DelegatingSpender is null ? null : AccountId.FromProtobuf(allowanceProto.DelegatingSpender),
                allowanceProto.SerialNumbers,
                allowanceProto.ApprovedForAll);
        }

		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.ToBytes"]' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.ToProtobuf"]' />
		public virtual Proto.Services.NftAllowance ToProtobuf()
        {
            Proto.Services.NftAllowance proto = new()
            {
				ApprovedForAll = AllSerials,
				TokenId = TokenId.ToProtobuf(),
			};

			if (OwnerAccountId?.ToProtobuf() is Proto.Services.AccountID owneraccountid)
				proto.Owner = owneraccountid;
			if (SpenderAccountId?.ToProtobuf() is Proto.Services.AccountID spenderaccountid)
				proto.Spender = spenderaccountid;
			if (DelegatingSpender?.ToProtobuf() is Proto.Services.AccountID delegatingspender)
				proto.DelegatingSpender = delegatingspender;

            proto.SerialNumbers.AddRange(SerialNumbers);

            return proto;
        }
        /// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.ToRemoveProtobuf"]' />
        public virtual Proto.Services.NftRemoveAllowance ToRemoveProtobuf()
        {
			Proto.Services.NftRemoveAllowance proto = new()
            {
				TokenId = TokenId.ToProtobuf(),
			};

			if (OwnerAccountId?.ToProtobuf() is Proto.Services.AccountID owneraccountid)
				proto.Owner = owneraccountid;

            proto.SerialNumbers.AddRange(SerialNumbers);
			
            return proto;
        }

		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.ValidateChecksums(Client)"]' />
		public virtual void ValidateChecksums(Client client)
		{
			TokenId.ValidateChecksum(client);
			OwnerAccountId?.ValidateChecksum(client);
			SpenderAccountId?.ValidateChecksum(client);
			DelegatingSpender?.ValidateChecksum(client);
		}
	}
}
