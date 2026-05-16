// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;

using System.Collections.Generic;
using Hedera.Hashgraph.SDK.Core;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    /// <include file="test-transactions-duplicate.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Transactions.DuplicateTransactionTest"]' />
    public class DuplicateTransactionTest
    {
        [Fact]
        /// <include file="test-transactions-duplicate.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Transactions.DuplicateTransactionTest.GenerateTransactionIds"]' />
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
