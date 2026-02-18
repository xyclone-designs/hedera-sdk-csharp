// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Java.Util.Concurrent;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class IntegrationTestEnv : IDisposable
    {
        static readonly string LOCAL_CONSENSUS_NODE_ENDPOINT = "127.0.0.1:50211";
        // Local mirror REST port is 8084; 5600 is gRPC and rejects HTTP/1.1 requests.
        public static readonly string LOCAL_MIRROR_NODE_GRPC_ENDPOINT = "127.0.0.1:5600";
        static readonly AccountId LOCAL_CONSENSUS_NODE_ACCOUNT_ID = new AccountId(0, 0, 3);
        private readonly Client originalClient;
        public Client Client;
        public PublicKey OperatorKey;
        public AccountId OperatorId;
        public bool IsLocalNode = false;
        private static ExecutorService ClientExecutor = Executors.NewFixedThreadPool(Runtime.GetRuntime().AvailableProcessors());
        public IntegrationTestEnv() : this(0)
        {
        }

        public IntegrationTestEnv(int maxNodesPerTransaction)
        {
            Client = CreateTestEnvClient();
            if (maxNodesPerTransaction == 0)
            {
                maxNodesPerTransaction = Client.Network.Count;
            }

            Client.SetMaxNodesPerTransaction(maxNodesPerTransaction);
            originalClient = Client;
            try
            {
                var operatorPrivateKey = PrivateKey.FromString(System.GetProperty("OPERATOR_KEY"));
                OperatorId = AccountId.FromString(System.GetProperty("OPERATOR_ID"));
                OperatorKey = operatorPrivateKey.GetPublicKey();
                Client.OperatorSet(OperatorId, operatorPrivateKey);
            }
            catch (Exception ignored)
            {
            }

            OperatorKey = Client.OperatorPublicKey;
            OperatorId = Client.OperatorAccountId;
            Assert.NotNull(Client.OperatorAccountId);
            Assert.NotNull(Client.OperatorPublicKey);
            if (Client.Network.Count > 0 && (Client.Network.ContainsKey(LOCAL_CONSENSUS_NODE_ENDPOINT)))
            {
                IsLocalNode = true;
            }

            var nodeGetter = new TestEnvNodeGetter(Client);
            var network = new Dictionary<string, AccountId>();
            var nodeCount = Math.Min(Client.Network.Count, maxNodesPerTransaction);
            for (int i = 0; i < nodeCount; i++)
            {
                nodeGetter.NextNode(network);
            }

            Client.SetNetwork(network);
        }

        private static Client CreateTestEnvClient()
        {
            if (System.GetProperty("HEDERA_NETWORK").Equals("previewnet"))
            {
                return Client.ForPreviewnet();
            }
            else if (System.GetProperty("HEDERA_NETWORK").Equals("testnet"))
            {
                return Client.ForTestnet();
            }
            else if (System.GetProperty("HEDERA_NETWORK").Equals("localhost"))
            {
                var network = new Dictionary<string, AccountId>();
                network.Put(LOCAL_CONSENSUS_NODE_ENDPOINT, LOCAL_CONSENSUS_NODE_ACCOUNT_ID);
                return SDK.Client.ForNetwork(network, ClientExecutor).SetMirrorNetwork(List.Of(LOCAL_MIRROR_NODE_GRPC_ENDPOINT));
            }
            else if (!System.GetProperty("CONFIG_FILE").Equals(""))
            {
                try
                {
                    return SDK.Client.FromConfigFile(System.GetProperty("CONFIG_FILE"));
                }
                catch (Exception configFileException)
                {
                    configFileException.PrintStackTrace();
                }
            }

            throw new InvalidOperationException("Failed to construct client for IntegrationTestEnv");
        }

        public virtual IntegrationTestEnv UseThrowawayAccount(Hbar initialBalance)
        {
            var key = PrivateKey.GenerateED25519();
            OperatorKey = key.GetPublicKey();
            OperatorId = new AccountCreateTransaction
            {
				InitialBalance = initialBalance,
			}
            .SetKeyWithoutAlias(key)
            .Execute(Client)
            .GetReceipt(Client).AccountId;
            Client = SDK.Client.ForNetwork(originalClient.Network);
            Client.MirrorNetwork = originalClient.MirrorNetwork;
            Client.LedgerId = originalClient.LedgerId;
            Client.MaxAttempts = 15;
            Client.OperatorSet(OperatorId, key);
			return this;
        }

        public virtual IntegrationTestEnv UseThrowawayAccount()
        {
            return UseThrowawayAccount(new Hbar(50));
        }

        // Note: this is a temporary workaround.
        // The assumption should be removed once the local node is supporting multiple nodes.
        public virtual void AssumeNotLocalNode()
        {

            // first clean up the current IntegrationTestEnv...
            if (IsLocalNode)
            {
                Dispose();
            }


            // then skip the current test
            Assumptions.AssumeFalse(IsLocalNode);
        }

        public virtual void Dispose()
        {
            if (!OperatorId.Equals(originalClient.GetOperatorAccountId()))
            {
                try
                {
                    var hbarsBalance = new AccountBalanceQuery().SetAccountId(OperatorId).Execute(originalClient).hbars;
                    new TransferTransaction().AddHbarTransfer(OperatorId, hbarsBalance.Negated()).AddHbarTransfer(originalClient.GetOperatorAccountId()), hbarsBalance).FreezeWith(originalClient).SignWithOperator(Client).Execute(originalClient).GetReceipt(originalClient);
                }
                catch (Exception e)
                {
                    Client.Dispose();
                }
            }

            originalClient.Dispose();
        }

        private class TestEnvNodeGetter
        {
            private readonly Client client;
            private readonly List<Map.Entry<String, AccountId>> nodes;
            private int index = 0;
            public TestEnvNodeGetter(Client client)
            {
                this.client = client;
                nodes = new List(client.Network.EntrySet());
                Collections.Shuffle(nodes);
            }

            public virtual void NextNode(Dictionary<string, AccountId> outMap)
            {
                if (nodes.IsEmpty())
                {
                    throw new InvalidOperationException("IntegrationTestEnv needs another node, but there aren't enough nodes in client network");
                }

                for (; index < nodes.Count; index++)
                {
                    var node = nodes[index];
                    try
                    {
                        new TransferTransaction().SetNodeAccountIds(Collections.SingletonList(node.GetValue())).SetMaxAttempts(1).AddHbarTransfer(client.GetOperatorAccountId(), Hbar.FromTinybars(1).Negated()).AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1)).Execute(client).GetReceipt(client);
                        nodes.Remove(index);
                        outMap.Put(node.GetKey(), node.GetValue());
                        return;
                    }
                    catch (Throwable err)
                    {
                        System.err.Println(err);
                    }
                }

                throw new Exception("Failed to find working node in " + nodes + " for IntegrationTestEnv");
            }
        }
    }
}