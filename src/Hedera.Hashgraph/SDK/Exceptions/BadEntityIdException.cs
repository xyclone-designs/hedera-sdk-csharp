// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <summary>
    /// Custom exception thrown by the entity helper validate method when the account id and checksum are invalid.
    /// </summary>
    public class BadEntityIdException : Exception
    {
        /// <summary>
        /// the shard portion of the account id
        /// </summary>
        public readonly long Shard;
        /// <summary>
        /// the realm portion of the account id
        /// </summary>
        public readonly long Realm;
        /// <summary>
        /// the num portion of the account id
        /// </summary>
        public readonly long Num;
        /// <summary>
        /// the user supplied checksum
        /// </summary>
        public readonly string PresentChecksum;
        /// <summary>
        /// the calculated checksum
        /// </summary>
        public readonly string ExpectedChecksum;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shard">the shard portion of the account id</param>
		/// <param name="realm">the realm portion of the account id</param>
		/// <param name="num">the num portion of the account id</param>
		/// <param name="presentChecksum">the user supplied checksum</param>
		/// <param name="expectedChecksum">the calculated checksum</param>
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