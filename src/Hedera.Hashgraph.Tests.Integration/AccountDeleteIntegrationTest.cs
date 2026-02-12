// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountDeleteIntegrationTest
    {
        virtual void CanDeleteAccount()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(info.accountId, accountId);
                AssertThat(info.isDeleted).IsFalse();
                Assert.Equal(info.key.ToString(), key.GetPublicKey().ToString());
                Assert.Equal(info.balance, new Hbar(1));
                Assert.Equal(info.autoRenewPeriod, Duration.OfDays(90));
                AssertThat(info.proxyAccountId).IsNull();
                Assert.Equal(info.proxyReceived, Hbar.ZERO);
            }
        }

        virtual void CannotCreateAccountWithNoKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new AccountDeleteTransaction().SetTransferAccountId(testEnv.operatorId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.ACCOUNT_ID_DOES_NOT_EXIST.ToString());
            }
        }

        virtual void CannotDeleteAccountThatHasNotSignedTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new AccountDeleteTransaction().SetAccountId(accountId).SetTransferAccountId(testEnv.operatorId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }
    }
}