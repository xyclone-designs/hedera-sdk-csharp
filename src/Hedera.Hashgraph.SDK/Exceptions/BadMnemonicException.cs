// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.Reference.Cryptography;
using Hedera.Hashgraph.Reference.Error;
using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="BadKeyException.cs.xml" path='docs/member[@name="T:BadKeyException"]/*' />
    public class BadMnemonicException : Exception, IBadMnemonic
    {
        internal BadMnemonicException(IMnemonic mnemonic, BadMnemonicReason reason, params long[] unknownWordIndicies)
        {
            Mnemonic = mnemonic;
            Reason = reason;
            UnknownWordIndicies = unknownWordIndicies;
        }

        public IMnemonic Mnemonic { get; }
        public BadMnemonicReason Reason { get; }
        public long[] UnknownWordIndicies { get; }
    }
}