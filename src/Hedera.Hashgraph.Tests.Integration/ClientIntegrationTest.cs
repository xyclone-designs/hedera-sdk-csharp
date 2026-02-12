// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph;
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
    public class ClientIntegrationTest
    {
        virtual void FailsWhenNoNodesAreMatching()
        {
            var client = Client.ForTestnet().SetTransportSecurity(true);
            var nodes = new List<AccountId>();
            nodes.Add(new AccountId(0, 0, 1000));
            nodes.Add(new AccountId(0, 0, 1001));
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => new AccountBalanceQuery().SetNodeAccountIds(nodes).SetAccountId(new AccountId(0, 0, 7)).Execute(client)).WithMessageContaining("All node account IDs did not map to valid nodes in the client's network");
            client.Dispose();
        }

        virtual void CanSkipNodes()
        {
            var client = Client.ForTestnet().SetTransportSecurity(true);
            var nodes = new List(client.GetNetwork().Values().Stream().ToList());
            nodes.Add(new AccountId(0, 0, 1000));
            new AccountBalanceQuery().SetNodeAccountIds(nodes).SetAccountId(new AccountId(0, 0, 7)).Execute(client);
            client.Dispose();
        }

        virtual void TestReplaceNodes()
        {
            Dictionary<string, AccountId> network = new HashMap();
            network.Put("0.testnet.hedera.com:50211", new AccountId(0, 0, 3));
            network.Put("1.testnet.hedera.com:50211", new AccountId(0, 0, 4));
            using (var testEnv = new IntegrationTestEnv(1))
            {
                testEnv.client.SetMaxQueryPayment(new Hbar(2)).SetRequestTimeout(Duration.OfMinutes(2)).SetNetwork(network);
                AssertThat(testEnv.operatorId).IsNotNull();

                // Execute two simple queries so we create a channel for each network node.
                new AccountBalanceQuery().SetAccountId(new AccountId(0, 0, 3)).Execute(testEnv.client);
                new AccountBalanceQuery().SetAccountId(new AccountId(0, 0, 3)).Execute(testEnv.client);
                network = new HashMap();
                network.Put("1.testnet.hedera.com:50211", new AccountId(0, 0, 4));
                network.Put("2.testnet.hedera.com:50211", new AccountId(0, 0, 5));
                testEnv.client.SetNetwork(network);
                network = new HashMap();
                network.Put("35.186.191.247:50211", new AccountId(0, 0, 4));
                network.Put("35.192.2.25:50211", new AccountId(0, 0, 5));
                testEnv.client.SetNetwork(network);
            }
        }

        virtual void TransactionIdNetworkIsVerified()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() =>
            {
                var client = Client.ForPreviewnet();
                client.SetAutoValidateChecksums(true);
                new AccountCreateTransaction().SetTransactionId(TransactionId.Generate(AccountId.FromString("0.0.123-esxsf"))).Execute(client);
                client.Dispose();
            });
        }

        virtual void TestMaxNodesPerTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                testEnv.client.SetMaxNodesPerTransaction(1);
                var transaction = new AccountDeleteTransaction().SetAccountId(testEnv.operatorId).FreezeWith(testEnv.client);
                AssertThat(transaction.GetNodeAccountIds()).IsNotNull();
                Assert.Equal(transaction.GetNodeAccountIds().Count, 1);
            }
        }

        virtual void Ping()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var network = testEnv.client.GetNetwork();
                var nodes = new List(network.Values());
                AssertThat(nodes.IsEmpty()).IsFalse();
                var node = nodes[0];
                testEnv.client.SetMaxNodeAttempts(1);
                testEnv.client.Ping(node);
            }
        }

        virtual void PingAll()
        {
            using (var testEnv = new IntegrationTestEnv())
            {
                testEnv.client.SetMaxNodeAttempts(1);
                testEnv.client.PingAll();
                var network = testEnv.client.GetNetwork();
                var nodes = new List(network.Values());
                AssertThat(nodes.IsEmpty()).IsFalse();
                var node = nodes[0];
                new AccountBalanceQuery().SetAccountId(node).Execute(testEnv.client);
            }
        }

        virtual void PingAllBadNetwork()
        {
            using (var testEnv = new IntegrationTestEnv(3))
            {

                // Skip if using local node.
                // Note: this check should be removed once the local node is supporting multiple nodes.
                testEnv.AssumeNotLocalNode();
                testEnv.client.SetMaxNodeAttempts(1);
                testEnv.client.SetMaxAttempts(1);
                testEnv.client.SetMaxNodesPerTransaction(2);
                var network = testEnv.client.GetNetwork();
                var entries = new List(network.EntrySet());
                AssertThat(entries.Count).IsGreaterThan(1);
                network.Clear();
                network.Put("in-process:name", entries[0].GetValue());
                network.Put(entries[1].GetKey(), entries[1].GetValue());
                testEnv.client.SetNetwork(network);
                AssertThatExceptionOfType(typeof(MaxAttemptsExceededException)).IsThrownBy(() =>
                {
                    testEnv.client.PingAll();
                }).WithMessageContaining("exceeded maximum attempts");
                var nodes = new List(testEnv.client.GetNetwork().Values());
                AssertThat(nodes.IsEmpty()).IsFalse();
                var node = nodes[0];
                new AccountBalanceQuery().SetAccountId(node).Execute(testEnv.client);
                Assert.Equal(testEnv.client.GetNetwork().Values().Count, 1);
            }
        }

        virtual void PingAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var network = testEnv.client.GetNetwork();
                var nodes = new List(network.Values());
                AssertThat(nodes.IsEmpty()).IsFalse();
                var node = nodes[0];
                testEnv.client.SetMaxNodeAttempts(1);
                testEnv.client.PingAsync(node).Get();
            }
        }

        virtual void PingAllAsync()
        {
            using (var testEnv = new IntegrationTestEnv())
            {
                testEnv.client.SetMaxNodeAttempts(1);
                testEnv.client.PingAllAsync().Get();
                var network = testEnv.client.GetNetwork();
                var nodes = new List(network.Values());
                AssertThat(nodes.IsEmpty()).IsFalse();
                var node = nodes[0];
                new AccountBalanceQuery().SetAccountId(node).Execute(testEnv.client);
            }
        }

        virtual void TestClientInitWithMirrorNetwork()
        {
            var mirrorNetworkString = "testnet.mirrornode.hedera.com:443";
            var client = Client.ForMirrorNetwork(List.Of(mirrorNetworkString));
            var mirrorNetwork = client.GetMirrorNetwork();
            AssertThat(mirrorNetwork).HasSize(1);
            Assert.Equal(mirrorNetwork[0], mirrorNetworkString);
            AssertThat(client.GetNetwork()).IsNotNull();
            Assert.NotEmpty(client.GetNetwork());
            client.Dispose();
        }

        virtual void TestClientInitWithMirrorNetworkAnCustomRealmAndShard()
        {
            var mirrorNetworkString = "testnet.mirrornode.hedera.com:443";
            var client = Client.ForMirrorNetwork(List.Of(mirrorNetworkString), 0, 0);
            var mirrorNetwork = client.GetMirrorNetwork();
            AssertThat(mirrorNetwork).HasSize(1);
            Assert.Equal(mirrorNetwork[0], mirrorNetworkString);
            AssertThat(client.GetNetwork()).IsNotNull();
            Assert.NotEmpty(client.GetNetwork());
        }
    }
}