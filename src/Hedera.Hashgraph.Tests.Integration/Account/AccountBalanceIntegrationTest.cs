// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Exceptions;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountBalanceIntegrationTest
    {
        public virtual void CanConnectToPreviewnetWithTLS()
        {
            var client = Client.ForPreviewnet(client => client.TransportSecurity = true);
            bool succeededAtLeastOnce = false;
            foreach (var entry in client.Network_.Network_Read)
            {
                Assert.True(entry.Key.EndsWith(":50212"));
                try
                {
                    new AccountBalanceQuery()
                    {
						MaxAttempts = 1,
						NodeAccountIds = [entry.Value],
						AccountId = entry.Value,
					}
                    .Execute(client);
                    Console.WriteLine("succeeded for " + entry);
                    succeededAtLeastOnce = true;
                }
                catch (Exception error)
                {
                    Console.WriteLine("failed for " + entry);
                }
            }

            client.Dispose();
            Assert.True(succeededAtLeastOnce);
        }

        public virtual void CanConnectToTestnetWithTLS()
        {
            var client = Client.ForPreviewnet(client => client.TransportSecurity = true);
			bool succeededAtLeastOnce = false;
            foreach (var entry in client.Network_.Network_Read)
            {
                Assert.True(entry.Key.EndsWith(":50212"));
                try
                {
                    new AccountBalanceQuery()
                    {
						MaxAttempts = 1,
						NodeAccountIds = [entry.Value],
						AccountId = entry.Value,
					}
                    .Execute(client);
                    Console.WriteLine("succeeded for " + entry);
                    succeededAtLeastOnce = true;
                }
                catch (Exception error)
                {
                    Console.WriteLine("failed for " + entry);
                }
            }

            client.Dispose();
            Assert.True(succeededAtLeastOnce);
        }

        public virtual void CanConnectToMainnetWithTLS()
        {
            var client = Client.ForPreviewnet(client => client.TransportSecurity = true);
			bool succeededAtLeastOnce = false;
            foreach (var entry in client.Network_.Network_Read)
            {
                Assert.True(entry.Key.EndsWith(":50212"));
                try
                {
                    new AccountBalanceQuery()
                    {
						MaxAttempts = 1,
						NodeAccountIds = [entry.Value],
						AccountId = entry.Value,
					}
                    .Execute(client);
                    Console.WriteLine("succeeded for " + entry);
                    succeededAtLeastOnce = true;
                }
                catch (Exception error)
                {
                    Console.WriteLine("failed for " + entry);
                    Console.WriteLine(error);
                }
            }

            client.Dispose();
            Assert.True(succeededAtLeastOnce);
        }

        public virtual void CannotConnectToPreviewnetWhenNetworkNameIsNullAndCertificateVerificationIsEnabled()
        {
            var client = Client.ForPreviewnet(client =>
            {
                client.TransportSecurity = true;
                client.VerifyCertificates = true;
                client.NetworkName = null;  
            });

            Assert.NotEmpty(client.Network_.Network_Read);

            foreach (var entry in client.Network_.Network_Read)
            {
                Assert.True(entry.Key.EndsWith(":50212"));
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                {
                    new AccountBalanceQuery()
                    {
						NodeAccountIds = [entry.Value],
						AccountId = entry.Value,
					
                    }.Execute(client);
                });
            }

            client.Dispose();
        }

        public virtual void CanFetchBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId
				
                }.Execute(testEnv.Client);

                Assert.True(balance.Hbars.ToTinybars() > 0);
            }
        }

        public virtual void GetCostBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId,
					MaxQueryPayment = new Hbar(1),
				};
                var cost = balance.GetCost(testEnv.Client);
				balance.QueryPayment = cost;
				var accBalance = balance.Execute(testEnv.Client);
				Assert.True(accBalance.Hbars.ToTinybars() > 0);
                Assert.Equal(0, cost.ToTinybars());
            }
        }

        public virtual void GetCostBigMaxBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId,
					MaxQueryPayment = new Hbar(1000000),
				};
                var cost = balance.GetCost(testEnv.Client);
                balance.QueryPayment = cost;
                var accBalance = balance.Execute(testEnv.Client);
                Assert.True(accBalance.Hbars.ToTinybars() > 0);
            }
        }

        public virtual void GetCostSmallMaxBalanceForClientOperator()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                var balance = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId,
					MaxQueryPayment = Hbar.FromTinybars(1)
				};
                var cost = balance.GetCost(testEnv.Client);
                balance.QueryPayment = cost;
                var accBalance = balance.Execute(testEnv.Client);
                
                Assert.True(accBalance.Hbars.ToTinybars() > 0);
            }
        }

        public virtual void CanNotFetchBalanceForInvalidAccountId()
        {
            using (IntegrationTestEnv testEnv = new IntegrationTestEnv(1))
            {
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new AccountBalanceQuery
                    {
						AccountId = AccountId.FromString("1.0.3")

					}.Execute(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidAccountId.ToString(), exception.Message);
            }
        }

        public virtual void CanFetchTokenBalancesForClientOperator()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    InitialSupply = 10000,
                    Decimals = 50,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                    FreezeDefault = false
                
                }.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var query = new AccountBalanceQuery
                {
					AccountId = testEnv.OperatorId
				};
                var balance = query.Execute(testEnv.Client);
                Assert.Equal<ulong>(balance.Tokens[tokenId], 10000);
                Assert.Equal<uint>(balance.TokenDecimals[tokenId], 50);
                Assert.NotEmpty(query.ToString());
                Assert.Null(query.PaymentTransactionId);
            }
        }
    }
}