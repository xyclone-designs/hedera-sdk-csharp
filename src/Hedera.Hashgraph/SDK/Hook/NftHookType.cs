// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <summary>
    /// Hook type for NFT transfers, indicating side (sender/receiver) and timing (pre / pre-post).
    /// </summary>
    public enum NftHookType
    {
        PreHookSender,
        PrePostHookSender,
        PreHookReceiver,
        PrePostHookReceiver
    }
}