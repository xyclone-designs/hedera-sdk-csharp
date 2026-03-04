// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class ClientIntegrationTest
    {
        public virtual void FailsWhenNoNodesAreMatching()
        {
            var client = Client.ForTestnet(client => client.TransportSecurity = true);
            var nodes = new List<AccountId>();
            nodes.Add(new AccountId(0, 0, 1000));
            nodes.Add(new AccountId(0, 0, 1001));
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                return new AccountBalanceQuery
                {
					NodeAccountIds = [.. nodes],
					AccountId = new AccountId(0, 0, 7),
				
                }.Execute(client);

			}); Assert.Contains("All node account IDs did not map to valid nodes in the client's network", exception.Message);
         
            client.Dispose();
        }

        public virtual void CanSkipNodes()
        {
            var client = Client.ForTestnet(client => client.TransportSecurity = true);
            var nodes = new List<AccountId>(client.Network_.GetNetwork().Values);
            nodes.Add(new AccountId(0, 0, 1000));
            new AccountBalanceQuery
            {
				NodeAccountIds = [.. nodes],
				AccountId = new AccountId(0, 0, 7),

			}.Execute(client);

            client.Dispose();
        }

        public virtual void TestReplaceNodes()
        {
            Dictionary<string, AccountId> network = new()
            {
                { "0.testnet.hedera.com:50211", new AccountId(0, 0, 3) },
                { "1.testnet.hedera.com:50211", new AccountId(0, 0, 4) },
            };

            using (var testEnv = new IntegrationTestEnv(1))
            {
                testEnv.Client.DefaultMaxQueryPayment = new Hbar(2);
                testEnv.Client.RequestTimeout = TimeSpan.FromMinutes(2);
				testEnv.Client.Network_.SetNetwork(network);

				Assert.NotNull(testEnv.OperatorId);

                // Execute two simple queries so we create a channel for each network node.
                new AccountBalanceQuery { AccountId = new AccountId(0, 0, 3) }.Execute(testEnv.Client);
                new AccountBalanceQuery { AccountId = new AccountId(0, 0, 3) }.Execute(testEnv.Client);

				network = new Dictionary<string, AccountId>
				{
    				{ "1.testnet.hedera.com:50211", new AccountId(0, 0, 4) },
    				{ "2.testnet.hedera.com:50211", new AccountId(0, 0, 5) },
				};
				testEnv.Client.Network_.SetNetwork(network);

				network = new Dictionary<string, AccountId>
                {
                    { "35.186.191.247:50211", new AccountId(0, 0, 4) },
                    { "35.192.2.25:50211", new AccountId(0, 0, 5) },
                };
                testEnv.Client.Network_.SetNetwork(network);
            }
        }

        public virtual void TransactionIdNetworkIsVerified()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var client = Client.ForPreviewnet();
                client.AutoValidateChecksums = true;
                new AccountCreateTransaction
                {
					TransactionId = TransactionId.Generate(AccountId.FromString("0.0.123-esxsf"))
				
                }.Execute(client);

                client.Dispose();
            });
        }

        public virtual void TestMaxNodesPerTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                testEnv.Client.MaxNodeAttempts = 1;
                var transaction = new AccountDeleteTransaction { AccountId = testEnv.OperatorId, }.FreezeWith(testEnv.Client);
                Assert.NotNull(transaction.NodeAccountIds);
                Assert.Equal(transaction.NodeAccountIds.Count, 1);
            }
        }

        public virtual void Ping()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var network = testEnv.Client.Network_;
                var nodes = new List<AccountId>(network.GetNetwork().Values);
                Assert.NotEmpty(nodes);
                var node = nodes[0];
                testEnv.Client.MaxNodeAttempts = 1;
                testEnv.Client.Ping(node);
            }
        }

        public virtual void PingAll()
        {
            using (var testEnv = new IntegrationTestEnv())
            {
                testEnv.Client.MaxNodeAttempts = 1;
                testEnv.Client.PingAll();
                var network = testEnv.Client.Network_.GetNetwork();
				var nodes = new List<AccountId>(network.Values);
				Assert.NotEmpty(nodes);
                var node = nodes[0];
                new AccountBalanceQuery { AccountId = node }.Execute(testEnv.Client);
            }
        }

        public virtual void PingAllBadNetwork()
        {
            using (var testEnv = new IntegrationTestEnv(3))
            {
                // Skip if using local node.
                // Note: this check should be removed once the local node is supporting multiple nodes.
                testEnv.AssumeNotLocalNode();
                testEnv.Client.MaxAttempts = 1;
                testEnv.Client.MaxNodeAttempts = 2;
                var network = testEnv.Client.Network_.GetNetwork();
                var entries = new Dictionary<string, AccountId>(network);
                Assert.True(entries.Count > 1);
                network.Clear();
                network.Add("in-process:name", entries.ElementAt(0).Value);
                network.Add(entries.ElementAt(2).Key, entries.ElementAt(2).Value);
                testEnv.Client.Network_.SetNetwork(network);
                MaxAttemptsExceededException exception = Assert.Throws<MaxAttemptsExceededException>(() =>
                {
                    testEnv.Client.PingAll();

                }); Assert.Contains("exceeded maximum attempts", exception.Message);
                var nodes = new List<AccountId>(testEnv.Client.Network_.GetNetwork().Values);
                Assert.NotEmpty(nodes);
                var node = nodes[0];
                new AccountBalanceQuery
                {
                    AccountId = node
                }
                .Execute(testEnv.Client);
                Assert.Equal(testEnv.Client.Network_.GetNetwork().Values.Count, 1);
            }
        }

        public virtual void PingAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var network = testEnv.Client.Network_;
                var nodes = new List<AccountId>(network.GetNetwork().Values);
                Assert.False(nodes.Count == 0);
                var node = nodes[0];
                testEnv.Client.MaxNodeAttempts = 1;
                testEnv.Client.PingAsync(node).GetAwaiter().GetResult();
            }
        }

        public virtual void PingAllAsync()
        {
            using (var testEnv = new IntegrationTestEnv())
            {
                testEnv.Client.MaxNodeAttempts = 1;
                testEnv.Client.PingAllAsync().GetAwaiter().GetResult();
                var network = testEnv.Client.Network_.GetNetwork();
                var nodes = new List<AccountId>(network.Values);
                Assert.False(nodes.Count == 0);
                var node = nodes[0];
                new AccountBalanceQuery
                {
                    AccountId = node
                }
                .Execute(testEnv.Client);
            }
        }

        public virtual void TestClientInitWithMirrorNetwork()
        {
            var mirrorNetworkString = "testnet.mirrornode.hedera.com:443";
            var client = Client.ForMirrorNetwork([mirrorNetworkString]);
            var mirrorNetwork = client.MirrorNetwork_;
            Assert.Single(mirrorNetwork.Network_Read);
            Assert.Equal(mirrorNetwork.Network[0], mirrorNetworkString);
            Assert.NotNull(client.Network_);
            Assert.NotEmpty(client.Network_.Network_Read);
            client.Dispose();
        }

        public virtual void TestClientInitWithMirrorNetworkAnCustomRealmAndShard()
        {
            var mirrorNetworkString = "testnet.mirrornode.hedera.com:443";
            var client = Client.ForMirrorNetwork([mirrorNetworkString], 0, 0);
            var mirrorNetwork = client.MirrorNetwork_;
            Assert.Single(mirrorNetwork.Network_Read);
            Assert.Equal(mirrorNetwork.Network[0], mirrorNetworkString);
            Assert.NotNull(client.Network_);
            Assert.NotEmpty(client.Network_.Network_Read);
        }
    }
}