namespace Hedera.Hashgraph.SDK
{
	/**
 * An approved allowance of token transfers for a spender.
 *
 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/tokenallowance">Hedera Documentation</a>
 */
public class TokenAllowance {
    /**
     * The token that the allowance pertains to
     */
    @Nullable
    public readonly TokenId tokenId;
    /**
     * The account ID of the hbar owner (ie. the grantor of the allowance)
     */
    @Nullable
    public readonly AccountId ownerAccountId;
    /**
     * The account ID of the spender of the hbar allowance
     */
    @Nullable
    public readonly AccountId spenderAccountId;
    /**
     * The amount of the spender's token allowance
     */
    public readonly long amount;

    /**
     * Constructor.
     *
     * @param tokenId                   the token id
     * @param ownerAccountId            the grantor account id
     * @param spenderAccountId          the spender account id
     * @param amount                    the token allowance
     */
    TokenAllowance(
            @Nullable TokenId tokenId,
            @Nullable AccountId ownerAccountId,
            @Nullable AccountId spenderAccountId,
            long amount) {
        this.tokenId = tokenId;
        this.ownerAccountId = ownerAccountId;
        this.spenderAccountId = spenderAccountId;
        this.amount = amount;
    }

    /**
     * Create a token allowance from a protobuf.
     *
     * @param allowanceProto            the protobuf
     * @return                          the new token allowance
     */
    static TokenAllowance FromProtobuf(Proto.TokenAllowance allowanceProto) {
        return new TokenAllowance(
                allowanceProto.hasTokenId() ? TokenId.FromProtobuf(allowanceProto.getTokenId()) : null,
                allowanceProto.hasOwner() ? AccountId.FromProtobuf(allowanceProto.getOwner()) : null,
                allowanceProto.hasSpender() ? AccountId.FromProtobuf(allowanceProto.getSpender()) : null,
                allowanceProto.getAmount());
    }

    /**
     * Create a token allowance from a protobuf.
     *
     * @param allowanceProto            the protobuf
     * @return                          the new token allowance
     */
    static TokenAllowance FromProtobuf(GrantedTokenAllowance allowanceProto) {
        return new TokenAllowance(
                allowanceProto.hasTokenId() ? TokenId.FromProtobuf(allowanceProto.getTokenId()) : null,
                null,
                allowanceProto.hasSpender() ? AccountId.FromProtobuf(allowanceProto.getSpender()) : null,
                allowanceProto.getAmount());
    }

    /**
     * Create a token allowance from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the new token allowance
     * @       when there is an issue with the protobuf
     */
    public static TokenAllowance FromBytes(byte[] bytes)  {
        return FromProtobuf(Proto.TokenAllowance.Parser.ParseFrom(Objects.requireNonNull(bytes)));
    }

    /**
     * Validate the configured client.
     *
     * @param client                    the configured client
     * @     if entity ID is formatted poorly
     */
    void validateChecksums(Client client)  {
        if (tokenId != null) {
            tokenId.validateChecksum(client);
        }
        if (ownerAccountId != null) {
            ownerAccountId.validateChecksum(client);
        }
        if (spenderAccountId != null) {
            spenderAccountId.validateChecksum(client);
        }
    }

    /**
     * Create the protobuf.
     *
     * @return                          the protobuf representation
     */
    Proto.TokenAllowance ToProtobuf() {
        var builder = Proto.TokenAllowance.newBuilder().setAmount(amount);
        if (tokenId != null) {
            builder.setTokenId(tokenId.ToProtobuf());
        }
        if (ownerAccountId != null) {
            builder.setOwner(ownerAccountId.ToProtobuf());
        }
        if (spenderAccountId != null) {
            builder.setSpender(spenderAccountId.ToProtobuf());
        }
        return builder.build();
    }

    /**
     * Create the byte array.
     *
     * @return                          the protobuf representation
     */
    GrantedTokenAllowance toGrantedProtobuf() {
        var builder = GrantedTokenAllowance.newBuilder().setAmount(amount);
        if (tokenId != null) {
            builder.setTokenId(tokenId.ToProtobuf());
        }
        if (spenderAccountId != null) {
            builder.setSpender(spenderAccountId.ToProtobuf());
        }
        return builder.build();
    }

    /**
     * Create the byte array.
     *
     * @return                          the byte array representation
     */
    public byte[] ToBytes() {
        return ToProtobuf().ToByteArray();
    }

    @Override
    public string toString() {
        return MoreObjects.toStringHelper(this)
                .Add("tokenId", tokenId)
                .Add("ownerAccountId", ownerAccountId)
                .Add("spenderAccountId", spenderAccountId)
                .Add("amount", amount)
                .toString();
    }
}

}