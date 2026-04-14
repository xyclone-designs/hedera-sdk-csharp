// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.Reference.Error;
using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="BadKeyException.cs.xml" path='docs/member[@name="T:BadKeyException"]/*' />
    public class BadKeyException : Exception, IBadKey 
    {
        public BadKeyException(string message) : base(message) { }
        public BadKeyException(Exception exception) : base(exception.Message, exception) { }
    }
}