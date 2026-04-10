// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="MaxAttemptsExceededException.cs.xml" path='docs/member[@name="T:MaxAttemptsExceededException"]/*' />
    public class MaxAttemptsExceededException : InvalidOperationException
    {
		internal MaxAttemptsExceededException(Exception e) : base("exceeded maximum attempts for request with last exception being", e) { }
    }
}