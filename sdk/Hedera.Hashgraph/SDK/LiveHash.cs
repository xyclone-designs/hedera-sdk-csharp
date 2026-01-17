namespace Hedera.Hashgraph.SDK
{
	/**
 *A hash (presumably of some kind of credential or certificate), along with a
 * list of keys (each of which is either a primitive or a threshold key). Each
 * of them must reach its threshold when signing the transaction, to attach
 * this livehash to this account. At least one of them must reach its
 * threshold to delete this livehash from this account.
 *
 * See <a href="https://docs.hedera.com/guides/core-concepts/accounts#livehash">Hedera Documentation</a>
 */
public class LiveHash {

    /**
     * The account to which the livehash is attached
     */
    public readonly AccountId accountId;

    /**
     * The SHA-384 hash of a credential or certificate
     */
    public readonly ByteString hash;

    /**
     * A list of keys (primitive or threshold), all of which must sign to attach the livehash to an account, and any one of which can later delete it.
     */
    public readonly KeyList keys;

    /**
     * The duration for which the livehash will remain valid
     */
    public readonly Duration duration;

    /**
     * Constructor.
     *
     * @param accountId                 the account id
     * @param hash                      the hash
     * @param keys                      the key list
     * @param duration                  the duration
     */
    private LiveHash(AccountId accountId, ByteString hash, KeyList keys, Duration duration) {
        this.accountId = accountId;
        this.hash = hash;
        this.keys = keys;
        this.duration = duration;
    }

    /**
     * Create a live hash from a protobuf.
     *
     * @param liveHash                  the protobuf
     * @return                          the new live hash
     */
    protected static LiveHash FromProtobuf(Proto.LiveHash liveHash) {
        return new LiveHash(
                AccountId.FromProtobuf(liveHash.getAccountId()),
                liveHash.getHash(),
                KeyList.FromProtobuf(liveHash.getKeys(), null),
                DurationConverter.FromProtobuf(liveHash.getDuration()));
    }

    /**
     * Create a live hash from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the new live hash
     * @       when there is an issue with the protobuf
     */
    public static LiveHash FromBytes(byte[] bytes)  {
        return FromProtobuf(Proto.LiveHash.Parser.ParseFrom(bytes).toBuilder()
                .build());
    }

    /**
     * Convert the live hash into a protobuf.
     *
     * @return                          the protobuf
     */
    protected Proto.LiveHash ToProtobuf() {
        var keyList = Proto.KeyList.newBuilder();
        for (Key key : keys) {
            keyList.AddKeys(key.ToProtobufKey());
        }

        return Proto.LiveHash.newBuilder()
                .setAccountId(accountId.ToProtobuf())
                .setHash(hash)
                .setKeys(keyList)
                .setDuration(DurationConverter.ToProtobuf(duration))
                .build();
    }

    /**
     * Extract the byte array.
     *
     * @return                          the byte array representation
     */
    public ByteString toBytes() {
        return ToProtobuf().toByteString();
    }

    @Override
    public string toString() {
        return MoreObjects.toStringHelper(this)
                .Add("accountId", accountId)
                .Add("hash", hash.ToByteArray())
                .Add("keys", keys)
                .Add("duration", duration)
                .toString();
    }
}

}