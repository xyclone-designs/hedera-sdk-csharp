// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.IO;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Networking;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    class ClientTest
    {
        public virtual void ForMainnet()
        {
            Client.ForMainnet().Dispose();
        }

        public virtual void ForMainnetWithExecutor()
        {
            Client.ForMainnet(new ExecutorService()).Dispose();
        }

        public virtual void ForTestnetWithExecutor()
        {
            Client.ForTestnet(new ExecutorService()).Dispose();
        }

        public virtual void ForPreviewnetWithWithExecutor()
        {
            Client.ForPreviewnet(new ExecutorService()).Dispose();
        }

        public virtual void SetMaxQueryPaymentNegative()
        {
            var client = Client.ForTestnet();
            Assert.Throws<ArgumentException>(() =>
            {
                client.DefaultMaxQueryPayment = Hbar.MIN;
            });
            client.Dispose();
        }

        public virtual void SetMaxAttempts(int maxAttempts)
        {
            var client = Client.ForNetwork([]);
            Assert.Throws<ArgumentException>(() =>
            {
                client.MaxAttempts = maxAttempts;
            });
            client.Dispose();
        }

        public virtual void SetMaxBackoffInvalid(long maxBackoffMillis)
        {
            var client = Client.ForNetwork([]);
            Assert.Throws<ArgumentException>(() =>
            {
                client.MaxBackoff = TimeSpan.FromMilliseconds(maxBackoffMillis);
            });
            client.Dispose();
        }

        public virtual void SetMaxBackoffValid(long maxBackoff)
        {
            Client.ForNetwork([]).MaxBackoff = TimeSpan.FromMilliseconds(maxBackoff);
        }
        public virtual void SetMinBackoffValid(long minBackoff)
        {
            Client.ForNetwork([]).MinBackoff = TimeSpan.FromMilliseconds(minBackoff);
        }

        public virtual void SetMaxTransactionFeeNegative()
        {
            var client = Client.ForTestnet();
            Assert.Throws<ArgumentException>(() =>
            {
                client.DefaultMaxTransactionFee = Hbar.MIN;
            });
            client.Dispose();
        }

        public virtual void FromJsonFile()
        {
            Client.FromConfigFile(new FileInfo("./src/test/resources/client-config.json")).Dispose();
            Client.FromConfigFile(new FileInfo("./src/test/resources/client-config-with-operator.json")).Dispose();
            Client.FromConfigFile("./src/test/resources/client-config.json").Dispose();
            Client.FromConfigFile("./src/test/resources/client-config-with-operator.json").Dispose();
        }

        public virtual void FromJsonFileWithShardAndRealm()
        {
            var client = Client.FromConfigFile(new FileInfo("./src/test/resources/client-config-with-shard-realm.json"));
            Assert.Equal(client.Shard, 2);
            Assert.Equal(client.Realm, 2);
            client.Dispose();
        }

        public virtual void TestFromJson()
        {

            // Copied content of `client-config-with-operator.json`
            var client = Client.FromConfig("{\n" + "    \"network\":\"mainnet\",\n" + "    \"operator\": {\n" + "        \"accountId\": \"0.0.36\",\n" + "        \"privateKey\": \"302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10\"\n" + "    }\n" + "}\n");

            // put it in a file for nicer formatting
            Stream clientConfig = typeof(ClientTest).Assembly.GetManifestResourceStream("client-config.json");
            Assert.NotNull(clientConfig);
            Client.FromConfig(new StreamReader(clientConfig).ReadToEnd()).Dispose();

            // put it in a file for nicer formatting
            Stream clientConfigWithOperator = typeof(ClientTest).Assembly.GetManifestResourceStream("client-config-with-operator.json");
            Assert.NotNull(clientConfigWithOperator);
            client.Dispose();
        }

        public virtual void TestFromJsonWithShardAndRealm()
        {

            // Copied content of `client-config-with-operator.json`
            var client = Client.FromConfig("{\n" + "    \"network\": {\n" + "        \"0.0.21\": \"0.testnet.hedera.com:50211\"\n" + "    },\n" + "    \"operator\": {\n" + "        \"accountId\": \"0.0.21\",\n" + "        \"privateKey\": \"302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10\"\n" + "    },\n" + "    \"shard\": \"2\",\n" + "    \"realm\": \"2\",\n" + "    \"mirrorNetwork\": \"mainnet\"\n" + "}\n");
            Assert.Equal(client.Shard, 2);
            Assert.Equal(client.Realm, 2);
        }

        public virtual void SetNetworkWorks()
        {
            var defaultNetwork = new Dictionary<string, AccountId>
            {
                { "0.testnet.hedera.com:50211", new AccountId(0, 0, 3) },
                { "1.testnet.hedera.com:50211", new AccountId(0, 0, 4) }
            };
            Client client = Client.ForNetwork(defaultNetwork);
            Assert.Equal(client.Network_.GetNetwork(), defaultNetwork);
            client.Network_.SetNetwork(defaultNetwork);
            Assert.Equal(client.Network_.GetNetwork(), defaultNetwork);

            var defaultNetworkWithExtraNode = new Dictionary<string, AccountId>
            {
                { "0.testnet.hedera.com:50211", new AccountId(0, 0, 3) },
                { "1.testnet.hedera.com:50211", new AccountId(0, 0, 4) },
                { "2.testnet.hedera.com:50211", new AccountId(0, 0, 5) }
            };
            client.Network_.SetNetwork(defaultNetworkWithExtraNode);
            Assert.Equal(client.Network_.GetNetwork(), defaultNetworkWithExtraNode);

            var singleNodeNetwork = new Dictionary<string, AccountId>
            {
                { "2.testnet.hedera.com:50211", new AccountId(0, 0, 5) }
            };
            client.Network_.SetNetwork(singleNodeNetwork);
            Assert.Equal(client.Network_.GetNetwork(), singleNodeNetwork);

            var singleNodeNetworkWithDifferentAccountId = new Dictionary<string, AccountId>
            {
                { "2.testnet.hedera.com:50211", new AccountId(0, 0, 6) }
            };
            client.Network_.SetNetwork(singleNodeNetworkWithDifferentAccountId);
            Assert.Equal(client.Network_.GetNetwork(), singleNodeNetworkWithDifferentAccountId);

            var multiAddressNetwork = new Dictionary<string, AccountId>
            {
                { "0.testnet.hedera.com:50211", new AccountId(0, 0, 3) },
                { "34.94.106.61:50211", new AccountId(0, 0, 3) },
                { "50.18.132.211:50211", new AccountId(0, 0, 3) },
                { "138.91.142.219:50211", new AccountId(0, 0, 3) },
                { "1.testnet.hedera.com:50211", new AccountId(0, 0, 4) },
                { "35.237.119.55:50211", new AccountId(0, 0, 4) },
                { "3.212.6.13:50211", new AccountId(0, 0, 4) },
                { "52.168.76.241:50211", new AccountId(0, 0, 4) }
            };
            client.Network_.SetNetwork(multiAddressNetwork);
            Assert.Equal(client.Network_.GetNetwork(), multiAddressNetwork);
            
            client.Dispose();
        }

        public virtual void SetMirrorNetworkWorks()
        {
            var defaultNetwork = new List<string> { "testnet.mirrornode.hedera.com:443" };
            Client client = Client.ForNetwork([], _client =>
            {
                _client.MirrorNetwork_.Network = defaultNetwork;
            });
            Assert.Equal(client.MirrorNetwork_.Network, defaultNetwork);
            client.MirrorNetwork_.Network = defaultNetwork;
            Assert.Equal(client.MirrorNetwork_.Network, defaultNetwork);
            var defaultNetworkWithExtraNode = new List<string> { "testnet.mirrornode.hedera.com:443", "testnet1.mirrornode.hedera.com:443" };
            client.MirrorNetwork_.Network = defaultNetworkWithExtraNode;
            Assert.Equal(client.MirrorNetwork_.Network, defaultNetworkWithExtraNode);
            var singleNodeNetwork = new List<string> { "testnet1.mirrornode.hedera.com:443" };
            client.MirrorNetwork_.Network = singleNodeNetwork;
            Assert.Equal(client.MirrorNetwork_.Network, singleNodeNetwork);
            var singleNodeNetworkWithDifferentNode = new List<string> { "testnet.mirrornode.hedera.com:443" };
            client.MirrorNetwork_.Network = singleNodeNetworkWithDifferentNode;
            Assert.Equal(client.MirrorNetwork_.Network, singleNodeNetworkWithDifferentNode);
            client.Dispose();
        }

        public virtual void SetMirrorNetworkFails()
        {
            var defaultNetwork =  new List<string> { "testnet.mirrornode.hedera.com:443", "testnet.mirrornode2.hedera.com:443" };
            Client client = Client.ForNetwork([], _client =>
            {
                _client.MirrorNetwork_.Network = defaultNetwork;
            });
            Assert.Equal(client.MirrorNetwork_.Network, defaultNetwork);
            client.CloseTimeout = TimeSpan.Zero;
            IList<string> updatedNetwork = ["testnet.mirrornode.hedera.com:443"];
            Exception exception = Assert.Throws<Exception>(() => client.MirrorNetwork_.Network = updatedNetwork);
            Assert.EndsWith(exception.Message, "Failed to properly shutdown all channels");
        }

        public virtual void ForNameReturnsCorrectNetwork()
        {
            Client mainnetClient = Client.ForName("mainnet");
            Assert.Equal(mainnetClient.Network_.LedgerId, LedgerId.MAINNET);
            Client testnetClient = Client.ForName("testnet");
            Assert.Equal(testnetClient.Network_.LedgerId, LedgerId.TESTNET);
            Client previewnetClient = Client.ForName("previewnet");
            Assert.Equal(previewnetClient.Network_.LedgerId, LedgerId.PREVIEWNET);

            Exception exception = Assert.Throws<Exception>(() => Client.ForName("unknown"));
            Assert.EndsWith(exception.Message, "Name must be one-of `mainnet`, `testnet`, or `previewnet`");
        }

        public virtual void TestExecuteAsyncTimeout(string timeoutSite)
        {
            AccountId accountId = AccountId.FromString("0.0.1");
            TimeSpan timeout = TimeSpan.FromSeconds(5);
            Client client = Client.ForNetwork(new Dictionary<string, AccountId> { { "1.1.1.1:50211", accountId } }, client =>
            {
                client.NodeMinBackoff = TimeSpan.FromMilliseconds(0);
                client.NodeMaxBackoff = TimeSpan.FromMilliseconds(0);
                client.MinNodeReadmitTime = TimeSpan.FromMilliseconds(0);
                client.MaxNodeReadmitTime = TimeSpan.FromMilliseconds(0);
            });

            AccountBalanceQuery query = new()
            {
                AccountId = accountId,
                MaxAttempts = 3
            };
            DateTimeOffset start = DateTimeOffset.UtcNow;
            try
            {
                if (timeoutSite.Equals("onClient"))
                {
                    client.RequestTimeout = timeout;
                    query.ExecuteAsync(client).GetAwaiter().GetResult();
                }
                else
                {
                    query.ExecuteAsync(client, timeout).GetAwaiter().GetResult();
                }
            }
            catch (Exception) { }

            long secondsTaken = (long)(DateTimeOffset.UtcNow - start).TotalSeconds;

            // 20 seconds would indicate we tried 2 times to connect
            Assert.True(secondsTaken < 7);
            client.Dispose();
        }

        public virtual void TestExecuteSyncTimeout(string timeoutSite)
        {
            AccountId accountId = AccountId.FromString("0.0.1");

            // Executing requests in sync mode will require at most 10 seconds to connect
            // to a gRPC node. If we're not able to connect to a gRPC node within 10 seconds
            // we fail that request attempt. This means setting at timeout on a request
            // which hits non-connecting gRPC nodes will fail within ~10s of the set timeout
            // e.g. setting a timeout of 15 seconds, the request could fail within the range
            // of [5 seconds, 25 seconds]. The 10 second timeout for connecting to gRPC nodes
            // is not configurable.
            TimeSpan timeout = TimeSpan.FromSeconds(5);
            Client client = Client.ForNetwork(new Dictionary<string, AccountId> { { "1.1.1.1:50211", accountId } }, client =>
            {
                client.NodeMinBackoff = TimeSpan.FromMilliseconds(0);
                client.NodeMaxBackoff = TimeSpan.FromMilliseconds(0);
                client.MinNodeReadmitTime = TimeSpan.FromMilliseconds(0);
                client.MaxNodeReadmitTime = TimeSpan.FromMilliseconds(0);
            });

            AccountBalanceQuery query = new()
            {
                AccountId = accountId,
                MaxAttempts = 3,
                GrpcDeadline = TimeSpan.FromSeconds(5)
            };
            DateTimeOffset start = DateTimeOffset.UtcNow;
            try
            {
                if (timeoutSite.Equals("onClient"))
                {
                    client.RequestTimeout = timeout;
                    query.Execute(client);
                }
                else
                {
                    query.Execute(client, timeout);
                }
            }
            catch (TimeoutException e) { }

            long secondsTaken = (long)(DateTimeOffset.UtcNow - start).TotalSeconds;

            // 20 seconds would indicate we tried 2 times to connect
            Assert.True(secondsTaken < 15);
            client.Dispose();
        }

        public virtual Proto.NodeAddress NodeAddress(long accountNum, string rsaPubKeyHex, byte[] certHash, byte[] ipv4)
        {
            Proto.NodeAddress builder = new Proto.NodeAddress
            {
                RSAPubKey = rsaPubKeyHex,
                NodeAccountId = new Proto.AccountID { AccountNum = accountNum },
            };

            builder.ServiceEndpoint.Add(new Proto.ServiceEndpoint
            {
                IpAddressV4 = ByteString.CopyFrom(ipv4),
                Port = PORT_NODE_PLAIN
            });
                
            if (certHash != null)
                builder.NodeCertHash = ByteString.CopyFrom(certHash);

            return builder;
        }

        public virtual void SetNetworkFromAddressBook()
        {
            using (Client client = Client.ForNetwork([]))
            {
                Func<int, NodeAddress> nodeAddress = (accountNum) => client.Network_.Network_Read[new AccountId(0, 0, accountNum)][0].AddressBookEntry;

                // reconfigure client network from addressbook (add new nodes)
                Proto.NodeAddressBook a = new();
                a.NodeAddress.Add(NodeAddress(10001, "10001", new byte[] { 1, 0, 1 }, new byte[] { 10, 0, 0, 1 }));
                a.NodeAddress.Add(NodeAddress(10002, "10002", new byte[] { 1, 0, 2 }, new byte[] { 10, 0, 0, 2 }));
                client.NetworkFromAddressBook = NodeAddressBook.FromBytes(a.ToByteString());

                // verify security parameters in client
                Assert.Equal(nodeAddress.Invoke(10001).CertHash, ByteString.CopyFrom(new byte[] { 1, 0, 1 }));
                Assert.Equal(nodeAddress.Invoke(10001).PublicKey, "10001");
                Assert.Equal(nodeAddress.Invoke(10002).CertHash, ByteString.CopyFrom(new byte[] { 1, 0, 2 }));
                Assert.Equal(nodeAddress.Invoke(10002).PublicKey, "10002");

                // reconfigure client network from addressbook without `certHash`
                Proto.NodeAddressBook b = new();
                b.NodeAddress.Add(NodeAddress(10001, "10001", null, new byte[] { 10, 0, 0, 1 }));
                b.NodeAddress.Add(NodeAddress(10002, "10002", null, new byte[] { 10, 0, 0, 2 }));
                client.NetworkFromAddressBook = NodeAddressBook.FromBytes(b.ToByteString());

                // verify security parameters in client (unchanged)
                Assert.Equal(nodeAddress.Invoke(10001).CertHash, ByteString.CopyFrom(new byte[] { 1, 0, 1 }));
                Assert.Equal(nodeAddress.Invoke(10001).PublicKey, "10001");
                Assert.Equal(nodeAddress.Invoke(10002).CertHash, ByteString.CopyFrom(new byte[] { 1, 0, 2 }));
                Assert.Equal(nodeAddress.Invoke(10002).PublicKey, "10002");

                // reconfigure client network from addressbook (update existing nodes)
                Proto.NodeAddressBook c = new();
                c.NodeAddress.Add(NodeAddress(10001, "810001", new byte[] { 8, 1, 0, 1 }, new byte[] { 10, 0, 0, 1 }));
                c.NodeAddress.Add(NodeAddress(10002, "810002", new byte[] { 8, 1, 0, 2 }, new byte[] { 10, 0, 0, 2 }));
                client.NetworkFromAddressBook = NodeAddressBook.FromBytes(c.ToByteString());

                // verify security parameters in client
                Assert.Equal(nodeAddress.Invoke(10001).CertHash, ByteString.CopyFrom(new byte[] { 8, 1, 0, 1 }));
                Assert.Equal(nodeAddress.Invoke(10001).PublicKey, "810001");
                Assert.Equal(nodeAddress.Invoke(10002).CertHash, ByteString.CopyFrom(new byte[] { 8, 1, 0, 2 }));
                Assert.Equal(nodeAddress.Invoke(10002).PublicKey, "810002");
            }
        }

        public virtual void AssignAddressBookOnNodeCreationWhenAddressBookPresentShouldHaveTLSParametersPresent()
        {
            var client = Client.ForTestnet();
            client.Network_.SetNetwork(new Dictionary<string, AccountId> { { "1.2.3.4:50211", AccountId.FromString("0.0.3") } });
            Assert.NotNull(client.Network_.Nodes[0].GetChannelCredentials());
            var addressBookEntry = client.Network_.Nodes[0].AddressBookEntry;
            Assert.NotNull(addressBookEntry);
            Assert.NotNull(addressBookEntry.CertHash);
            Assert.NotNull(addressBookEntry.Addresses);
            Assert.NotNull(addressBookEntry.AccountId);
            Assert.NotNull(addressBookEntry.Description);
            client.Dispose();
        }

        public virtual void ClientPersistsShardAndRealm()
        {
            var network = Network.ForNetwork(new ExecutorService(), []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(new ExecutorService(), []);
            var client = new Client(new ExecutorService(), network, mirrorNetwork, null, true, null, 2, 1);
            Assert.Equal(client.Shard, 2);
            Assert.Equal(client.Realm, 1);
            client.Dispose();
        }

        public virtual void ForNetworkValidatesSameShardAndRealm()
        {
            var network = new Dictionary<string, AccountId>
            {
                { "127.0.0.1:50211", new AccountId(1, 2, 3) },
                { "127.0.0.1:50212", new AccountId(1, 2, 4) },
                { "127.0.0.1:50213", new AccountId(1, 2, 5) }
            };
            var client = Client.ForNetwork(network);
            Assert.Equal(client.Shard, 1);
            Assert.Equal(client.Realm, 2);
            client.Dispose();
        }

        public virtual void ForNetworkThrowsExceptionForDifferentShards()
        {
            var network = new Dictionary<string, AccountId>
            {
                { "127.0.0.1:50211", new AccountId(2, 2, 3) },
                { "127.0.0.1:50212", new AccountId(1, 2, 4) },
                { "127.0.0.1:50213", new AccountId(1, 2, 5) }
            };
            ArgumentException argumentexception = Assert.Throws<ArgumentException>(() => Client.ForNetwork(network));
            Assert.Equal(argumentexception.Message, "Network is not valid, all nodes must be in the same shard and realm");
        }

        public virtual void ForNetworkThrowsExceptionForDifferentRealms()
        {
            var network = new Dictionary<string, AccountId>
            {
                { "127.0.0.1:50211", new AccountId(1, 1, 3) },
                { "127.0.0.1:50212", new AccountId(1, 2, 4) },
                { "127.0.0.1:50213", new AccountId(1, 2, 5) }
            };
            ArgumentException argumentexception = Assert.Throws<ArgumentException>(() => Client.ForNetwork(network));
            Assert.Equal(argumentexception.Message, "Network is not valid, all nodes must be in the same shard and realm");
        }

        public virtual void ForNetworkWithExecutorValidatesSameShardAndRealm()
        {
            var network = new Dictionary<string, AccountId>
            {
                { "127.0.0.1:50211", new AccountId(1, 2, 3) }, 
                { "127.0.0.1:50212", new AccountId(1, 2, 4) }, 
                { "127.0.0.1:50213", new AccountId(1, 2, 5) }
            };
            var client = Client.ForNetwork(network);
            Assert.Equal(client.Shard, 1);
            Assert.Equal(client.Realm, 2);
            client.Dispose();
        }

        public virtual void ForNetworkWithExecutorThrowsExceptionForDifferentShards()
        {
            var network = new Dictionary<string, AccountId> 
            { 
                { "127.0.0.1:50211", new AccountId(2, 2, 3) }, 
                { "127.0.0.1:50212", new AccountId(1, 2, 4) }, 
                { "127.0.0.1:50213", new AccountId(1, 2, 5) } 
            };
            
            ArgumentException argumentexception = Assert.Throws<ArgumentException>(() => Client.ForNetwork(network));
            Assert.Equal(argumentexception.Message, "Network is not valid, all nodes must be in the same shard and realm");
        }

        public virtual void ForNetworkWithExecutorThrowsExceptionForDifferentRealms()
        {
            var network = new Dictionary<string, AccountId> 
            { 
                { "127.0.0.1:50211", new AccountId(1, 1, 3) }, 
                { "127.0.0.1:50212", new AccountId(1, 2, 4) }, 
                { "127.0.0.1:50213", new AccountId(1, 2, 5) } 
            };
            
            Exception exception = Assert.Throws<Exception>(() => Client.ForNetwork(network));
            Assert.Equal(exception.Message, "Network is not valid, all nodes must be in the same shard and realm");
        }

        public virtual void ForNetworkHandlesEmptyNetworkMap()
        {
            var client = Client.ForNetwork([]);

            // When network is empty, should use default values
            Assert.Equal(client.Shard, 0);
            Assert.Equal(client.Realm, 0);
            client.Dispose();
        }

        public virtual void ForNetworkHandlesSingleNodeNetwork()
        {
            var network = new Dictionary<string, AccountId>
            {
                { "127.0.0.1:50211", new AccountId(3, 4, 5) }
            };
            var client = Client.ForNetwork(network);
            Assert.Equal(client.Shard, 3);
            Assert.Equal(client.Realm, 4);
            client.Dispose();
        }
    }
}