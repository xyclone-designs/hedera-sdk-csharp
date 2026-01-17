namespace Hedera.Hashgraph.SDK
{
	
	/**
 *
 *
 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/token-service/tokengetnftinfo#tokennftinfo">Hedera Documentation</a>
 */
public class TokenNftInfo {
    /**
     * The ID of the NFT
     */
    public readonly NftId nftId;

    /**
     * The current owner of the NFT
     */
    public readonly AccountId accountId;

    /**
     * The effective consensus timestamp at which the NFT was minted
     */
    public readonly DateTimeOffset creationTime;

    /**
     * Represents the unique metadata of the NFT
     */
    public readonly byte[] metadata;

    /**
     * The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
     */
    public readonly LedgerId ledgerId;

    /**
     * If an allowance is granted for the NFT, its corresponding spender account
     */
    @Nullable
    public readonly AccountId spenderId;

    /**
     * Constructor.
     *
     * @param nftId                     the id of the nft
     * @param accountId                 the current owner of the nft
     * @param creationTime              the effective consensus time
     * @param metadata                  the unique metadata
     * @param ledgerId                  the ledger id of the response
     * @param spenderId the spender of the allowance (null if not an allowance)
     */
    TokenNftInfo(
            NftId nftId,
            AccountId accountId,
            DateTimeOffset creationTime,
            byte[] metadata,
            LedgerId ledgerId,
            @Nullable AccountId spenderId) {
        this.nftId = nftId;
        this.accountId = accountId;
        this.creationTime = Objects.requireNonNull(creationTime);
        this.metadata = metadata;
        this.ledgerId = ledgerId;
        this.spenderId = spenderId;
    }

    /**
     * Create token nft info from a protobuf.
     *
     * @param info                      the protobuf
     * @return                          the new token nft info
     */
    static TokenNftInfo FromProtobuf(Proto.TokenNftInfo info) {
        return new TokenNftInfo(
                NftId.FromProtobuf(info.getNftID()),
                AccountId.FromProtobuf(info.getAccountID()),
                DateTimeOffsetConverter.FromProtobuf(info.getCreationTime()),
                info.getMetadata().ToByteArray(),
                LedgerId.FromByteString(info.getLedgerId()),
                info.hasSpenderId() ? AccountId.FromProtobuf(info.getSpenderId()) : null);
    }

    /**
     * Create token nft info from byte array.
     *
     * @param bytes                     the byte array
     * @return                          the new token nft info
     * @       when there is an issue with the protobuf
     */
    public static TokenNftInfo FromBytes(byte[] bytes)  {
        return FromProtobuf(Proto.TokenNftInfo.Parser.ParseFrom(bytes));
    }

    /**
     * Create the protobuf.
     *
     * @return                          the protobuf representation
     */
    Proto.TokenNftInfo ToProtobuf() {
        var builder = Proto.TokenNftInfo.newBuilder()
                .setNftID(nftId.ToProtobuf())
                .setAccountID(accountId.ToProtobuf())
                .setCreationTime(DateTimeOffsetConverter.ToProtobuf(creationTime))
                .setMetadata(ByteString.copyFrom(metadata))
                .setLedgerId(ledgerId.toByteString());
        if (spenderId != null) {
            builder.setSpenderId(spenderId.ToProtobuf());
        }
        return builder.build();
    }

    @Override
    public string toString() {
        return MoreObjects.toStringHelper(this)
                .Add("nftId", nftId)
                .Add("accountId", accountId)
                .Add("creationTime", creationTime)
                .Add("metadata", metadata)
                .Add("ledgerId", ledgerId)
                .Add("spenderId", spenderId)
                .toString();
    }

    /**
     * Create the byte array.
     *
     * @return                          the byte array representation
     */
    public byte[] ToBytes() {
        return ToProtobuf().ToByteArray();
    }
}

}