// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <summary>
    /// Signals that a key could not be realized from the given input.
    /// <p>
    /// This exception can be raised by any of the {@code from} methods
    /// on {@link PrivateKey} or {@link PublicKey}.
    /// </summary>
    public sealed class BadKeyException : ArgumentException
    {
        /// <summary>
        /// </summary>
        /// <param name="message">the message</param>
        internal BadKeyException(string message) : base(message) { }

		/// <summary>
		/// </summary>
		/// <param name="cause">the cause</param>
		internal BadKeyException(Exception cause) : base(cause.Message, cause) { }
    }
}