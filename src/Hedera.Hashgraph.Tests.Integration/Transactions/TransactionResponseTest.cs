// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TransactionResponseTest
    {
        [Fact]
        public virtual void TransactionHashInTransactionRecordIsEqualToTheTransactionResponseTransactionHash()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction
                {
					Key = key,
				
                }.Execute(testEnv.Client);
                var record = transaction.GetRecord(testEnv.Client);
                
                Assert.Equal(record.TransactionHash.ToByteArray(), transaction.TransactionHash);
                
                var accountId = record.Receipt.AccountId;

                Assert.NotNull(accountId);
            }
        }
    }
}