// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Transactions;

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
        private static ExecutorService ClientExecutor = new ();
        public IntegrationTestEnv() : this(0) { }

        public IntegrationTestEnv(int maxNodesPerTransaction)
        {
            Client = CreateTestEnvClient();
            if (maxNodesPerTransaction == 0)
            {
                maxNodesPerTransaction = Client.Network_.GetNetwork().Count;
            }

            Client.Network_.MaxNodesPerRequest = maxNodesPerTransaction;
            originalClient = Client;
            try
            {
                var operatorPrivateKey = PrivateKey.FromString(Environment.GetEnvironmentVariable("OPERATOR_KEY"));
                OperatorId = AccountId.FromString(Environment.GetEnvironmentVariable("OPERATOR_ID"));
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
            if (Client.Network_.GetNetwork().Count > 0 && (Client.Network_.GetNetwork().ContainsKey(LOCAL_CONSENSUS_NODE_ENDPOINT)))
            {
                IsLocalNode = true;
            }

            var nodeGetter = new TestEnvNodeGetter(Client);
            var network = new Dictionary<string, AccountId>();
            var nodeCount = Math.Min(Client.Network_.GetNetwork().Count, maxNodesPerTransaction);
            for (int i = 0; i < nodeCount; i++)
            {
                nodeGetter.NextNode(network);
            }

            Client.Network_.SetNetwork(network);
        }

        private static Client CreateTestEnvClient()
        {
            if (Environment.GetEnvironmentVariable("HEDERA_NETWORK") is not string env) { }
            else if (env.Equals("previewnet")) return Client.ForPreviewnet();
            else if (env.Equals("testnet")) return Client.ForTestnet();
            else if (env.Equals("localhost")) return Client.ForNetwork(
                executor: ClientExecutor,
                onCreate: client => client.MirrorNetwork_ = MirrorNetwork.ForNetwork(ClientExecutor, [LOCAL_MIRROR_NODE_GRPC_ENDPOINT]),
                networkMap: new Dictionary<string, AccountId>
                {
                    { LOCAL_CONSENSUS_NODE_ENDPOINT, LOCAL_CONSENSUS_NODE_ACCOUNT_ID }
                });
            else if (!env.Equals(""))
                try
                {
                    return SDK.Client.FromConfigFile(Environment.GetEnvironmentVariable("CONFIG_FILE"));
                }
                catch (Exception exception) { Console.WriteLine(exception.StackTrace); }
			throw new InvalidOperationException("Failed to construct client for IntegrationTestEnv");
        }

        public virtual IntegrationTestEnv UseThrowawayAccount(Hbar initialBalance)
        {
            var key = PrivateKey.GenerateED25519();
            OperatorKey = key.GetPublicKey();
            OperatorId = new AccountCreateTransaction
            {
				InitialBalance = initialBalance,
				Key = key,
			}
            .Execute(Client)
            .GetReceipt(Client).AccountId;

            Client = Client.ForNetwork(originalClient.Network_.GetNetwork(), client =>
            {
				Client.MirrorNetwork_ = originalClient.MirrorNetwork_;
				Client.Network_.LedgerId = originalClient.Network_.LedgerId;
				Client.MaxAttempts = 15;
				Client.OperatorSet(OperatorId, key);
			});
            
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
            //Assumptions.AssumeFalse(IsLocalNode);
        }

        public virtual void Dispose()
        {
            if (!OperatorId.Equals(originalClient.OperatorAccountId))
            {
                try
                {
                    var hbarsBalance = new AccountBalanceQuery
                    {
						AccountId = OperatorId

					}.Execute(originalClient).Hbars;

                    new TransferTransaction()
                        .AddHbarTransfer(OperatorId, hbarsBalance.Negated())
                        .AddHbarTransfer(originalClient.OperatorAccountId, hbarsBalance)
                        .FreezeWith(originalClient)
                        .SignWithOperator(Client)
                        .Execute(originalClient)
                        .GetReceipt(originalClient);
                }
                catch (Exception)
                {
                    Client.Dispose();
                }
            }

            originalClient.Dispose();
        }

        private class TestEnvNodeGetter
        {
            private readonly Client client;
            private readonly Dictionary<string, AccountId> nodes;
            private int index = 0;
            public TestEnvNodeGetter(Client client)
            {
                this.client = client;
                nodes = new Dictionary<string, AccountId>(client.Network_.GetNetwork().Shuffle());
            }

            public virtual void NextNode(Dictionary<string, AccountId> outMap)
            {
                if (nodes.Count == 0)
					throw new InvalidOperationException("IntegrationTestEnv needs another node, but there aren't enough nodes in client network");

                var enumerator = nodes.GetEnumerator();

				while (enumerator.MoveNext())
                {
					try
					{
						new TransferTransaction
						{
							MaxAttempts = 1,
						}
						.SetNodeAccountIds([enumerator.Current.Value])
						.AddHbarTransfer(client.OperatorAccountId, Hbar.FromTinybars(1).Negated())
						.AddHbarTransfer(AccountId.FromString("0.0.3"), Hbar.FromTinybars(1))
						.Execute(client)
						.GetReceipt(client);

						//nodes.Remove(index);

						outMap.Add(enumerator.Current.Key, enumerator.Current.Value);

						return;
					}
					catch (Exception err)
					{
						Console.Error.WriteLine(err);
					}
				}

                throw new Exception("Failed to find working node in " + nodes + " for IntegrationTestEnv");
            }
        }
    }
}