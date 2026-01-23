// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Hook type for fungible (HBAR and FT) transfers.
    /// </summary>
    public enum FungibleHookType
    {
        PRE_TX_ALLOWANCE_HOOK,
        PRE_POST_TX_ALLOWANCE_HOOK
    }
}