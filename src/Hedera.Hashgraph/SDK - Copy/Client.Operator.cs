// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions.Account;

using System;

namespace Hedera.Hashgraph.SDK
{
    public partial sealed class Client 
    {
		private class Operator
		{
			readonly AccountId accountId;
			readonly PublicKey publicKey;
			readonly Func<byte[], byte[]> transactionSigner;
			
			public Operator(AccountId accountId, PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
			{
				accountId = accountId;
				publicKey = publicKey;
				transactionSigner = transactionSigner;
			}
		}
	}
}