// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions.Account;


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
        /// The account ID of the hbar owner (ie. the grantor of the allowance)
        /// </summary>
        public readonly AccountId OwnerAccountId;
        /// <summary>
        /// The account ID of the spender of the hbar allowance
        /// </summary>
        public readonly AccountId SpenderAccountId;
        /// <summary>
        /// The amount of the spender's allowance in Tinybars
        /// </summary>
        public readonly Hbar Amount;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ownerAccountId">the owner granting the allowance</param>
        /// <param name="spenderAccountId">the spender</param>
        /// <param name="amount">the amount of hbar</param>
        internal HbarAllowance(AccountId ownerAccountId, AccountId spenderAccountId, Hbar amount)
        {
            OwnerAccountId = ownerAccountId;
            SpenderAccountId = spenderAccountId;
            Amount = amount;
        }

		/// <summary>
		/// Create a hbar allowance from a crypto allowance protobuf.
		/// </summary>
		/// <param name="allowanceProto">the crypto allowance protobuf</param>
		/// <returns>                         the new hbar allowance</returns>
		internal static HbarAllowance FromProtobuf(Proto.CryptoAllowance allowanceProto)
        {
            return new HbarAllowance(allowanceProto.Owner is not null ? AccountId.FromProtobuf(allowanceProto.Owner) : null, allowanceProto.Spender is not null ? AccountId.FromProtobuf(allowanceProto.Spender) : null, Hbar.FromTinybars(allowanceProto.Amount));
        }
        /// <summary>
        /// Create a hbar allowance from a granted crypto allowance protobuf.
        /// </summary>
        /// <param name="allowanceProto">the granted crypto allowance protobuf</param>
        /// <returns>                         the new hbar allowance</returns>
        internal static HbarAllowance FromProtobuf(Proto.GrantedCryptoAllowance allowanceProto)
        {
            return new HbarAllowance(null, allowanceProto.Spender is not null ? AccountId.FromProtobuf(allowanceProto.Spender) : null, Hbar.FromTinybars(allowanceProto.Amount));
        }
        /// <summary>
        /// Convert a crypto allowance into a protobuf.
        /// </summary>
        /// <returns>                         the protobuf</returns>
        internal virtual Proto.CryptoAllowance ToProtobuf()
        {
			Proto.CryptoAllowance proto = new () 
            { 
                Amount = Amount.ToTinybars()
            };

			if (OwnerAccountId != null)
				proto.Owner = OwnerAccountId.ToProtobuf();

			if (SpenderAccountId != null)
				proto.Spender = SpenderAccountId.ToProtobuf();

			return proto;
		}
		/// <summary>
		/// Convert a crypto allowance into a granted crypto allowance protobuf.
		/// </summary>
		/// <returns>                         the granted crypto allowance</returns>
		internal virtual Proto.GrantedCryptoAllowance ToGrantedProtobuf()
		{
			Proto.GrantedCryptoAllowance proto = new()
			{
				Amount = Amount.ToTinybars()
			};

			if (SpenderAccountId != null)
				proto.Spender = SpenderAccountId.ToProtobuf();

			return proto;
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
        /// Validate that the client is configured correctly.
        /// </summary>
        /// <param name="client">the client to verify</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        public virtual void ValidateChecksums(Client client)
        {
            if (OwnerAccountId != null)
            {
                OwnerAccountId.ValidateChecksum(client);
            }

            if (SpenderAccountId != null)
            {
                SpenderAccountId.ValidateChecksum(client);
            }
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         a byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}