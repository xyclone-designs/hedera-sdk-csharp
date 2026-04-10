// SPDX-License-Identifier: Apache-2.0
using Org.BouncyCastle.Security;

using System.Threading;

namespace Hedera.Hashgraph.SDK
{
	internal static class ThreadLocalSecureRandom
	{
		private static readonly ThreadLocal<SecureRandom> SecureRandom = new(() => new SecureRandom());

		internal static SecureRandom Current()
		{
			return SecureRandom.Value!;
		}
	}
}