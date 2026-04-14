// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="HookId.cs.xml" path='docs/member[@name="T:HookId"]/*' />
    public class HookId(HookEntityId entityId, long hookId)
    {
        public HookEntityId EntityId { get; } = entityId;
        public long HookId_ { get; } = hookId;

        /// <include file="HookId.cs.xml" path='docs/member[@name="M:HookId.ToProtobuf"]/*' />
        public virtual Proto.Services.HookId ToProtobuf()
        {
            return new Proto.Services.HookId
            {
				HookId_ = HookId_,
                EntityId = EntityId.ToProtobuf()
            };
        }

        /// <include file="HookId.cs.xml" path='docs/member[@name="M:HookId.FromProtobuf(Proto.Services.HookId)"]/*' />
        public static HookId FromProtobuf(Proto.Services.HookId proto)
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
