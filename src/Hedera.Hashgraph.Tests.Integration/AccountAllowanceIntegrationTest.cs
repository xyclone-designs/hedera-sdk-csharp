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
    public class AccountAllowanceIntegrationTest
    {
        virtual void CanSpendHbarAllowance()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var aliceKey = PrivateKey.GenerateED25519();
                var aliceId = new AccountCreateTransaction().SetKeyWithoutAlias(aliceKey).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var bobKey = PrivateKey.GenerateED25519();
                var bobId = new AccountCreateTransaction().SetKeyWithoutAlias(bobKey).SetInitialBalance(new Hbar(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                Objects.RequireNonNull(aliceId);
                Objects.RequireNonNull(bobId);
                new AccountAllowanceApproveTransaction().ApproveHbarAllowance(bobId, aliceId, new Hbar(10)).FreezeWith(testEnv.client).Sign(bobKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var transferRecord = new TransferTransaction().AddHbarTransfer(testEnv.operatorId, new Hbar(5)).AddApprovedHbarTransfer(bobId, new Hbar(5).Negated()).SetTransactionId(TransactionId.Generate(aliceId)).FreezeWith(testEnv.client).Sign(aliceKey).Execute(testEnv.client).GetRecord(testEnv.client);
                var transferFound = false;
                foreach (var transfer in transferRecord.transfers)
                {
                    if (transfer.accountId.Equals(testEnv.operatorId) && transfer.amount.Equals(new Hbar(5)))
                    {
                        transferFound = true;
                        break;
                    }
                }

                AssertThat(transferFound).IsTrue();
                new AccountDeleteTransaction().SetAccountId(bobId).SetTransferAccountId(testEnv.operatorId).FreezeWith(testEnv.client).Sign(bobKey).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }
    }
}