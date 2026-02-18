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
        public virtual void CanConnectToPreviewnetWithTLS()
        {
            var client = Client.ForPreviewnet().SetTransportSecurity(true);
            bool succeededAtLeastOnce = false;
            foreach (var entry in client.Network.EntrySet())
            {
                Assert.True(entry.GetKey().EndsWith(":50212"));
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
            Assert.True(succeededAtLeastOnce);
        }

        public virtual void CanConnectToTestnetWithTLS()
        {
            var client = Client.ForTestnet().SetTransportSecurity(true);
            bool succeededAtLeastOnce = false;
            foreach (var entry in client.Network.EntrySet())
            {
                Assert.True(entry.GetKey().EndsWith(":50212"));
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
            Assert.True(succeededAtLeastOnce);
        }

        public virtual void CanConnectToMainnetWithTLS()
        {
            var client = Client.ForMainnet().SetTransportSecurity(true);
            bool succeededAtLeastOnce = false;
            foreach (var entry in client.Network.EntrySet())
            {
                Assert.True(entry.GetKey().EndsWith(":50212"));
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
            Assert.True(succeededAtLeastOnce);
        }

        public virtual void CannotConnectToPreviewnetWhenNetworkNameIsNullAndCertificateVerificationIsEnabled()
        {
            var client = Client.ForPreviewnet().SetTransportSecurity(true).SetVerifyCertificates(true).SetNetworkName(null);
            Assert.False(client.Network.IsEmpty());
            foreach (var entry in client.Network.EntrySet())
            {
                Assert.True(entry.GetKey().EndsWith(":50212"));
                Assert.Throws(typeof(InvalidOperationException), () =>
                {
                    new AccountBalanceQuery().SetNodeAccountIds(Collections.SingletonList(entry.GetValue())).SetAccountId(entry.GetValue()).Execute(client);
                });
            }

            client.Dispose();
        }

        public virtual void CanFetchBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).Execute(testEnv.Client);
                Assert.True(balance.hbars.ToTinybars() > 0);
            }
        }

        public virtual void GetCostBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).SetMaxQueryPayment(new Hbar(1));
                var cost = balance.GetCost(testEnv.Client);
                var accBalance = balance.SetQueryPayment(cost).Execute(testEnv.Client);
                Assert.True(accBalance.hbars.ToTinybars() > 0);
                Assert.Equal(cost.ToTinybars(), 0);
            }
        }

        public virtual void GetCostBigMaxBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).SetMaxQueryPayment(new Hbar(1000000));
                var cost = balance.GetCost(testEnv.Client);
                var accBalance = balance.SetQueryPayment(cost).Execute(testEnv.Client);
                Assert.True(accBalance.hbars.ToTinybars() > 0);
            }
        }

        public virtual void GetCostSmallMaxBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery().SetAccountId(testEnv.OperatorId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                var cost = balance.GetCost(testEnv.Client);
                var accBalance = balance.SetQueryPayment(cost).Execute(testEnv.Client);
                Assert.True(accBalance.hbars.ToTinybars() > 0);
            }
        }

        public virtual void CanNotFetchBalanceForInvalidAccountId()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new AccountBalanceQuery().SetAccountId(AccountId.FromString("1.0.3")).Execute(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_ACCOUNT_ID.ToString());
            }
        }

        public virtual void CanFetchTokenBalancesForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetInitialSupply(10000).SetDecimals(50)TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var query = new AccountBalanceQuery();
                var balance = query.SetAccountId(testEnv.OperatorId).Execute(testEnv.Client);
                Assert.Equal(balance.tokens[tokenId], 10000);
                Assert.Equal(balance.tokenDecimals[tokenId], 50);
                Assert.NotEmpty(query.ToString());
                Assert.Null(query.GetPaymentTransactionId());
            }
        }
    }
}