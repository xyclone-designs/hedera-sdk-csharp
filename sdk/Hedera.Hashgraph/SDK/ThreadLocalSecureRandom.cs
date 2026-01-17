using Org.BouncyCastle.Security;
using System.Threading;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Internal utility class.
     */
	public static class ThreadLocalSecureRandom
	{
		private static readonly ThreadLocal<SecureRandom> _secureRandom = new (() => new SecureRandom());

		/**
         * Extract seme randomness.
         *
         * @return                          some randomness
         */
		public static SecureRandom Current()
		{
			// Use the .Value property instead of .get()
			return _secureRandom.Value!;
		}
	}    
}