// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.SDK.Nfts
{
    /// <summary>
    /// A typed hook call for NFT transfers.
    /// </summary>
    public class NftHookCall : HookCall
    {
        public NftHookCall(long hookId, EvmHookCall evmHookCall, NftHookType type) : base(hookId, evmHookCall)
        {
            Type = type;
        }

        public virtual NftHookType Type { get; }
    }
}