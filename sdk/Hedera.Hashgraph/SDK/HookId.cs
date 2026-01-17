namespace Hedera.Hashgraph.SDK
{
	/**
 * The ID of a hook.
 * <p>
 * This class represents the HookId protobuf message, which contains the hook's creating entity ID
 * and an arbitrary 64-bit identifier.
 */
public class HookId {
    private readonly HookEntityId entityId;
    private readonly long hookId;

    /**
     * Create a new HookId.
     *
     * @param entityId the hook's creating entity ID
     * @param hookId the arbitrary 64-bit identifier
     */
    public HookId(HookEntityId entityId, long hookId) {
        this.entityId = Objects.requireNonNull(entityId, "entityId cannot be null");
        this.hookId = hookId;
    }

    /**
     * Get the hook's creating entity ID.
     *
     * @return the entity ID
     */
    public HookEntityId getEntityId() {
        return entityId;
    }

    /**
     * Get the hook ID.
     *
     * @return the hook ID
     */
    public long getHookId() {
        return hookId;
    }

    /**
     * Convert this HookId to a protobuf message.
     *
     * @return the protobuf HookId
     */
    Proto.HookId ToProtobuf() {
        return Proto.HookId.newBuilder()
                .setEntityId(entityId.ToProtobuf())
                .setHookId(hookId)
                .build();
    }

    /**
     * Create a HookId from a protobuf message.
     *
     * @param proto the protobuf HookId
     * @return a new HookId instance
     */
    static HookId FromProtobuf(Proto.HookId proto) {
        return new HookId(HookEntityId.FromProtobuf(proto.getEntityId()), proto.getHookId());
    }

    @Override
    public override bool Equals(object? obj) {
        if (this == obj) return true;
        if (obj == null || GetType() != obj.GetType()) return false;

        HookId hookId1 = (HookId) o;
        return hookId == hookId1.hookId && entityId.equals(hookId1.entityId);
    }

    @Override
    public int hashCode() {
        int result = entityId.hashCode();
        result = 31 * result + long.hashCode(hookId);
        return result;
    }

    @Override
    public string toString() {
        return MoreObjects.toStringHelper(this)
                .Add("entityId", entityId)
                .Add("hookId", hookId)
                .toString();
    }
}

}