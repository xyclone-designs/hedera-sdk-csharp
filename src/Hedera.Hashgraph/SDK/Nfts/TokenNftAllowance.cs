// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Nfts
{
	/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="T:TokenNftAllowance"]/*' />
	public class TokenNftAllowance
    {
        /// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.#ctor(TokenId,AccountId,AccountId,AccountId,System.Collections.Generic.IEnumerable{System.Int64},System.Boolean)"]/*' />
        internal TokenNftAllowance(TokenId tokenId, AccountId? ownerAccountId, AccountId? spenderAccountId, AccountId? delegatingSpender, IEnumerable<long> serialNumbers, bool? allSerials)
        {
            TokenId = tokenId;
            OwnerAccountId = ownerAccountId;
            SpenderAccountId = spenderAccountId;
            DelegatingSpender = delegatingSpender;
            SerialNumbers = [.. serialNumbers];
            AllSerials = allSerials;
        }

		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.TokenId"]/*' />
		public readonly TokenId TokenId;
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.OwnerAccountId"]/*' />
		public readonly AccountId? OwnerAccountId;
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.SpenderAccountId"]/*' />
		public readonly AccountId? SpenderAccountId;
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.DelegatingSpender"]/*' />
		public readonly AccountId? DelegatingSpender;
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.SerialNumbers"]/*' />
		public readonly List<long> SerialNumbers;
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="F:TokenNftAllowance.AllSerials"]/*' />
		public readonly bool? AllSerials;

		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.CopyFrom(TokenNftAllowance)"]/*' />
		public static TokenNftAllowance CopyFrom(TokenNftAllowance allowance)
        {
            return new TokenNftAllowance(allowance.TokenId, allowance.OwnerAccountId, allowance.SpenderAccountId, allowance.DelegatingSpender, allowance.SerialNumbers, allowance.AllSerials);
        }
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.FromBytes(System.Byte[])"]/*' />
		public static TokenNftAllowance FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.NftAllowance.Parser.ParseFrom(bytes));
		}
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.FromProtobuf(Proto.NftAllowance)"]/*' />
		public static TokenNftAllowance FromProtobuf(Proto.NftAllowance allowanceProto)
        {
            return new TokenNftAllowance(
                TokenId.FromProtobuf(allowanceProto.TokenId), 
                AccountId.FromProtobuf(allowanceProto.Owner),
                AccountId.FromProtobuf(allowanceProto.Spender),
                AccountId.FromProtobuf(allowanceProto.DelegatingSpender),
                allowanceProto.SerialNumbers,
                allowanceProto.ApprovedForAll);
        }

		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.ToProtobuf"]/*' />
		public virtual Proto.NftAllowance ToProtobuf()
        {
            Proto.NftAllowance proto = new()
            {
				ApprovedForAll = AllSerials,
				TokenId = TokenId.ToProtobuf(),
			};

			if (OwnerAccountId?.ToProtobuf() is Proto.AccountID owneraccountid)
				proto.Owner = owneraccountid;
			if (SpenderAccountId?.ToProtobuf() is Proto.AccountID spenderaccountid)
				proto.Spender = spenderaccountid;
			if (DelegatingSpender?.ToProtobuf() is Proto.AccountID delegatingspender)
				proto.DelegatingSpender = delegatingspender;

			proto.SerialNumbers.AddRange(SerialNumbers);

            return proto;
        }
        /// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.ToRemoveProtobuf"]/*' />
        public virtual Proto.NftRemoveAllowance ToRemoveProtobuf()
        {
			Proto.NftRemoveAllowance proto = new()
            {
				TokenId = TokenId.ToProtobuf(),
			};

			if (OwnerAccountId?.ToProtobuf() is Proto.AccountID owneraccountid)
				proto.Owner = owneraccountid;

			proto.SerialNumbers.AddRange(SerialNumbers);
			
            return proto;
        }

		/// <include file="TokenNftAllowance.cs.xml" path='docs/member[@name="M:TokenNftAllowance.ValidateChecksums(Client)"]/*' />
		public virtual void ValidateChecksums(Client client)
		{
			TokenId.ValidateChecksum(client);
			OwnerAccountId?.ValidateChecksum(client);
			SpenderAccountId?.ValidateChecksum(client);
			DelegatingSpender?.ValidateChecksum(client);
		}
	}
}