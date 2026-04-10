// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.SDK.Nfts
{
    /// <include file="NftHookCall.cs.xml" path='docs/member[@name="T:NftHookCall"]/*' />
    public class NftHookCall : HookCall
    {
        public NftHookCall(long hookId, EvmHookCall evmHookCall, NftHookType type) : base(hookId, evmHookCall)
        {
            Type = type;
        }

        public virtual NftHookType Type { get; }
    }
}