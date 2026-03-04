// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="BadKeyException.cs.xml" path='docs/member[@name="T:BadKeyException"]/*' />
    public sealed class BadKeyException : ArgumentException
    {
        /// <include file="BadKeyException.cs.xml" path='docs/member[@name="M:BadKeyException.#ctor(System.String)"]/*' />
        internal BadKeyException(string message) : base(message) { }

		/// <include file="BadKeyException.cs.xml" path='docs/member[@name="M:BadKeyException.#ctor(System.Exception)"]/*' />
		internal BadKeyException(Exception cause) : base(cause.Message, cause) { }
    }
}