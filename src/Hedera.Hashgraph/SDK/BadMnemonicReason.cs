// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK
{
    /// <include file="BadMnemonicReason.cs.xml" path='docs/member[@name="T:BadMnemonicReason"]/*' />
    public enum BadMnemonicReason
    {
        /// <include file="BadMnemonicReason.cs.xml" path='docs/member[@name="T:BadMnemonicReason_2"]/*' />
        BadLength,
        /// <include file="BadMnemonicReason.cs.xml" path='docs/member[@name="M:BadMnemonicReason.validate"]/*' />
        UnknownWords,
        /// <include file="BadMnemonicReason.cs.xml" path='docs/member[@name="T:BadMnemonicReason_3"]/*' />
        ChecksumMismatch,
        /// <include file="BadMnemonicReason.cs.xml" path='docs/member[@name="T:BadMnemonicReason_4"]/*' />
        NotLegacy
    }
}