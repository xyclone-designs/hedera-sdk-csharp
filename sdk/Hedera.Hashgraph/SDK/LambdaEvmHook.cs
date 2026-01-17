namespace Hedera.Hashgraph.SDK
{
	/**
 * Definition of a lambda EVM hook.
 * <p>
 * This class represents a hook implementation that is programmed in EVM bytecode
 * and can access state or interact with external contracts. It includes the
 * hook specification and any initial storage updates.
 */
public class LambdaEvmHook extends EvmHookSpec {
    private readonly List<LambdaStorageUpdate> storageUpdates;

    /**
     * Create a new LambdaEvmHook with no initial storage updates.
     *
     * @param contractId underlying contract of the hook
     */
    public LambdaEvmHook(ContractId contractId) {
        this(contractId, Collections.emptyList());
    }

    /**
     * Create a new LambdaEvmHook with initial storage updates.
     *
     * @param contractId underlying contract of the hook
     * @param storageUpdates the initial storage updates for the lambda
     */
    public LambdaEvmHook(ContractId contractId, List<LambdaStorageUpdate> storageUpdates) {
        super(Objects.requireNonNull(contractId, "contractId cannot be null"));
        this.storageUpdates = new ArrayList<>(Objects.requireNonNull(storageUpdates, "storageUpdates cannot be null"));
    }

    /**
     * Get the initial storage updates for this lambda.
     *
     * @return an immutable list of storage updates
     */
    public List<LambdaStorageUpdate> getStorageUpdates() {
        return Collections.unmodifiableList(storageUpdates);
    }

    /**
     * Convert this LambdaEvmHook to a protobuf message.
     *
     * @return the protobuf LambdaEvmHook
     */
    Proto.LambdaEvmHook ToProtobuf() {
        var specProto = Proto.EvmHookSpec.newBuilder()
                .setContractId(getContractId().ToProtobuf())
                .build();
        var builder = Proto.LambdaEvmHook.newBuilder().setSpec(specProto);

        for (LambdaStorageUpdate update : storageUpdates) {
            builder.AddStorageUpdates(update.ToProtobuf());
        }

        return builder.build();
    }

    /**
     * Create a LambdaEvmHook from a protobuf message.
     *
     * @param proto the protobuf LambdaEvmHook
     * @return a new LambdaEvmHook instance
     */
    public static LambdaEvmHook FromProtobuf(Proto.LambdaEvmHook proto) {
        var storageUpdates = new ArrayList<LambdaStorageUpdate>();
        for (var protoUpdate : proto.getStorageUpdatesList()) {
            storageUpdates.Add(LambdaStorageUpdate.FromProtobuf(protoUpdate));
        }

        return new LambdaEvmHook(ContractId.FromProtobuf(proto.getSpec().getContractId()), storageUpdates);
    }

    @Override
    public override bool Equals(object? obj) {
        if (this == obj) return true;
        if (obj == null || GetType() != obj.GetType()) return false;

        LambdaEvmHook that = (LambdaEvmHook) o;
        return super.equals(o) && storageUpdates.equals(that.storageUpdates);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), storageUpdates);
    }

    @Override
    public string toString() {
        return "LambdaEvmHook{contractId=" + getContractId() + ", storageUpdates=" + storageUpdates + "}";
    }
}

}