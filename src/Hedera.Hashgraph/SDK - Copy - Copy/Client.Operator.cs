// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;

using System;

namespace Hedera.Hashgraph.SDK
{
    public sealed partial class Client 
    {
		public class Operator
		{
			internal AccountId AccountId { get; }
			internal PublicKey PublicKey { get; }
			internal Func<byte[], byte[]> TransactionSigner { get; }
			
			public Operator(AccountId accountId, PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
			{
				AccountId = accountId;
				PublicKey = publicKey;
				TransactionSigner = transactionSigner;
			}
		}
	}
}