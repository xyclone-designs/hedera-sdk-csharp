// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Cryptocurrency;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    /// <include file="TransactionReceiptIntegrationTest.cs.xml" path="docs/member[@name="T:Hedera.Hashgraph.SDK.Tests.Integration.TransactionReceiptIntegrationTest"]" />
    public class TransactionReceiptIntegrationTest
    {
        [Fact]
        /// <include file="TransactionReceiptIntegrationTest.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.SDK.Tests.Integration.TransactionReceiptIntegrationTest.NextExchangeRatePropertyIsNotNullInTransactionReceipt"]" />
        public virtual void NextExchangeRatePropertyIsNotNullInTransactionReceipt()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction
                {
					Key = key,
				
                }.Execute(testEnv.Client);
                var receipt = transaction.GetReceipt(testEnv.Client);
                var nextExchangeRate = receipt.NextExchangeRate;

                Assert.NotNull(nextExchangeRate);
            }
        }
    }
}
