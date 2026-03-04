// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK
{
    /// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType"]/*' />
    public enum FreezeType
    {
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType_2"]/*' />
		UnknownFreezeType = Proto.FreezeType.UnknownFreezeType,
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="M:FreezeType.completes(dependent on the size /// of the upgrade)"]/*' />
		FreezeOnly = Proto.FreezeType.FreezeOnly,
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType_3"]/*' />
		PrepareUpgrade = Proto.FreezeType.PrepareUpgrade,
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="M:FreezeType.default(empty)"]/*' />
		FreezeUpgrade = Proto.FreezeType.FreezeUpgrade,
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType_4"]/*' />
		FreezeAbort = Proto.FreezeType.FreezeAbort,
        /// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType_5"]/*' />
        TelemetryUpgrade = Proto.FreezeType.TelemetryUpgrade,
    }
}