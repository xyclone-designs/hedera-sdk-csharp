// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Cryptography;

using System;

namespace Hedera.Hashgraph.SDK
{
    public sealed partial class Client 
    {
		public class Operator(AccountId accountId, PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
		{
			public AccountId AccountId { get; internal set; } = accountId;
			public PublicKey PublicKey { get; internal set; } = publicKey;
			public Func<byte[], byte[]> TransactionSigner { get; internal set; } = transactionSigner;
		}
	}
}