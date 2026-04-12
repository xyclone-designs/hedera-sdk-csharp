// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK
{
    /// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType"]/*' />
    public enum FreezeType
    {
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType_2"]/*' />
		UnknownFreezeType = Proto.Services.FreezeType.UnknownFreezeType,
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="M:FreezeType.completes(dependent on the size /// of the upgrade)"]/*' />
		FreezeOnly = Proto.Services.FreezeType.FreezeOnly,
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType_3"]/*' />
		PrepareUpgrade = Proto.Services.FreezeType.PrepareUpgrade,
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="M:FreezeType.default(empty)"]/*' />
		FreezeUpgrade = Proto.Services.FreezeType.FreezeUpgrade,
		/// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType_4"]/*' />
		FreezeAbort = Proto.Services.FreezeType.FreezeAbort,
        /// <include file="FreezeType.cs.xml" path='docs/member[@name="T:FreezeType_5"]/*' />
        TelemetryUpgrade = Proto.Services.FreezeType.TelemetryUpgrade,
    }
}
