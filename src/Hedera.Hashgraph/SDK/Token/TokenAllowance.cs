// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ids;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// An approved allowance of token transfers for a spender.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/tokenallowance">Hedera Documentation</a>
    /// </summary>
    public class TokenAllowance
    {
        /// <summary>
        /// The token that the allowance pertains to
        /// </summary>
        public readonly TokenId TokenId;
        /// <summary>
        /// The account ID of the hbar owner (ie. the grantor of the allowance)
        /// </summary>
        public readonly AccountId? OwnerAccountId;
        /// <summary>
        /// The account ID of the spender of the hbar allowance
        /// </summary>
        public readonly AccountId SpenderAccountId;
        /// <summary>
        /// The amount of the spender's token allowance
        /// </summary>
        public readonly long Amount;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="ownerAccountId">the grantor account id</param>
        /// <param name="spenderAccountId">the spender account id</param>
        /// <param name="amount">the token allowance</param>
        internal TokenAllowance(TokenId tokenId, AccountId? ownerAccountId, AccountId spenderAccountId, long amount)
        {
            TokenId = tokenId;
            OwnerAccountId = ownerAccountId;
            SpenderAccountId = spenderAccountId;
            Amount = amount;
        }

		/// <summary>
		/// Create a token allowance from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new token allowance</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static TokenAllowance FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TokenAllowance.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a token allowance from a protobuf.
		/// </summary>
		/// <param name="allowanceProto">the protobuf</param>
		/// <returns>                         the new token allowance</returns>
		public static TokenAllowance FromProtobuf(Proto.TokenAllowance allowanceProto)
        {
            return new TokenAllowance(
                TokenId.FromProtobuf(allowanceProto.TokenId), 
                AccountId.FromProtobuf(allowanceProto.Owner), 
                AccountId.FromProtobuf(allowanceProto.Spender), 
                allowanceProto.Amount);
        }
        /// <summary>
        /// Create a token allowance from a protobuf.
        /// </summary>
        /// <param name="allowanceProto">the protobuf</param>
        /// <returns>                         the new token allowance</returns>
        public static TokenAllowance FromProtobuf(Proto.GrantedTokenAllowance allowanceProto)
        {
            return new TokenAllowance(
                TokenId.FromProtobuf(allowanceProto.TokenId), 
                null, 
                AccountId.FromProtobuf(allowanceProto.Spender), 
                allowanceProto.Amount);
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
		/// Validate the configured client.
		/// </summary>
		/// <param name="client">the configured client</param>
		/// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
		public virtual void ValidateChecksums(Client client)
        {
            TokenId?.ValidateChecksum(client);
            OwnerAccountId?.ValidateChecksum(client);
            SpenderAccountId?.ValidateChecksum(client);
        }
		/// <summary>
		/// Create the protobuf.
		/// </summary>
		/// <returns>                         the protobuf representation</returns>
		public virtual Proto.TokenAllowance ToProtobuf()
        {
            Proto.TokenAllowance proto = new()
            {
				Amount = Amount
			};

            if (TokenId != null)
                proto.TokenId = TokenId.ToProtobuf();

            if (OwnerAccountId != null)
                proto.Owner = OwnerAccountId.ToProtobuf();

            if (SpenderAccountId != null)
                proto.Spender = SpenderAccountId.ToProtobuf();

            return proto;
        }
        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        public virtual Proto.GrantedTokenAllowance ToGrantedProtobuf()
        {
			Proto.GrantedTokenAllowance proto = new()
            {
				Amount = Amount
			};
            
            if (TokenId != null)
                proto.TokenId = TokenId.ToProtobuf();
            
            if (SpenderAccountId != null)
                proto.Spender = SpenderAccountId.ToProtobuf();

            return proto;
        }
    }
}