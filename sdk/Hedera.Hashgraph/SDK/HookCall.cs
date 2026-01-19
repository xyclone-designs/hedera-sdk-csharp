using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Specifies a call to a hook from within a transaction.
     * <p>
     * Often the hook's entity is implied by the nature of the call site. For example, when using an account allowance hook
     * inside a crypto transfer, the hook's entity is necessarily the account whose authorization is required.
     * <p>
     * For future extension points where the hook owner is not forced by the context, we include the option to fully
     * specify the hook id for the call.
     */
    public abstract class HookCall 
    {
        /**
         * Create a HookCall with a numeric hook ID.
         *
         * @param hookId the numeric ID of the hook to call
         * @param evmHookCall the EVM hook call details
         */
        protected HookCall(long? hookId, EvmHookCall evmHookCall) 
        {
            HookId = hookId;
            EvmHookCall = evmHookCall;
        }

		public long? HookId { get; }
		public EvmHookCall EvmHookCall { get; }

		/**
         * Convert this HookCall to a protobuf message.
         *
         * @return the protobuf HookCall
         */
		public Proto.HookCall ToProtobuf() 
        {
            Proto.HookCall proto = new()
            {
                EvmHookCall = EvmHookCall.ToProtobuf(),
            };

            if (HookId.HasValue) proto.HookId = HookId.Value;

            return proto;
        }

        public override int GetHashCode()
        {
			int result = HashCode.Combine(HookId);
			result = 31 * result + EvmHookCall.GetHashCode();
			return result;
        }
        public override bool Equals(object? obj)
        {
			if (this == obj) return true;
			if (obj == null || GetType() != obj.GetType()) return false;

			HookCall hookCall = (HookCall)obj;

			return Equals(HookId, hookCall.HookId) && EvmHookCall.Equals(hookCall.EvmHookCall);
		}
    }
}