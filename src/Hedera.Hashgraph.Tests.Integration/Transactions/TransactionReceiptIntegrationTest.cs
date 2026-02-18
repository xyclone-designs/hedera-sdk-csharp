// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TransactionReceiptIntegrationTest
    {
        public virtual void NextExchangeRatePropertyIsNotNullInTransactionReceipt()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction()
                    .SetKeyWithoutAlias(key)
                    .Execute(testEnv.Client);
                var receipt = transaction.GetReceipt(testEnv.Client);
                var nextExchangeRate = receipt.NextExchangeRate;

                Assert.NotNull(nextExchangeRate);
            }
        }
    }
}