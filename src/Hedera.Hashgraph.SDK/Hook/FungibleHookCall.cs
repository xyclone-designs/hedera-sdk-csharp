// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="FungibleHookCall.cs.xml" path='docs/member[@name="T:FungibleHookCall"]/*' />
    public class FungibleHookCall : HookCall
    {
        public FungibleHookCall(long hookId, EvmHookCall evmHookCall, FungibleHookType type) : base(hookId, evmHookCall)
        {
            Type = type;
        }

        public virtual FungibleHookType Type { get; }
    }
}