// SPDX-License-Identifier: Apache-2.0
using Java.Util;
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

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// A typed hook call for fungible (HBAR and FT) transfers.
    /// </summary>
    public class FungibleHookCall : HookCall
    {
        private readonly FungibleHookType type;
        public FungibleHookCall(long hookId, EvmHookCall evmHookCall, FungibleHookType type) : base(hookId, evmHookCall)
        {
            this.type = Objects.RequireNonNull(type, "type cannot be null");
        }

        public virtual FungibleHookType GetType()
        {
            return type;
        }
    }
}