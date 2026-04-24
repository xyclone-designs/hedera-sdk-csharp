// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Cryptocurrency
{
    /// <include file="HbarAllowance.cs.xml" path='docs/member[@name="T:HbarAllowance"]/*' />
    public class HbarAllowance
    {        
        /// <include file="HbarAllowance.cs.xml" path='docs/member[@name="M:HbarAllowance.#ctor(AccountId,AccountId,Hbar)"]/*' />
        internal HbarAllowance(AccountId? ownerAccountId, AccountId? spenderAccountId, Hbar? amount)
        {
            OwnerAccountId = ownerAccountId;
            SpenderAccountId = spenderAccountId;
            Amount = amount;
        }

		/// <include file="HbarAllowance.cs.xml" path='docs/member[@name="M:HbarAllowance.FromBytes(System.Byte[])"]/*' />
		public static HbarAllowance FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.CryptoAllowance.Parser.ParseFrom(bytes));
		}
		/// <include file="HbarAllowance.cs.xml" path='docs/member[@name="M:HbarAllowance.FromProtobuf(Proto.Services.CryptoAllowance)"]/*' />
		public static HbarAllowance FromProtobuf(Proto.Services.CryptoAllowance allowanceProto)
        {
            return new HbarAllowance(AccountId.FromProtobuf(allowanceProto.Owner), AccountId.FromProtobuf(allowanceProto.Spender), Hbar.FromTinybars(allowanceProto.Amount));
        }
		/// <include file="HbarAllowance.cs.xml" path='docs/member[@name="M:HbarAllowance.FromProtobuf(Proto.Services.GrantedCryptoAllowance)"]/*' />
		public static HbarAllowance FromProtobuf(Proto.Services.GrantedCryptoAllowance allowanceProto)
        {
            return new HbarAllowance(null, AccountId.FromProtobuf(allowanceProto.Spender), Hbar.FromTinybars(allowanceProto.Amount));
        }

		/// <include file="HbarAllowance.cs.xml" path='docs/member[@name="P:HbarAllowance.Amount"]/*' />
		public Hbar? Amount { get; init; }
		/// <include file="HbarAllowance.cs.xml" path='docs/member[@name="P:HbarAllowance.OwnerAccountId"]/*' />
		public AccountId? OwnerAccountId { get; init; }
		/// <include file="HbarAllowance.cs.xml" path='docs/member[@name="P:HbarAllowance.SpenderAccountId"]/*' />
		public AccountId? SpenderAccountId { get; init; }

		/// <include file="HbarAllowance.cs.xml" path='docs/member[@name="M:HbarAllowance.ValidateChecksums(Client)"]/*' />
		public virtual void ValidateChecksums(Client client)
        {
            OwnerAccountId?.ValidateChecksum(client);
			SpenderAccountId?.ValidateChecksum(client);
		}

        /// <include file="HbarAllowance.cs.xml" path='docs/member[@name="M:HbarAllowance.ToBytes"]/*' />
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <include file="HbarAllowance.cs.xml" path='docs/member[@name="M:HbarAllowance.ToProtobuf"]/*' />
		public virtual Proto.Services.CryptoAllowance ToProtobuf()
		{
			Proto.Services.CryptoAllowance proto = new();

			if (OwnerAccountId != null)
                proto.Owner = OwnerAccountId.ToProtobuf();

			if (Amount != null)
				proto.Amount = Amount.ToTinybars();

			if (SpenderAccountId != null)
				proto.Spender = SpenderAccountId.ToProtobuf();

			return proto;
		}
		/// <include file="HbarAllowance.cs.xml" path='docs/member[@name="M:HbarAllowance.ToGrantedProtobuf"]/*' />
		public virtual Proto.Services.GrantedCryptoAllowance ToGrantedProtobuf()
		{
			Proto.Services.GrantedCryptoAllowance proto = new();

			if (Amount != null)
				proto.Amount = Amount.ToTinybars();

			if (SpenderAccountId != null)
				proto.Spender = SpenderAccountId.ToProtobuf();

			return proto;
		}
	}
}
