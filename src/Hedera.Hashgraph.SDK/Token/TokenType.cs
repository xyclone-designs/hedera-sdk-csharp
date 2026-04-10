// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenType.cs.xml" path='docs/member[@name="T:TokenType"]/*' />
    public enum TokenType
    {
        /// <include file="TokenType.cs.xml" path='docs/member[@name="M:TokenType.properties(e.g. serial)"]/*' />
        FungibleCommon = Proto.TokenType.FungibleCommon,

		/// <include file="TokenType.cs.xml" path='docs/member[@name="T:TokenType_2"]/*' />
		NonFungibleUnique = Proto.TokenType.NonFungibleUnique,
    }
}