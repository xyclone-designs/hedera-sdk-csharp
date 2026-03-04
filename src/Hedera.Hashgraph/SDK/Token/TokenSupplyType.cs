// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenSupplyType.cs.xml" path='docs/member[@name="T:TokenSupplyType"]/*' />
    public enum TokenSupplyType
    {
		/// <include file="TokenSupplyType.cs.xml" path='docs/member[@name="T:TokenSupplyType_2"]/*' />
		Infinite = Proto.TokenSupplyType.Infinite,

        /// <include file="TokenSupplyType.cs.xml" path='docs/member[@name="T:TokenSupplyType_3"]/*' />
        Finite = Proto.TokenSupplyType.Finite, 
    }
}