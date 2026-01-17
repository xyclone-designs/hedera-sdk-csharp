using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Signals that a key could not be realized from the given input.
     * <p>
     * This exception can be raised by any of the {@code from} methods
     * on {@link PrivateKey} or {@link PublicKey}.
     */
    public sealed class BadKeyException : ArgumentException
    {
        /**
         * @param message                   
         * the message
         */
        public BadKeyException(string message) : base(message) { }

        /**
         * @param cause                     
         * the cause
         */
        public BadKeyException(Exception cause) : base(cause.Message, cause) { }
    }
}