// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK
{
    /// <include file="ExecutionState.cs.xml" path='docs/member[@name="T:ExecutionState"]/*' />
    public enum ExecutionState
    {
        /// <include file="ExecutionState.cs.xml" path='docs/member[@name="T:ExecutionState_2"]/*' />
        Success,
        /// <include file="ExecutionState.cs.xml" path='docs/member[@name="T:ExecutionState_3"]/*' />
        Retry,
        /// <include file="ExecutionState.cs.xml" path='docs/member[@name="T:ExecutionState_4"]/*' />
        ServerError,
        /// <include file="ExecutionState.cs.xml" path='docs/member[@name="T:ExecutionState_5"]/*' />
        RequestError
    }
}