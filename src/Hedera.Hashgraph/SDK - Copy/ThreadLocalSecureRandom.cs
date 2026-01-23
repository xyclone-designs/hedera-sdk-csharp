// SPDX-License-Identifier: Apache-2.0
using Org.BouncyCastle.Security;

using System.Threading;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Internal utility class.
    /// </summary>
    sealed class ThreadLocalSecureRandom
    {
        private static readonly ThreadLocal<SecureRandom> secureRandom = new AnonymousThreadLocal(this);
        private sealed class AnonymousThreadLocal : ThreadLocal<SecureRandom>
		{
            public AnonymousThreadLocal(ThreadLocalSecureRandom parent)
            {
                parent = parent;
            }

            private readonly ThreadLocalSecureRandom parent;
            protected SecureRandom InitialValue()
            {
                return new SecureRandom();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        private ThreadLocalSecureRandom() { }

        /// <summary>
        /// Extract seme randomness.
        /// </summary>
        /// <returns>                         some randomness</returns>
        internal static SecureRandom Current()
        {
            return secureRandom.Get();
        }
    }
}