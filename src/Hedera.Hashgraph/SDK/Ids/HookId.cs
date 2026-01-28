// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Ids
{
    /// <summary>
    /// The ID of a hook.
    /// <p>
    /// This class represents the HookId protobuf message, which contains the hook's creating entity ID
    /// and an arbitrary 64-bit identifier.
    /// </summary>
    public class HookId
    {
        /// <summary>
        /// Create a new HookId.
        /// </summary>
        /// <param name="entityId">the hook's creating entity ID</param>
        /// <param name="hookId">the arbitrary 64-bit identifier</param>
        public HookId(HookEntityId entityId, long hookId)
        {
            EntityId = entityId;
			HookId_ = hookId;
        }

		public HookEntityId EntityId { get; }
		public long HookId_ { get; }

		/// <summary>
		/// Convert this HookId to a protobuf message.
		/// </summary>
		/// <returns>the protobuf HookId</returns>
		public virtual Proto.HookId ToProtobuf()
        {
            return new Proto.HookId
            {
				HookId_ = HookId_,
                EntityId = EntityId.ToProtobuf()
            };
        }

        /// <summary>
        /// Create a HookId from a protobuf message.
        /// </summary>
        /// <param name="proto">the protobuf HookId</param>
        /// <returns>a new HookId instance</returns>
        public static HookId FromProtobuf(Proto.HookId proto)
        {
            return new HookId(HookEntityId.FromProtobuf(proto.EntityId), proto.HookId_);
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o?.GetType())
                return false;
            
            HookId hookId1 = (HookId)o;

            return HookId_ == hookId1.HookId_ && EntityId.Equals(hookId1.EntityId);
        }
        public override int GetHashCode()
        {
            int result = EntityId.GetHashCode();
            result = 31 * result + HookId_.GetHashCode();
            return result;
        }
    }
}