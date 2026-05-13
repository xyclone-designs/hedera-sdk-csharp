// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="FungibleHookCall.cs.xml" path='docs/member[@name="T:FungibleHookCall"]' />
    public class FungibleHookCall(long hookId, EvmHookCall evmHookCall, FungibleHookType type) : HookCall(hookId, evmHookCall)
    {
        public virtual FungibleHookType Type { get; } = type;
    }
}