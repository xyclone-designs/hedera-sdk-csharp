// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Enum for the execution states.
    /// </summary>
    public enum ExecutionState
    {
        /// <summary>
        /// Indicates that the execution was successful
        /// </summary>
        SUCCESS,
        /// <summary>
        /// Indicates that the call was successful but the operation did not complete. Retry with same/new node
        /// </summary>
        RETRY,
        /// <summary>
        /// Indicates that the receiver was bad node. Retry with new node
        /// </summary>
        SERVER_ERROR,
        /// <summary>
        /// Indicates that the request was incorrect
        /// </summary>
        REQUEST_ERROR
    }
}