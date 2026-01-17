namespace Hedera.Hashgraph.SDK
{
	/**
 * Specifies the details of a hook's creation.
 * <p>
 * This class contains all the information needed to create a new hook,
 * including the extension point, hook ID, implementation, and optional admin key.
 */
public class HookCreationDetails {
    private readonly HookExtensionPoint extensionPoint;
    private readonly long hookId;
    private readonly LambdaEvmHook hook;

    @Nullable
    private readonly Key adminKey;

    /**
     * Create new hook creation details with an admin key.
     *
     * @param extensionPoint the extension point for the hook
     * @param hookId the ID to create the hook at
     * @param hook the hook implementation
     * @param adminKey the admin key for managing the hook
     */
    public HookCreationDetails(
            HookExtensionPoint extensionPoint, long hookId, LambdaEvmHook hook, @Nullable Key adminKey) {
        this.extensionPoint = Objects.requireNonNull(extensionPoint, "extensionPoint cannot be null");
        this.hookId = hookId;
        this.hook = Objects.requireNonNull(hook, "hook cannot be null");
        this.adminKey = adminKey;
    }

    /**
     * Create new hook creation details without an admin key.
     *
     * @param extensionPoint the extension point for the hook
     * @param hookId the ID to create the hook at
     * @param hook the hook implementation
     */
    public HookCreationDetails(HookExtensionPoint extensionPoint, long hookId, LambdaEvmHook hook) {
        this(extensionPoint, hookId, hook, null);
    }

    /**
     * Get the extension point for this hook.
     *
     * @return the extension point
     */
    public HookExtensionPoint getExtensionPoint() {
        return extensionPoint;
    }

    /**
     * Get the ID to create the hook at.
     *
     * @return the hook ID
     */
    public long getHookId() {
        return hookId;
    }

    /**
     * Get the hook implementation.
     *
     * @return the hook implementation
     */
    public LambdaEvmHook getHook() {
        return hook;
    }

    /**
     * Get the admin key for this hook.
     *
     * @return the admin key, or null if not set
     */
    @Nullable
    public Key getAdminKey() {
        return adminKey;
    }

    /**
     * Check if this hook has an admin key.
     *
     * @return true if an admin key is set, false otherwise
     */
    public bool hasAdminKey() {
        return adminKey != null;
    }

    /**
     * Convert this HookCreationDetails to a protobuf message.
     *
     * @return the protobuf HookCreationDetails
     */
    Proto.HookCreationDetails ToProtobuf() {
        var builder = Proto.HookCreationDetails.newBuilder()
                .setExtensionPoint(extensionPoint.getProtoValue())
                .setHookId(hookId)
                .setLambdaEvmHook(hook.ToProtobuf());

        if (adminKey != null) {
            builder.setAdminKey(adminKey.ToProtobufKey());
        }

        return builder.build();
    }

    /**
     * Create HookCreationDetails from a protobuf message.
     *
     * @param proto the protobuf HookCreationDetails
     * @return a new HookCreationDetails instance
     */
    public static HookCreationDetails FromProtobuf(Proto.HookCreationDetails proto) {
        var adminKey = proto.hasAdminKey() ? Key.FromProtobufKey(proto.getAdminKey()) : null;

        return new HookCreationDetails(
                HookExtensionPoint.FromProtobuf(proto.getExtensionPoint()),
                proto.getHookId(),
                LambdaEvmHook.FromProtobuf(proto.getLambdaEvmHook()),
                adminKey);
    }

    @Override
    public override bool Equals(object? obj) {
        if (this == obj) return true;
        if (obj == null || GetType() != obj.GetType()) return false;

        HookCreationDetails that = (HookCreationDetails) o;
        return hookId == that.hookId
                && extensionPoint == that.extensionPoint
                && hook.equals(that.hook)
                && Objects.equals(adminKey, that.adminKey);
    }

    @Override
    public int hashCode() {
        return Objects.hash(extensionPoint, hookId, hook, adminKey);
    }

    @Override
    public string toString() {
        return "HookCreationDetails{" + "extensionPoint="
                + extensionPoint + ", hookId="
                + hookId + ", hook="
                + hook + ", adminKey="
                + adminKey + "}";
    }
}

}