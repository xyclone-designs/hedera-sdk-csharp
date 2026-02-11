// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ids;

namespace Hedera.Hashgraph.SDK.HBar
{
    /// <summary>
    /// An approved allowance of hbar transfers for a spender.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/cryptoallowance">Hedera Documentation</a>
    /// </summary>
    public class HbarAllowance
    {        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ownerAccountId">the owner granting the allowance</param>
        /// <param name="spenderAccountId">the spender</param>
        /// <param name="amount">the amount of hbar</param>
        internal HbarAllowance(AccountId? ownerAccountId, AccountId spenderAccountId, Hbar amount)
        {
            OwnerAccountId = ownerAccountId;
            SpenderAccountId = spenderAccountId;
            Amount = amount;
        }

		/// <summary>
		/// Create a hbar allowance from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new hbar allowance</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static HbarAllowance FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.CryptoAllowance.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a hbar allowance from a crypto allowance protobuf.
		/// </summary>
		/// <param name="allowanceProto">the crypto allowance protobuf</param>
		/// <returns>                         the new hbar allowance</returns>
		public static HbarAllowance FromProtobuf(Proto.CryptoAllowance allowanceProto)
        {
            return new HbarAllowance(AccountId.FromProtobuf(allowanceProto.Owner), AccountId.FromProtobuf(allowanceProto.Spender), Hbar.FromTinybars(allowanceProto.Amount));
        }
		/// <summary>
		/// Create a hbar allowance from a granted crypto allowance protobuf.
		/// </summary>
		/// <param name="allowanceProto">the granted crypto allowance protobuf</param>
		/// <returns>                         the new hbar allowance</returns>
		public static HbarAllowance FromProtobuf(Proto.GrantedCryptoAllowance allowanceProto)
        {
            return new HbarAllowance(null, AccountId.FromProtobuf(allowanceProto.Spender), Hbar.FromTinybars(allowanceProto.Amount));
        }

		/// <summary>
		/// The amount of the spender's allowance in Tinybars
		/// </summary>
		public Hbar Amount { get; init; }
		/// <summary>
		/// The account ID of the hbar owner (ie. the grantor of the allowance)
		/// </summary>
		public AccountId? OwnerAccountId { get; init; }
		/// <summary>
		/// The account ID of the spender of the hbar allowance
		/// </summary>
		public AccountId SpenderAccountId { get; init; }

		/// <summary>
		/// Validate that the client is configured correctly.
		/// </summary>
		/// <param name="client">the client to verify</param>
		/// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
		public virtual void ValidateChecksums(Client client)
        {
            OwnerAccountId?.ValidateChecksum(client);
			SpenderAccountId.ValidateChecksum(client);
		}

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         a byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <summary>
		/// Convert a crypto allowance into a protobuf.
		/// </summary>
		/// <returns>                         the protobuf</returns>
		public virtual Proto.CryptoAllowance ToProtobuf()
		{
			Proto.CryptoAllowance proto = new()
			{
				Amount = Amount.ToTinybars(),
				Spender = SpenderAccountId.ToProtobuf()
			};

			if (OwnerAccountId != null)
				proto.Owner = OwnerAccountId.ToProtobuf();

			return proto;
		}
		/// <summary>
		/// Convert a crypto allowance into a granted crypto allowance protobuf.
		/// </summary>
		/// <returns>                         the granted crypto allowance</returns>
		public virtual Proto.GrantedCryptoAllowance ToGrantedProtobuf()
		{
			return new Proto.GrantedCryptoAllowance
			{
				Amount = Amount.ToTinybars(),
				Spender = SpenderAccountId.ToProtobuf()
			};
		}
	}
}