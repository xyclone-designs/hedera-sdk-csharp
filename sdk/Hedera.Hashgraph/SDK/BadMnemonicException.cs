using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Custom exception for when there are issues with the mnemonic.
     */
    public class BadMnemonicException : Exception 
    {
		/**
         * Constructor.
         *
         * @param mnemonic                  the mnemonic
         * @param reason                    the reason
         */
		BadMnemonicException(Mnemonic mnemonic, BadMnemonicReason reason)
		{
			Mnemonic = mnemonic;
			Reason = reason;
			UnknownWordIndices = null;
		}
		/**
         * Constructor.
         *
         * @param mnemonic                  the mnemonic
         * @param reason                    the reason
         * @param unknownWordIndices        the indices
         */
		BadMnemonicException(Mnemonic mnemonic, BadMnemonicReason reason, List<int> unknownWordIndices)
		{
			Mnemonic = mnemonic;
			Reason = reason;
			UnknownWordIndices = unknownWordIndices;
		}

		/**
         * The mnemonic that failed validation.
         */
		public Mnemonic Mnemonic { get; }
        /**
         * The reason for which the mnemonic failed validation.
         */
        public BadMnemonicReason Reason { get; }
	    /**
         * If not null, these are the indices in the mnemonic that were not found in the
         * BIP-39 standard English word list.
         * <p>
         * If {@code reason == BadMnemonicReason.UnknownWords} then this will be not null.
         */
	    public List<int>? UnknownWordIndices { get; }
    }
}