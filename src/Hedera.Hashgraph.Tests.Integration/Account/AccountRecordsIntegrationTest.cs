// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountRecordsIntegrationTest
    {
        public virtual void CanQueryAccountRecords()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                new TransferTransaction().AddHbarTransfer(testEnv.operatorId, new Hbar(1).Negated()).AddHbarTransfer(accountId, new Hbar(1)).Execute(testEnv.client).GetReceipt(testEnv.client);
                new TransferTransaction().AddHbarTransfer(testEnv.operatorId, new Hbar(1)).AddHbarTransfer(accountId, new Hbar(1).Negated()).FreezeWith(testEnv.client).Sign(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                var records = new AccountRecordsQuery().SetAccountId(testEnv.operatorId).SetQueryPayment(new Hbar(10)).Execute(testEnv.client);
                AssertThat(records.IsEmpty()).IsFalse();
            }
        }
    }
}