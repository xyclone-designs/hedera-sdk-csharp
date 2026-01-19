namespace Hedera.Hashgraph.SDK
{
	/**
     * The ID of a hook.
     * <p>
     * This class represents the HookId protobuf message, which contains the hook's creating entity ID
     * and an arbitrary 64-bit identifier.
     */
    public class HookId
	{
        /**
         * Create a new HookId.
         *
         * @param entityId the hook's creating entity ID
         * @param hookId the arbitrary 64-bit identifier
         */
        public HookId(HookEntityId entityId, long hookId) {
            EntityId = entityId;
			HookId_ = hookId;
        }
		/**
         * Create a HookId from a protobuf message.
         *
         * @param proto the protobuf HookId
         * @return a new HookId instance
         */
		public static HookId FromProtobuf(Proto.HookId proto)
		{
			return new HookId(HookEntityId.FromProtobuf(proto.EntityId), proto.HookId_);
		}

		public long HookId_ { get; }
		public HookEntityId EntityId { get; }

		/**
         * Convert this HookId to a protobuf message.
         *
         * @return the protobuf HookId
         */
		public Proto.HookId ToProtobuf() 
        {
            return new Proto.HookId
            {
				EntityId = EntityId.ToProtobuf(),
				HookId_ = HookId_,
			};
        }

		public override int GetHashCode()
		{
			int result = EntityId.GetHashCode();
			result = 31 * result + HookId_.GetHashCode();
			return result;
		}
		public override bool Equals(object? obj) 
        {
            if (this == obj) return true;
            if (obj is not HookId hookid) return false;

            return HookId_ == hookid.HookId_ && EntityId.Equals(hookid.EntityId);
        }
    }

}