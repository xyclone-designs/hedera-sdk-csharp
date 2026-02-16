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
    public class TransactionResponseTest
    {
        public virtual void TransactionHashInTransactionRecordIsEqualToTheTransactionResponseTransactionHash()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var transaction = new AccountCreateTransaction().SetKeyWithoutAlias(key).Execute(testEnv.client);
                var record = transaction.GetRecord(testEnv.client);
                AssertThat(record.transactionHash.ToByteArray()).ContainsExactly(transaction.transactionHash);
                var accountId = record.receipt.accountId;
                AssertThat(accountId).IsNotNull();
            }
        }
    }
}