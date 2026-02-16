// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TransactionReceiptIntegrationTest
    {
        public virtual void NextExchangeRatePropertyIsNotNullInTransactionReceipt()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.client);
                var receipt = transaction.GetReceipt(testEnv.client);
                var nextExchangeRate = receipt.nextExchangeRate;
                AssertThat(nextExchangeRate).IsNotNull();
            }
        }
    }
}