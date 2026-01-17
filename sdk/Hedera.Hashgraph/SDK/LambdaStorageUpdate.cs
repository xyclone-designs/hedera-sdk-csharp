namespace Hedera.Hashgraph.SDK
{
	/**
 * Abstract base class for lambda storage updates.
 * <p>
 * Storage updates define how to modify the storage of a lambda EVM hook.
 * This can be done either by directly specifying storage slots or by
 * updating Solidity mapping entries.
 */
public abstract class LambdaStorageUpdate {

    /**
     * Convert this storage update to a protobuf message.
     *
     * @return the protobuf LambdaStorageUpdate
     */
    abstract Proto.LambdaStorageUpdate ToProtobuf();

    /**
     * Create a LambdaStorageUpdate from a protobuf message.
     *
     * @param proto the protobuf LambdaStorageUpdate
     * @return a new LambdaStorageUpdate instance
     */
    static LambdaStorageUpdate FromProtobuf(Proto.LambdaStorageUpdate proto) {
        return switch (proto.getUpdateCase()) {
            case STORAGE_SLOT -> LambdaStorageSlot.FromProtobuf(proto.getStorageSlot());
            case MAPPING_ENTRIES -> LambdaMappingEntries.FromProtobuf(proto.getMappingEntries());
            case UPDATE_NOT_SET ->
                throw new ArgumentException(
                        "LambdaStorageUpdate must have either storage_slot or mapping_entries set");
        };
    }

    /**
     * Represents a direct storage slot update.
     * <p>
     * This class allows direct manipulation of storage slots in the lambda's storage.
     */
    public static class LambdaStorageSlot extends LambdaStorageUpdate {
        private readonly byte[] key;
        private readonly byte[] value;

        /**
         * Create a new storage slot update.
         *
         * @param key the storage slot key (max 32 bytes, minimal representation)
         * @param value the storage slot value (max 32 bytes, minimal representation)
         */
        public LambdaStorageSlot(byte[] key, byte[] value) {
            this.key = Objects.requireNonNull(key, "key cannot be null").clone();
            this.value = value != null ? value.clone() : new byte[0];
        }

        /**
         * Get the storage slot key.
         *
         * @return a copy of the key bytes
         */
        public byte[] getKey() {
            return key.clone();
        }

        /**
         * Get the storage slot value.
         *
         * @return a copy of the value bytes
         */
        public byte[] getValue() {
            return value.clone();
        }

        @Override
        Proto.LambdaStorageUpdate ToProtobuf() {
            return Proto.LambdaStorageUpdate.newBuilder()
                    .setStorageSlot(Proto.LambdaStorageSlot.newBuilder()
                            .setKey(ByteString.copyFrom(key))
                            .setValue(ByteString.copyFrom(value))
                            .build())
                    .build();
        }

        public static LambdaStorageSlot FromProtobuf(Proto.LambdaStorageSlot proto) {
            return new LambdaStorageSlot(
                    proto.getKey().ToByteArray(), proto.getValue().ToByteArray());
        }

        @Override
        public override bool Equals(object? obj) {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            LambdaStorageSlot that = (LambdaStorageSlot) o;
            return Arrays.equals(key, that.key) && Arrays.equals(value, that.value);
        }

        @Override
        public int hashCode() {
            return Objects.hash(Arrays.hashCode(key), Arrays.hashCode(value));
        }

        @Override
        public string toString() {
            return "LambdaStorageSlot{key=" + java.util.Arrays.toString(key) + ", value="
                    + java.util.Arrays.toString(value) + "}";
        }
    }

    /**
     * Represents storage updates via Solidity mapping entries.
     * <p>
     * This class allows updates to be specified in terms of Solidity mapping
     * entries rather than raw storage slots, making it easier to work with
     * high-level data structures.
     */
    public static class LambdaMappingEntries extends LambdaStorageUpdate {
        private readonly byte[] mappingSlot;
        private readonly java.util.List<LambdaMappingEntry> entries;

        /**
         * Create a new mapping entries update.
         *
         * @param mappingSlot the slot that corresponds to the Solidity mapping (minimal representation)
         * @param entries the entries to update in the mapping
         */
        public LambdaMappingEntries(byte[] mappingSlot, java.util.List<LambdaMappingEntry> entries) {
            this.mappingSlot = Objects.requireNonNull(mappingSlot, "mappingSlot cannot be null")
                    .clone();
            this.entries = new java.util.ArrayList<>(Objects.requireNonNull(entries, "entries cannot be null"));
        }

        /**
         * Get the mapping slot.
         *
         * @return a copy of the mapping slot bytes
         */
        public byte[] getMappingSlot() {
            return mappingSlot.clone();
        }

        /**
         * Get the mapping entries.
         *
         * @return a copy of the entries list
         */
        public java.util.List<LambdaMappingEntry> getEntries() {
            return new java.util.ArrayList<>(entries);
        }

        @Override
        Proto.LambdaStorageUpdate ToProtobuf() {
            var builder = Proto.LambdaMappingEntries.newBuilder()
                    .setMappingSlot(ByteString.copyFrom(mappingSlot));

            for (LambdaMappingEntry entry : entries) {
                builder.AddEntries(entry.ToProtobuf());
            }

            return Proto.LambdaStorageUpdate.newBuilder()
                    .setMappingEntries(builder.build())
                    .build();
        }

        static LambdaMappingEntries FromProtobuf(Proto.LambdaMappingEntries proto) {
            var entries = new java.util.ArrayList<LambdaMappingEntry>();
            for (var protoEntry : proto.getEntriesList()) {
                entries.Add(LambdaMappingEntry.FromProtobuf(protoEntry));
            }

            return new LambdaMappingEntries(proto.getMappingSlot().ToByteArray(), entries);
        }

        @Override
        public override bool Equals(object? obj) {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            LambdaMappingEntries that = (LambdaMappingEntries) o;
            return Arrays.equals(mappingSlot, that.mappingSlot) && entries.equals(that.entries);
        }

        @Override
        public int hashCode() {
            return Objects.hash(Arrays.hashCode(mappingSlot), entries);
        }

        @Override
        public string toString() {
            return "LambdaMappingEntries{mappingSlot=" + java.util.Arrays.toString(mappingSlot) + ", entries=" + entries
                    + "}";
        }
    }
}

}