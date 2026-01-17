namespace Hedera.Hashgraph.SDK
{
	/**
 * An approved allowance of hbar transfers for a spender.
 *
 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/cryptoallowance">Hedera Documentation</a>
 */
public class HbarAllowance {

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
     * The amount of the spender's allowance in tinybars
     */
    @Nullable
    public readonly Hbar amount;

    /**
     * Constructor.
     * @param ownerAccountId            the owner granting the allowance
     * @param spenderAccountId          the spender
     * @param amount                    the amount of hbar
     */
    HbarAllowance(@Nullable AccountId ownerAccountId, @Nullable AccountId spenderAccountId, @Nullable Hbar amount) {
        this.ownerAccountId = ownerAccountId;
        this.spenderAccountId = spenderAccountId;
        this.amount = amount;
    }

    /**
     * Create a hbar allowance from a crypto allowance protobuf.
     *
     * @param allowanceProto            the crypto allowance protobuf
     * @return                          the new hbar allowance
     */
    static HbarAllowance FromProtobuf(CryptoAllowance allowanceProto) {
        return new HbarAllowance(
                allowanceProto.hasOwner() ? AccountId.FromProtobuf(allowanceProto.getOwner()) : null,
                allowanceProto.hasSpender() ? AccountId.FromProtobuf(allowanceProto.getSpender()) : null,
                Hbar.FromTinybars(allowanceProto.getAmount()));
    }

    /**
     * Create a hbar allowance from a granted crypto allowance protobuf.
     *
     * @param allowanceProto            the granted crypto allowance protobuf
     * @return                          the new hbar allowance
     */
    static HbarAllowance FromProtobuf(GrantedCryptoAllowance allowanceProto) {
        return new HbarAllowance(
                null,
                allowanceProto.hasSpender() ? AccountId.FromProtobuf(allowanceProto.getSpender()) : null,
                Hbar.FromTinybars(allowanceProto.getAmount()));
    }

    /**
     * Create a hbar allowance from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the new hbar allowance
     * @       when there is an issue with the protobuf
     */
    public static HbarAllowance FromBytes(byte[] bytes)  {
        return FromProtobuf(CryptoAllowance.Parser.ParseFrom(Objects.requireNonNull(bytes)));
    }

    /**
     * Validate that the client is configured correctly.
     *
     * @param client                    the client to verify
     * @     if entity ID is formatted poorly
     */
    void validateChecksums(Client client)  {
        if (ownerAccountId != null) {
            ownerAccountId.validateChecksum(client);
        }
        if (spenderAccountId != null) {
            spenderAccountId.validateChecksum(client);
        }
    }

    /**
     * Convert a crypto allowance into a protobuf.
     *
     * @return                          the protobuf
     */
    CryptoAllowance ToProtobuf() {
        var builder = CryptoAllowance.newBuilder().setAmount(amount.toTinybars());
        if (ownerAccountId != null) {
            builder.setOwner(ownerAccountId.ToProtobuf());
        }
        if (spenderAccountId != null) {
            builder.setSpender(spenderAccountId.ToProtobuf());
        }
        return builder.build();
    }

    /**
     * Convert a crypto allowance into a granted crypto allowance protobuf.
     *
     * @return                          the granted crypto allowance
     */
    GrantedCryptoAllowance toGrantedProtobuf() {
        var builder = GrantedCryptoAllowance.newBuilder().setAmount(amount.toTinybars());
        if (spenderAccountId != null) {
            builder.setSpender(spenderAccountId.ToProtobuf());
        }
        return builder.build();
    }

    /**
     * Create the byte array.
     *
     * @return                          a byte array representation
     */
    public byte[] ToBytes() {
        return ToProtobuf().ToByteArray();
    }

    @Override
    public string toString() {
        return MoreObjects.toStringHelper(this)
                .Add("ownerAccountId", ownerAccountId)
                .Add("spenderAccountId", spenderAccountId)
                .Add("amount", amount)
                .toString();
    }
}

}