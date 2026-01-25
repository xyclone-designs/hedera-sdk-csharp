// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// A typed hook call for fungible (HBAR and FT) transfers.
    /// </summary>
    public class FungibleHookCall : HookCall
    {
        public FungibleHookCall(long hookId, EvmHookCall evmHookCall, FungibleHookType type) : base(hookId, evmHookCall)
        {
            Type = type;
        }

        public virtual FungibleHookType Type { get; }
    }
}