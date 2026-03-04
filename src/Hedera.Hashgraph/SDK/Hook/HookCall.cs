// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="HookCall.cs.xml" path='docs/member[@name="T:HookCall"]/*' />
    public abstract class HookCall
    {
        /// <include file="HookCall.cs.xml" path='docs/member[@name="M:HookCall.#ctor(System.Int64,EvmHookCall)"]/*' />
        protected HookCall(long hookId, EvmHookCall evmHookCall)
        {
            HookId = hookId;
            EvmHookCall = evmHookCall;
        }

        /// <include file="HookCall.cs.xml" path='docs/member[@name="P:HookCall.HookId"]/*' />
        public virtual long HookId { get; }

        /// <include file="HookCall.cs.xml" path='docs/member[@name="P:HookCall.EvmHookCall"]/*' />
        public virtual EvmHookCall EvmHookCall { get; }

        /// <include file="HookCall.cs.xml" path='docs/member[@name="M:HookCall.ToProtobuf"]/*' />
        public virtual Proto.HookCall ToProtobuf()
        {
            return new Proto.HookCall
			{
				HookId = HookId,
                EvmHookCall = EvmHookCall.ToProtobuf()
			};
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o?.GetType())
                return false;

            HookCall hookCall = (HookCall)o;

            return Equals(HookId, hookCall.HookId) && EvmHookCall.Equals(hookCall.EvmHookCall);
        }

        public override int GetHashCode()
        {
            int result = HookId.GetHashCode();

            result = 31 * result + EvmHookCall.GetHashCode();
            return result;
        }
    }
}