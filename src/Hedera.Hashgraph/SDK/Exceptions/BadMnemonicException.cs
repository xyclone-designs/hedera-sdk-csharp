// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <summary>
    /// Custom exception for when there are issues with the mnemonic.
    /// </summary>
    public class BadMnemonicException : Exception
    {
        /// <summary>
        /// The mnemonic that failed validation.
        /// </summary>
        public readonly Mnemonic Mnemonic;
        /// <summary>
        /// The reason for which the mnemonic failed validation.
        /// </summary>
        public readonly BadMnemonicReason Reason;
        /// <summary>
        /// If not null, these are the indices in the mnemonic that were not found in the
        /// BIP-39 standard English word list.
        /// <p>
        /// If {@code reason == BadMnemonicReason.UnknownWords} then this will be not null.
        /// </summary>
        public readonly List<int>? UnknownWordIndices;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mnemonic">the mnemonic</param>
		/// <param name="reason">the reason</param>
		/// <param name="unknownWordIndices">the indices</param>
		internal BadMnemonicException(Mnemonic mnemonic, BadMnemonicReason reason, IList<int> unknownWordIndices)
        {
            Mnemonic = mnemonic;
            Reason = reason;
            UnknownWordIndices = unknownWordIndices;
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mnemonic">the mnemonic</param>
		/// <param name="reason">the reason</param>
		internal BadMnemonicException(Mnemonic mnemonic, BadMnemonicReason reason)
        {
            Mnemonic = mnemonic;
            Reason = reason;
            UnknownWordIndices = null;
        }
    }
}