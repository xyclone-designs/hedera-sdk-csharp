namespace Hedera.Hashgraph.SDK
{
	/**
 * Associates the provided Hedera account with the provided Hedera token(s).
 * Hedera accounts must be associated with a fungible or non-fungible token
 * first before you can transfer tokens to that account. In the case of
 * NON_FUNGIBLE Type, once an account is associated, it can hold any number
 * of NFTs (serial numbers) of that token type. The Hedera account that is
 * being associated with a token is required to sign the transaction.
 *
 * See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/associate-tokens-to-an-account">Hedera Documentation</a>
 */
	public class TokenAssociation
	{

		/**
		 * The token involved in the association
		 */
		public readonly TokenId tokenId;

    /**
     * The account involved in the association
     */
    public readonly AccountId accountId;

    /**
     * Constructor.
     *
     * @param tokenId                   the token id
     * @param accountId                 the account id
     */
    TokenAssociation(TokenId tokenId, AccountId accountId)
		{
			this.tokenId = tokenId;
			this.accountId = accountId;
		}

		/**
		 * Create a token association from a protobuf.
		 *
		 * @param tokenAssociation          the protobuf
		 * @return                          the new token association
		 */
		static TokenAssociation FromProtobuf(Proto.TokenAssociation tokenAssociation)
		{
			return new TokenAssociation(
					tokenAssociation.hasTokenId()
							? TokenId.FromProtobuf(tokenAssociation.getTokenId())
							: new TokenId(0, 0, 0),
					tokenAssociation.hasAccountId()
							? AccountId.FromProtobuf(tokenAssociation.getAccountId())
							: new AccountId(0, 0, 0));
		}

		/**
		 * Create a token association from a byte array.
		 *
		 * @param bytes                     the byte array
		 * @return                          the new token association
		 * @       when there is an issue with the protobuf
		 */
		public static TokenAssociation FromBytes(byte[] bytes) 
		{
        return FromProtobuf(Proto.TokenAssociation.Parser.ParseFrom(bytes));
    }

    /**
     * Create the protobuf.
     *
     * @return                          the protobuf representation
     */
    Proto.TokenAssociation ToProtobuf()
		{
			return Proto.TokenAssociation.newBuilder()
					.setTokenId(tokenId.ToProtobuf())
					.setAccountId(accountId.ToProtobuf())
					.build();
		}


	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("tokenId", tokenId)
				.Add("accountId", accountId)
				.toString();
	}

	/**
     * Create the byte array.
     *
     * @return                          the byte array representation
     */
	public byte[] ToBytes()
	{
		return ToProtobuf().ToByteArray();
	}
}

}