// SPDX-License-Identifier: Apache-2.0
using Java.Util.Concurrent;
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
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;

namespace Hedera.Hashgraph.SDK.Queries
{
    public class MirrorNodeContractEstimateGasQuery : MirrorNodeContractQuery<MirrorNodeContractEstimateGasQuery>
    {
        /// <summary>
        /// Returns gas estimation for the EVM execution.
        /// </summary>
        /// <param name="client">The Client instance to perform the operation with</param>
        /// <returns>The estimated gas cost</returns>
        /// <exception cref="ExecutionException"></exception>
        /// <exception cref="InterruptedException"></exception>
        public virtual long Execute(Client client)
        {
            return Estimate(client);
        }

        public override string ToString()
        {
            return "MirrorNodeContractEstimateGasQuery" + base.ToString();
        }
    }
}