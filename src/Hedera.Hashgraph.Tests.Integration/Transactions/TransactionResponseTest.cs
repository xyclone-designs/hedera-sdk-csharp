// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TransactionResponseTest
    {
        public virtual void TransactionHashInTransactionRecordIsEqualToTheTransactionResponseTransactionHash()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction()
                    Key = key,
                    .Execute(testEnv.Client);
                var record = transaction.GetRecord(testEnv.Client);
                
                AssertThat(record.TransactionHash.ToByteArray()).ContainsExactly(transaction.TransactionHash);
                
                var accountId = record.Receipt.AccountId;

                Assert.NotNull(accountId);
            }
        }
    }
}