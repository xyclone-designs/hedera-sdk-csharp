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

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class IntegrationTestEnv : IDisposable
    {
        static readonly string LOCAL_CONSENSUS_NODE_ENDPOINT = "127.0.0.1:50211";
        // Local mirror REST port is 8084; 5600 is gRPC and rejects HTTP/1.1 requests.
        public static readonly string LOCAL_MIRROR_NODE_GRPC_ENDPOINT = "127.0.0.1:5600";
        static readonly AccountId LOCAL_CONSENSUS_NODE_ACCOUNT_ID = new AccountId(0, 0, 3);
        private readonly Client originalClient;
        public Client client;
        public PublicKey operatorKey;
        public AccountId operatorId;
        public bool isLocalNode = false;
        private static ExecutorService clientExecutor = Executors.NewFixedThreadPool(Runtime.GetRuntime().AvailableProcessors());
        public IntegrationTestEnv() : this(0)
        {
        }

        public IntegrationTestEnv(int maxNodesPerTransaction)
        {
            client = CreateTestEnvClient();
            if (maxNodesPerTransaction == 0)
            {
                maxNodesPerTransaction = client.GetNetwork().Count;
            }

            client.SetMaxNodesPerTransaction(maxNodesPerTransaction);
            originalClient = client;
            try
            {
                var operatorPrivateKey = PrivateKey.FromString(System.GetProperty("OPERATOR_KEY"));
                operatorId = AccountId.FromString(System.GetProperty("OPERATOR_ID"));
                operatorKey = operatorPrivateKey.GetPublicKey();
                client.SetOperator(operatorId, operatorPrivateKey);
            }
            catch (Exception ignored)
            {
            }

            operatorKey = client.GetOperatorPublicKey();
            operatorId = client.GetOperatorAccountId();
            AssertThat(client.GetOperatorAccountId()).IsNotNull();
            AssertThat(client.GetOperatorPublicKey()).IsNotNull();
            if (client.GetNetwork().Count > 0 && (client.GetNetwork().ContainsKey(LOCAL_CONSENSUS_NODE_ENDPOINT)))
            {
                isLocalNode = true;
            }

            var nodeGetter = new TestEnvNodeGetter(client);
            var network = new HashMap<string, AccountId>();
            var nodeCount = Math.Min(client.GetNetwork().Count, maxNodesPerTransaction);
            for (int i = 0; i < nodeCount; i++)
            {
                nodeGetter.NextNode(network);
            }

            client.SetNetwork(network);
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
                var network = new HashMap<string, AccountId>();
                network.Put(LOCAL_CONSENSUS_NODE_ENDPOINT, LOCAL_CONSENSUS_NODE_ACCOUNT_ID);
                return Client.ForNetwork(network, clientExecutor).SetMirrorNetwork(List.Of(LOCAL_MIRROR_NODE_GRPC_ENDPOINT));
            }
            else if (!System.GetProperty("CONFIG_FILE").Equals(""))
            {
                try
                {
                    return Client.FromConfigFile(System.GetProperty("CONFIG_FILE"));
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
            operatorKey = key.GetPublicKey();
            operatorId = new AccountCreateTransaction().SetInitialBalance(initialBalance).SetKeyWithoutAlias(key).Execute(client).GetReceipt(client).accountId;
            client = Client.ForNetwork(originalClient.GetNetwork());
            client.SetMirrorNetwork(originalClient.GetMirrorNetwork());
            client.SetOperator(Objects.RequireNonNull(operatorId), key);
            client.SetLedgerId(originalClient.GetLedgerId());
            client.SetMaxAttempts(15);
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
            if (isLocalNode)
            {
                Dispose();
            }


            // then skip the current test
            Assumptions.AssumeFalse(isLocalNode);
        }

        public virtual void Dispose()
        {
            if (!operatorId.Equals(originalClient.GetOperatorAccountId()))
            {
                try
                {
                    var hbarsBalance = new AccountBalanceQuery().SetAccountId(operatorId).Execute(originalClient).hbars;
                    new TransferTransaction().AddHbarTransfer(operatorId, hbarsBalance.Negated()).AddHbarTransfer(Objects.RequireNonNull(originalClient.GetOperatorAccountId()), hbarsBalance).FreezeWith(originalClient).SignWithOperator(client).Execute(originalClient).GetReceipt(originalClient);
                }
                catch (Exception e)
                {
                    client.Dispose();
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
                nodes = new List(client.GetNetwork().EntrySet());
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