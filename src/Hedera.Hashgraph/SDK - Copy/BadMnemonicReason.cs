// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Possible reason why a {@link Mnemonic} failed validation.
    /// </summary>
    public enum BadMnemonicReason
    {
        /// <summary>
        /// The mnemonic did not contain exactly 24 words.
        /// </summary>
        BadLength,
        /// <summary>
        /// The mnemonic contained words which were not found in the BIP-39 standard English word list.
        /// <p>
        /// {@link BadMnemonicException#unknownWordIndices} will be set with the list of word indices
        /// in {@link Mnemonic#words} which were not found in the standard word list.
        /// </summary>
        /// <remarks>
        /// @see<a href="https://github.com/bitcoin/bips/blob/master/bip-0039/english.txt">BIP-39
        /// English word list</a>.
        /// </remarks>
        UnknownWords,
        /// <summary>
        /// The checksum encoded in the mnemonic did not match the checksum we just calculated for
        /// that mnemonic.
        /// <p>
        /// 24-word mnemonics have an 8-bit checksum that is appended to the 32 bytes of source entropy
        /// after being calculated from it, before being encoded into words. This status is returned if
        /// {@link Mnemonic#validate()} calculated a different checksum for the mnemonic than that which
        /// was encoded into it.
        /// <p>
        /// This could happen if two or more of the words were entered out of the original order or
        /// replaced with another from the standard word list (as this is only returned if all the words
        /// exist in the word list).
        /// </summary>
        ChecksumMismatch,
        /// <summary>
        /// The given mnemonic doesn't contain 22 words required to be a legacy mnemonic, or the words are
        /// not in the legacy list.
        /// </summary>
        NotLegacy
    }
}