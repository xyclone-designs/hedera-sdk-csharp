// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <summary>
    /// Utility exception class.
    /// </summary>
    public class MaxAttemptsExceededException : InvalidOperationException
    {
		internal MaxAttemptsExceededException(Exception e) : base("exceeded maximum attempts for request with last exception being", e) { }
    }
}