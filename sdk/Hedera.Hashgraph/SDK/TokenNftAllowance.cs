namespace Hedera.Hashgraph.SDK
{
	/**
     * Class to encapsulate the nft methods for token allowance's.
     */
    public class TokenNftAllowance {

        /**
         * The NFT token type that the allowance pertains to
         */
        @Nullable
        public readonly TokenId tokenId;

        /**
         * The account ID of the token owner (ie. the grantor of the allowance)
         */
        @Nullable
        public readonly AccountId ownerAccountId;

        /**
         * The account ID of the token allowance spender
         */
        @Nullable
        public readonly AccountId spenderAccountId;

        /**
         * The account ID of the spender who is granted approvedForAll allowance and granting
         * approval on an NFT serial to another spender.
         */
        @Nullable
        AccountId delegatingSpender;

        /**
         * The list of serial numbers that the spender is permitted to transfer.
         */
        public readonly List<long> serialNumbers;

        /**
         * If true, the spender has access to all of the owner's NFT units of type tokenId (currently
         * owned and any in the future).
         */
        @Nullable
        public readonly Boolean allSerials;

        /**
         * Constructor.
         *
         * @param tokenId                   the token id
         * @param ownerAccountId            the grantor's account id
         * @param spenderAccountId          the spender's account id
         * @param delegatingSpender         the delegating spender's account id
         * @param serialNumbers             the list of serial numbers
         * @param allSerials                grant for all serial's
         */
        TokenNftAllowance(
                @Nullable TokenId tokenId,
                @Nullable AccountId ownerAccountId,
                @Nullable AccountId spenderAccountId,
                @Nullable AccountId delegatingSpender,
                Collection<long> serialNumbers,
                @Nullable Boolean allSerials) {
            this.tokenId = tokenId;
            this.ownerAccountId = ownerAccountId;
            this.spenderAccountId = spenderAccountId;
            this.delegatingSpender = delegatingSpender;
            this.serialNumbers = new ArrayList<>(serialNumbers);
            this.allSerials = allSerials;
        }

        /**
         * Create a copy of a nft token allowance object.
         *
         * @param allowance                 the nft token allowance to copj
         * @return                          a new copy
         */
        static TokenNftAllowance copyFrom(TokenNftAllowance allowance) {
            return new TokenNftAllowance(
                    allowance.tokenId,
                    allowance.ownerAccountId,
                    allowance.spenderAccountId,
                    allowance.delegatingSpender,
                    allowance.serialNumbers,
                    allowance.allSerials);
        }

        /**
         * Create a nft token allowance from a protobuf.
         *
         * @param allowanceProto            the protobuf
         * @return                          the nft token allowance
         */
        static TokenNftAllowance FromProtobuf(NftAllowance allowanceProto) {
            return new TokenNftAllowance(
                    allowanceProto.hasTokenId() ? TokenId.FromProtobuf(allowanceProto.getTokenId()) : null,
                    allowanceProto.hasOwner() ? AccountId.FromProtobuf(allowanceProto.getOwner()) : null,
                    allowanceProto.hasSpender() ? AccountId.FromProtobuf(allowanceProto.getSpender()) : null,
                    allowanceProto.hasDelegatingSpender()
                            ? AccountId.FromProtobuf(allowanceProto.getDelegatingSpender())
                            : null,
                    allowanceProto.getSerialNumbersList(),
                    allowanceProto.hasApprovedForAll()
                            ? allowanceProto.getApprovedForAll().getValue()
                            : null);
        }

        /**
         * Create a nft token allowance from a byte array.
         *
         * @param bytes                     the byte array
         * @return                          the nft token allowance
         * @       when there is an issue with the protobuf
         */
        public static TokenNftAllowance FromBytes(byte[] bytes)  {
            return FromProtobuf(NftAllowance.Parser.ParseFrom(Objects.requireNonNull(bytes)));
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
            if (delegatingSpender != null) {
                delegatingSpender.validateChecksum(client);
            }
        }

        /**
         * Create the protobuf.
         *
         * @return                          the protobuf representation
         */
        NftAllowance ToProtobuf() {
            var builder = NftAllowance.newBuilder();
            if (tokenId != null) {
                builder.setTokenId(tokenId.ToProtobuf());
            }
            if (ownerAccountId != null) {
                builder.setOwner(ownerAccountId.ToProtobuf());
            }
            if (spenderAccountId != null) {
                builder.setSpender(spenderAccountId.ToProtobuf());
            }
            if (delegatingSpender != null) {
                builder.setDelegatingSpender(delegatingSpender.ToProtobuf());
            }
            builder.AddAllSerialNumbers(serialNumbers);
            if (allSerials != null) {
                builder.setApprovedForAll(
                        BoolValue.newBuilder().setValue(allSerials).build());
            }
            return builder.build();
        }

        /**
         * Create the protobuf.
         *
         * @return                          the remove protobuf
         */
        NftRemoveAllowance toRemoveProtobuf() {
            var builder = NftRemoveAllowance.newBuilder();
            if (tokenId != null) {
                builder.setTokenId(tokenId.ToProtobuf());
            }
            if (ownerAccountId != null) {
                builder.setOwner(ownerAccountId.ToProtobuf());
            }
            builder.AddAllSerialNumbers(serialNumbers);
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
            var stringHelper = MoreObjects.toStringHelper(this)
                    .Add("tokenId", tokenId)
                    .Add("ownerAccountId", ownerAccountId)
                    .Add("spenderAccountId", spenderAccountId)
                    .Add("delegatingSpender", delegatingSpender);
            if (allSerials != null) {
                stringHelper.Add("allSerials", allSerials);
            } else {
                stringHelper.Add("serials", serialNumbers);
            }
            return stringHelper.toString();
        }
    }

}