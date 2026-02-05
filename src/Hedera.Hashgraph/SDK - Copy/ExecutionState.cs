// SPDX-License-Identifier: Apache-2.0

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
        Success,
        /// <summary>
        /// Indicates that the call was successful but the operation did not complete. Retry with same/new node
        /// </summary>
        Retry,
        /// <summary>
        /// Indicates that the receiver was bad node. Retry with new node
        /// </summary>
        ServerError,
        /// <summary>
        /// Indicates that the request was incorrect
        /// </summary>
        RequestError
    }
}