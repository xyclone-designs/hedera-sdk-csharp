// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="BadEntityIdException.cs.xml" path='docs/member[@name="T:BadEntityIdException"]/*' />
    public class BadEntityIdException : Exception
    {
        /// <include file="BadEntityIdException.cs.xml" path='docs/member[@name="F:BadEntityIdException.Shard"]/*' />
        public readonly long Shard;
        /// <include file="BadEntityIdException.cs.xml" path='docs/member[@name="F:BadEntityIdException.Realm"]/*' />
        public readonly long Realm;
        /// <include file="BadEntityIdException.cs.xml" path='docs/member[@name="F:BadEntityIdException.Num"]/*' />
        public readonly long Num;
        /// <include file="BadEntityIdException.cs.xml" path='docs/member[@name="M:BadEntityIdException.#ctor(System.Int64,System.Int64,System.Int64,System.String,System.String)"]/*' />
        public readonly string PresentChecksum;
        /// <include file="BadEntityIdException.cs.xml" path='docs/member[@name="M:BadEntityIdException.#ctor(System.Int64,System.Int64,System.Int64,System.String,System.String)_2"]/*' />
        public readonly string ExpectedChecksum;
		/// <include file="BadEntityIdException.cs.xml" path='docs/member[@name="M:BadEntityIdException.#ctor(System.Int64,System.Int64,System.Int64,System.String,System.String)_3"]/*' />
		internal BadEntityIdException(long shard, long realm, long num, string presentChecksum, string expectedChecksum) : base(string.Format("Entity ID {0}.{1}.{2}-{3} was incorrect.", shard, realm, num, presentChecksum))
        {
            Shard = shard;
            Realm = realm;
            Num = num;
            PresentChecksum = presentChecksum;
            ExpectedChecksum = expectedChecksum;
        }
    }
}