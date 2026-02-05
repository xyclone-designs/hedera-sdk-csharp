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
    class AccountBalanceIntegrationTest
    {
        virtual void CanConnectToPreviewnetWithTLS()
        {
            var client = Client.ForPreviewnet().SetTransportSecurity(true);
            bool succeededAtLeastOnce = false;
            foreach (var entry in client.GetNetwork().EntrySet())
            {
                AssertThat(entry.GetKey().EndsWith(":50212")).IsTrue();
                try
                {
                    new AccountBalanceQuery().SetMaxAttempts(1).SetNodeAccountIds(Collections.SingletonList(entry.GetValue())).SetAccountId(entry.GetValue()).Execute(client);
                    System.@out.Println("succeeded for " + entry);
                    succeededAtLeastOnce = true;
                }
                catch (Throwable error)
                {
                    System.@out.Println("failed for " + entry);
                }
            }

            client.Dispose();
            AssertThat(succeededAtLeastOnce).IsTrue();
        }

        virtual void CanConnectToTestnetWithTLS()
        {
            var client = Client.ForTestnet().SetTransportSecurity(true);
            bool succeededAtLeastOnce = false;
            foreach (var entry in client.GetNetwork().EntrySet())
            {
                AssertThat(entry.GetKey().EndsWith(":50212")).IsTrue();
                try
                {
                    new AccountBalanceQuery().SetMaxAttempts(1).SetNodeAccountIds(Collections.SingletonList(entry.GetValue())).SetAccountId(entry.GetValue()).Execute(client);
                    System.@out.Println("succeeded for " + entry);
                    succeededAtLeastOnce = true;
                }
                catch (Throwable error)
                {
                    System.@out.Println("failed for " + entry);
                }
            }

            client.Dispose();
            AssertThat(succeededAtLeastOnce).IsTrue();
        }

        virtual void CanConnectToMainnetWithTLS()
        {
            var client = Client.ForMainnet().SetTransportSecurity(true);
            bool succeededAtLeastOnce = false;
            foreach (var entry in client.GetNetwork().EntrySet())
            {
                AssertThat(entry.GetKey().EndsWith(":50212")).IsTrue();
                try
                {
                    new AccountBalanceQuery().SetMaxAttempts(1).SetNodeAccountIds(Collections.SingletonList(entry.GetValue())).SetAccountId(entry.GetValue()).Execute(client);
                    System.@out.Println("succeeded for " + entry);
                    succeededAtLeastOnce = true;
                }
                catch (Throwable error)
                {
                    System.@out.Println("failed for " + entry);
                    System.@out.Println(error);
                }
            }

            client.Dispose();
            AssertThat(succeededAtLeastOnce).IsTrue();
        }

        virtual void CannotConnectToPreviewnetWhenNetworkNameIsNullAndCertificateVerificationIsEnabled()
        {
            var client = Client.ForPreviewnet().SetTransportSecurity(true).SetVerifyCertificates(true).SetNetworkName(null);
            AssertThat(client.GetNetwork().IsEmpty()).IsFalse();
            foreach (var entry in client.GetNetwork().EntrySet())
            {
                AssertThat(entry.GetKey().EndsWith(":50212")).IsTrue();
                AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() =>
                {
                    new AccountBalanceQuery().SetNodeAccountIds(Collections.SingletonList(entry.GetValue())).SetAccountId(entry.GetValue()).Execute(client);
                });
            }

            client.Dispose();
        }

        virtual void CanFetchBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                AssertThat(balance.hbars.ToTinybars() > 0).IsTrue();
            }
        }

        virtual void GetCostBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).SetMaxQueryPayment(new Hbar(1));
                var cost = balance.GetCost(testEnv.client);
                var accBalance = balance.SetQueryPayment(cost).Execute(testEnv.client);
                AssertThat(accBalance.hbars.ToTinybars() > 0).IsTrue();
                Assert.Equal(cost.ToTinybars(), 0);
            }
        }

        virtual void GetCostBigMaxBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).SetMaxQueryPayment(new Hbar(1000000));
                var cost = balance.GetCost(testEnv.client);
                var accBalance = balance.SetQueryPayment(cost).Execute(testEnv.client);
                AssertThat(accBalance.hbars.ToTinybars() > 0).IsTrue();
            }
        }

        virtual void GetCostSmallMaxBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery().SetAccountId(testEnv.operatorId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                var cost = balance.GetCost(testEnv.client);
                var accBalance = balance.SetQueryPayment(cost).Execute(testEnv.client);
                AssertThat(accBalance.hbars.ToTinybars() > 0).IsTrue();
            }
        }

        virtual void CanNotFetchBalanceForInvalidAccountId()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new AccountBalanceQuery().SetAccountId(AccountId.FromString("1.0.3")).Execute(testEnv.client);
                }).WithMessageContaining(Status.INVALID_ACCOUNT_ID.ToString());
            }
        }

        virtual void CanFetchTokenBalancesForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetInitialSupply(10000).SetDecimals(50).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var query = new AccountBalanceQuery();
                var balance = query.SetAccountId(testEnv.operatorId).Execute(testEnv.client);
                Assert.Equal(balance.tokens[tokenId], 10000);
                Assert.Equal(balance.tokenDecimals[tokenId], 50);
                Assert.NotEmpty(query.ToString());
                AssertThat(query.GetPaymentTransactionId()).IsNull();
            }
        }
    }
}