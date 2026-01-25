// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Transactions.Account;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Associates the provided Hedera account with the provided Hedera token(s).
    /// Hedera accounts must be associated with a fungible or non-fungible token
    /// first before you can transfer tokens to that account. In the case of
    /// NON_FUNGIBLE Type, once an account is associated, it can hold any number
    /// of NFTs (serial numbers) of that token type. The Hedera account that is
    /// being associated with a token is required to sign the transaction.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/associate-tokens-to-an-account">Hedera Documentation</a>
    /// </summary>
    public class TokenAssociation
    {
        /// <summary>
        /// The token involved in the association
        /// </summary>
        public readonly TokenId tokenId;
        /// <summary>
        /// The account involved in the association
        /// </summary>
        public readonly AccountId accountId;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="accountId">the account id</param>
        TokenAssociation(TokenId tokenId, AccountId accountId)
        {
            this.tokenId = tokenId;
            this.accountId = accountId;
        }

		/// <summary>
		/// Create a token association from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new token association</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static TokenAssociation FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TokenAssociation.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a token association from a protobuf.
		/// </summary>
		/// <param name="tokenAssociation">the protobuf</param>
		/// <returns>                         the new token association</returns>
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

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <summary>
		/// Create the protobuf.
		/// </summary>
		/// <returns>                         the protobuf representation</returns>
		public virtual Proto.TokenAssociation ToProtobuf()
		{
			return new Proto.TokenAssociation
			{
				TokenId = tokenId.ToProtobuf(),
				AccountId = accountId.ToProtobuf(),
			};
		}
	}
}