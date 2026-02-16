// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;

using System.Collections.Generic;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class DuplicateTransactionTest
    {
        public virtual void GenerateTransactionIds()
        {
            TransactionId[] ids = new TransactionId[1000000];
            AccountId accountId = AccountId.FromString("0.0.1000");

            for (int i = 0; i < ids.Length; ++i)
				ids[i] = TransactionId.Generate(accountId);

			HashSet<TransactionId> set = new (ids.Length);

            for (int i = 0; i < ids.Length; ++i)
				Assert.True(set.Add(ids[i]), $"ids[{i}] is not unique");
		}
	}
}