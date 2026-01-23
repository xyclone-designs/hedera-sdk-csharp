// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Specifies a call to a hook from within a transaction.
    /// <p>
    /// Often the hook's entity is implied by the nature of the call site. For example, when using an account allowance hook
    /// inside a crypto transfer, the hook's entity is necessarily the account whose authorization is required.
    /// <p>
    /// For future extension points where the hook owner is not forced by the context, we include the option to fully
    /// specify the hook id for the call.
    /// </summary>
    public abstract class HookCall
    {
        /// <summary>
        /// Create a HookCall with a numeric hook ID.
        /// </summary>
        /// <param name="hookId">the numeric ID of the hook to call</param>
        /// <param name="evmHookCall">the EVM hook call details</param>
        protected HookCall(long hookId, EvmHookCall evmHookCall)
        {
            HookId = hookId;
            EvmHookCall = evmHookCall;
        }

        /// <summary>
        /// Get the numeric hook ID.
        /// </summary>
        /// <returns>the numeric hook ID, or null if using full hook ID</returns>
        public virtual long HookId { get; }

        /// <summary>
        /// Get the EVM hook call details.
        /// </summary>
        /// <returns>the EVM hook call details</returns>
        public virtual EvmHookCall EvmHookCall { get; }

        /// <summary>
        /// Convert this HookCall to a protobuf message.
        /// </summary>
        /// <returns>the protobuf HookCall</returns>
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
            if (o == null || GetType() != o.GetType())
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