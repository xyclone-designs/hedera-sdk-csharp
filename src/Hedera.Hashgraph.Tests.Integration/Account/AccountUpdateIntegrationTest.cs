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
    class AccountUpdateIntegrationTest
    {
        public virtual void CanUpdateAccountWithNewKey()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key1 = PrivateKey.GenerateED25519();
                var key2 = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKey(key1).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(info.accountId, accountId);
                AssertThat(info.isDeleted).IsFalse();
                Assert.Equal(info.key.ToString(), key1.GetPublicKey().ToString());
                Assert.Equal(info.balance, new Hbar(0));
                Assert.Equal(info.autoRenewPeriod, Duration.OfDays(90));
                AssertThat(info.proxyAccountId).IsNull();
                Assert.Equal(info.proxyReceived, Hbar.ZERO);
                new AccountUpdateTransaction().SetAccountId(accountId).SetKey(key2.GetPublicKey()).FreezeWith(testEnv.client).Sign(key1).Sign(key2).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new AccountInfoQuery().SetAccountId(accountId).Execute(testEnv.client);
                Assert.Equal(info.accountId, accountId);
                AssertThat(info.isDeleted).IsFalse();
                Assert.Equal(info.key.ToString(), key2.GetPublicKey().ToString());
                Assert.Equal(info.balance, new Hbar(0));
                Assert.Equal(info.autoRenewPeriod, Duration.OfDays(90));
                AssertThat(info.proxyAccountId).IsNull();
                Assert.Equal(info.proxyReceived, Hbar.ZERO);
            }
        }

        public virtual void CannotUpdateAccountWhenAccountIdIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new AccountUpdateTransaction().Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.ACCOUNT_ID_DOES_NOT_EXIST.ToString());
            }
        }
    }
}