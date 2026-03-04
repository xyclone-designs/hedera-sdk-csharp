// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="BadMnemonicException.cs.xml" path='docs/member[@name="T:BadMnemonicException"]/*' />
    public class BadMnemonicException : Exception
    {
        /// <include file="BadMnemonicException.cs.xml" path='docs/member[@name="F:BadMnemonicException.Mnemonic"]/*' />
        public readonly Mnemonic Mnemonic;
        /// <include file="BadMnemonicException.cs.xml" path='docs/member[@name="M:BadMnemonicException.#ctor(Mnemonic,BadMnemonicReason,System.Collections.Generic.IEnumerable{System.Int32})"]/*' />
        public readonly BadMnemonicReason Reason;
        /// <include file="BadMnemonicException.cs.xml" path='docs/member[@name="M:BadMnemonicException.#ctor(Mnemonic,BadMnemonicReason,System.Collections.Generic.IEnumerable{System.Int32})_2"]/*' />
        public readonly List<int>? UnknownWordIndices;
		/// <include file="BadMnemonicException.cs.xml" path='docs/member[@name="M:BadMnemonicException.#ctor(Mnemonic,BadMnemonicReason,System.Collections.Generic.IEnumerable{System.Int32})_3"]/*' />
		internal BadMnemonicException(Mnemonic mnemonic, BadMnemonicReason reason, IEnumerable<int> unknownWordIndices)
        {
            Mnemonic = mnemonic;
            Reason = reason;
            UnknownWordIndices = [.. unknownWordIndices];
        }

		/// <include file="BadMnemonicException.cs.xml" path='docs/member[@name="M:BadMnemonicException.#ctor(Mnemonic,BadMnemonicReason)"]/*' />
		internal BadMnemonicException(Mnemonic mnemonic, BadMnemonicReason reason)
        {
            Mnemonic = mnemonic;
            Reason = reason;
            UnknownWordIndices = null;
        }
    }
}