// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Hook type for fungible (HBAR and FT) transfers.
    /// </summary>
    public enum FungibleHookType
    {
        PreTxAllowanceHook,
        PrePostTxAllowanceHook
    }
}