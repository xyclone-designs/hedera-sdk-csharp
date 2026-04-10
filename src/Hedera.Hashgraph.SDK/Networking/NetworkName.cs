// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <include file="NetworkName.cs.xml" path='docs/member[@name="T:NetworkName"]/*' />
	public enum NetworkName
    {
        /// <include file="NetworkName.cs.xml" path='docs/member[@name="T:NetworkName_2"]/*' />
        [Obsolete]
        MainNet = 0,
        /// <include file="NetworkName.cs.xml" path='docs/member[@name="T:NetworkName_3"]/*' />
        [Obsolete]
        TestNet = 1,
        /// <include file="NetworkName.cs.xml" path='docs/member[@name="T:NetworkName_4"]/*' />
        [Obsolete]
        PreviewNet = 2,
        /// <include file="NetworkName.cs.xml" path='docs/member[@name="T:NetworkName_5"]/*' />
        [Obsolete]
        Other = int.MaxValue
    }
}