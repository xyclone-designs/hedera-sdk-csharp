// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.BaseNodeAddress;
using Com.Hedera.Hashgraph.Sdk.Client;
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Java.Io;
using Java.Nio.Charset;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Javax.Annotation;
using Org.Junit.Jupiter.Api;
using Org.Junit.Jupiter.Params;
using Org.Junit.Jupiter.Params.Provider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
            var executor = new ThreadPoolExecutor(2, 2, 0, TimeUnit.MILLISECONDS, new LinkedBlockingQueue(), new CallerRunsPolicy());
            Client.ForMainnet(executor).Dispose();
        }

        public virtual void ForTestnetWithExecutor()
        {
            var executor = new ThreadPoolExecutor(2, 2, 0, TimeUnit.MILLISECONDS, new LinkedBlockingQueue(), new CallerRunsPolicy());
            Client.ForTestnet(executor).Dispose();
        }

        public virtual void ForPreviewnetWithWithExecutor()
        {
            var executor = new ThreadPoolExecutor(2, 2, 0, TimeUnit.MILLISECONDS, new LinkedBlockingQueue(), new CallerRunsPolicy());
            Client.ForPreviewnet(executor).Dispose();
        }

        public virtual void SetMaxQueryPaymentNegative()
        {
            var client = Client.ForTestnet();
            Assert.Throws<ArgumentException>(() =>
            {
                client.SetMaxQueryPayment(Hbar.MIN);
            });
            client.Dispose();
        }

        public virtual void SetMaxAttempts(int maxAttempts)
        {
            var client = Client.ForNetwork(Map.Of());
            Assert.Throws<ArgumentException>(() =>
            {
                client.SetMaxAttempts(maxAttempts);
            });
            client.Dispose();
        }

        public virtual void SetMaxBackoffInvalid(long maxBackoffMillis)
        {
            Duration maxBackoff = maxBackoffMillis != null ? Duration.OfMillis(maxBackoffMillis) : null;
            var client = Client.ForNetwork(Map.Of());
            Assert.Throws<ArgumentException>(() =>
            {
                client.SetMaxBackoff(maxBackoff);
            });
            client.Dispose();
        }

        public virtual void SetMaxBackoffValid(long maxBackoff)
        {
            Client.ForNetwork(Map.Of()).SetMaxBackoff(Duration.OfMillis(maxBackoff)).Dispose();
        }

        public virtual void SetMinBackoffInvalid(long minBackoffMillis)
        {
            Duration minBackoff = minBackoffMillis != null ? Duration.OfMillis(minBackoffMillis) : null;
            var client = Client.ForNetwork(Map.Of());
            Assert.Throws<ArgumentException>(() =>
            {
                client.SetMinBackoff(minBackoff);
            });
            client.Dispose();
        }

        public virtual void SetMinBackoffValid(long minBackoff)
        {
            Client.ForNetwork(Map.Of()).SetMinBackoff(Duration.OfMillis(minBackoff)).Dispose();
        }

        public virtual void SetMaxTransactionFeeNegative()
        {
            var client = Client.ForTestnet();
            Assert.Throws<ArgumentException>(() =>
            {
                client.SetDefaultMaxTransactionFee(Hbar.MIN);
            });
            client.Dispose();
        }

        public virtual void FromJsonFile()
        {
            Client.FromConfigFile(new File("./src/test/resources/client-config.json")).Dispose();
            Client.FromConfigFile(new File("./src/test/resources/client-config-with-operator.json")).Dispose();
            Client.FromConfigFile("./src/test/resources/client-config.json").Dispose();
            Client.FromConfigFile("./src/test/resources/client-config-with-operator.json").Dispose();
        }

        public virtual void FromJsonFileWithShardAndRealm()
        {
            var client = Client.FromConfigFile(new File("./src/test/resources/client-config-with-shard-realm.json"));
            Assert.Equal(client.GetShard(), 2);
            Assert.Equal(client.GetRealm(), 2);
            client.Dispose();
        }

        public virtual void TestFromJson()
        {

            // Copied content of `client-config-with-operator.json`
            var client = Client.FromConfig("{\n" + "    \"network\":\"mainnet\",\n" + "    \"operator\": {\n" + "        \"accountId\": \"0.0.36\",\n" + "        \"privateKey\": \"302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10\"\n" + "    }\n" + "}\n");

            // put it in a file for nicer formatting
            InputStream clientConfig = typeof(ClientTest).GetClassLoader().GetResourceAsStream("client-config.json");
            AssertThat(clientConfig).IsNotNull();
            Client.FromConfig(new InputStreamReader(clientConfig, StandardCharsets.UTF_8)).Dispose();

            // put it in a file for nicer formatting
            InputStream clientConfigWithOperator = typeof(ClientTest).GetClassLoader().GetResourceAsStream("client-config-with-operator.json");
            AssertThat(clientConfigWithOperator).IsNotNull();
            client.Dispose();
        }

        public virtual void TestFromJsonWithShardAndRealm()
        {

            // Copied content of `client-config-with-operator.json`
            var client = Client.FromConfig("{\n" + "    \"network\": {\n" + "        \"0.0.21\": \"0.testnet.hedera.com:50211\"\n" + "    },\n" + "    \"operator\": {\n" + "        \"accountId\": \"0.0.21\",\n" + "        \"privateKey\": \"302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10\"\n" + "    },\n" + "    \"shard\": \"2\",\n" + "    \"realm\": \"2\",\n" + "    \"mirrorNetwork\": \"mainnet\"\n" + "}\n");
            Assert.Equal(client.GetShard(), 2);
            Assert.Equal(client.GetRealm(), 2);
        }

        public virtual void SetNetworkWorks()
        {
            var defaultNetwork = Map.Of("0.testnet.hedera.com:50211", new AccountId(0, 0, 3), "1.testnet.hedera.com:50211", new AccountId(0, 0, 4));
            Client client = Client.ForNetwork(defaultNetwork);
            AssertThat(client.GetNetwork()).ContainsExactlyInAnyOrderEntriesOf(defaultNetwork);
            client.SetNetwork(defaultNetwork);
            AssertThat(client.GetNetwork()).ContainsExactlyInAnyOrderEntriesOf(defaultNetwork);
            var defaultNetworkWithExtraNode = Map.Of("0.testnet.hedera.com:50211", new AccountId(0, 0, 3), "1.testnet.hedera.com:50211", new AccountId(0, 0, 4), "2.testnet.hedera.com:50211", new AccountId(0, 0, 5));
            client.SetNetwork(defaultNetworkWithExtraNode);
            AssertThat(client.GetNetwork()).ContainsExactlyInAnyOrderEntriesOf(defaultNetworkWithExtraNode);
            var singleNodeNetwork = Map.Of("2.testnet.hedera.com:50211", new AccountId(0, 0, 5));
            client.SetNetwork(singleNodeNetwork);
            AssertThat(client.GetNetwork()).ContainsExactlyInAnyOrderEntriesOf(singleNodeNetwork);
            var singleNodeNetworkWithDifferentAccountId = Map.Of("2.testnet.hedera.com:50211", new AccountId(0, 0, 6));
            client.SetNetwork(singleNodeNetworkWithDifferentAccountId);
            AssertThat(client.GetNetwork()).ContainsExactlyInAnyOrderEntriesOf(singleNodeNetworkWithDifferentAccountId);
            var multiAddressNetwork = Map.Of("0.testnet.hedera.com:50211", new AccountId(0, 0, 3), "34.94.106.61:50211", new AccountId(0, 0, 3), "50.18.132.211:50211", new AccountId(0, 0, 3), "138.91.142.219:50211", new AccountId(0, 0, 3), "1.testnet.hedera.com:50211", new AccountId(0, 0, 4), "35.237.119.55:50211", new AccountId(0, 0, 4), "3.212.6.13:50211", new AccountId(0, 0, 4), "52.168.76.241:50211", new AccountId(0, 0, 4));
            client.SetNetwork(multiAddressNetwork);
            AssertThat(client.GetNetwork()).ContainsExactlyInAnyOrderEntriesOf(multiAddressNetwork);
            client.Dispose();
        }

        public virtual void SetMirrorNetworkWorks()
        {
            var defaultNetwork = List.Of("testnet.mirrornode.hedera.com:443");
            Client client = Client.ForNetwork(new HashMap()).SetMirrorNetwork(defaultNetwork);
            AssertThat(client.GetMirrorNetwork()).ContainsExactlyInAnyOrderElementsOf(defaultNetwork);
            client.SetMirrorNetwork(defaultNetwork);
            AssertThat(client.GetMirrorNetwork()).ContainsExactlyInAnyOrderElementsOf(defaultNetwork);
            var defaultNetworkWithExtraNode = List.Of("testnet.mirrornode.hedera.com:443", "testnet1.mirrornode.hedera.com:443");
            client.SetMirrorNetwork(defaultNetworkWithExtraNode);
            AssertThat(client.GetMirrorNetwork()).ContainsExactlyInAnyOrderElementsOf(defaultNetworkWithExtraNode);
            var singleNodeNetwork = List.Of("testnet1.mirrornode.hedera.com:443");
            client.SetMirrorNetwork(singleNodeNetwork);
            AssertThat(client.GetMirrorNetwork()).ContainsExactlyInAnyOrderElementsOf(singleNodeNetwork);
            var singleNodeNetworkWithDifferentNode = List.Of("testnet.mirrornode.hedera.com:443");
            client.SetMirrorNetwork(singleNodeNetworkWithDifferentNode);
            AssertThat(client.GetMirrorNetwork()).ContainsExactlyInAnyOrderElementsOf(singleNodeNetworkWithDifferentNode);
            client.Dispose();
        }

        public virtual void SetMirrorNetworkFails()
        {
            var defaultNetwork = List.Of("testnet.mirrornode.hedera.com:443", "testnet.mirrornode2.hedera.com:443");
            Client client = Client.ForNetwork(new HashMap()).SetMirrorNetwork(defaultNetwork);
            AssertThat(client.GetMirrorNetwork()).ContainsExactlyInAnyOrderElementsOf(defaultNetwork);
            client.SetCloseTimeout(Duration.ZERO);
            IList<string> updatedNetwork = List.Of("testnet.mirrornode.hedera.com:443");
            AssertThatThrownBy(() => client.SetMirrorNetwork(updatedNetwork)).HasMessageEndingWith("Failed to properly shutdown all channels");
        }

        public virtual void ForNameReturnsCorrectNetwork()
        {
            Client mainnetClient = Client.ForName("mainnet");
            Assert.Equal(mainnetClient.GetLedgerId(), LedgerId.MAINNET);
            Client testnetClient = Client.ForName("testnet");
            Assert.Equal(testnetClient.GetLedgerId(), LedgerId.TESTNET);
            Client previewnetClient = Client.ForName("previewnet");
            Assert.Equal(previewnetClient.GetLedgerId(), LedgerId.PREVIEWNET);
            AssertThatThrownBy(() => Client.ForName("unknown")).HasMessageEndingWith("Name must be one-of `mainnet`, `testnet`, or `previewnet`");
        }

        public virtual void TestExecuteAsyncTimeout(string timeoutSite)
        {
            AccountId accountId = AccountId.FromString("0.0.1");
            Duration timeout = Duration.OfSeconds(5);
            Client client = Client.ForNetwork(Map.Of("1.1.1.1:50211", accountId)).SetNodeMinBackoff(Duration.OfMillis(0)).SetNodeMaxBackoff(Duration.OfMillis(0)).SetMinNodeReadmitTime(Duration.OfMillis(0)).SetMaxNodeReadmitTime(Duration.OfMillis(0));
            AccountBalanceQuery query = new AccountBalanceQuery().SetAccountId(accountId).SetMaxAttempts(3);
            DateTimeOffset start = DateTimeOffset.UtcNow;
            try
            {
                if (timeoutSite.Equals("onClient"))
                {
                    client.SetRequestTimeout(timeout);
                    query.ExecuteAsync(client).Get();
                }
                else
                {
                    query.ExecuteAsync(client, timeout).Get();
                }
            }
            catch (ExecutionException e)
            {
            }

            long secondsTaken = java.time.Duration.Between(start, DateTimeOffset.UtcNow).ToSeconds();

            // 20 seconds would indicate we tried 2 times to connect
            AssertThat(secondsTaken).IsLessThan(7);
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
            Duration timeout = Duration.OfSeconds(5);
            Client client = Client.ForNetwork(Map.Of("1.1.1.1:50211", accountId)).SetNodeMinBackoff(Duration.OfMillis(0)).SetNodeMaxBackoff(Duration.OfMillis(0)).SetMinNodeReadmitTime(Duration.OfMillis(0)).SetMaxNodeReadmitTime(Duration.OfMillis(0));
            AccountBalanceQuery query = new AccountBalanceQuery().SetAccountId(accountId).SetMaxAttempts(3).SetGrpcDeadline(Duration.OfSeconds(5));
            DateTimeOffset start = DateTimeOffset.UtcNow;
            try
            {
                if (timeoutSite.Equals("onClient"))
                {
                    client.SetRequestTimeout(timeout);
                    query.Execute(client);
                }
                else
                {
                    query.Execute(client, timeout);
                }
            }
            catch (TimeoutException e)
            {
            }

            long secondsTaken = java.time.Duration.Between(start, DateTimeOffset.UtcNow).ToSeconds();

            // 20 seconds would indicate we tried 2 times to connect
            AssertThat(secondsTaken).IsLessThan(15);
            client.Dispose();
        }

        public virtual Proto.NodeAddress NodeAddress(long accountNum, string rsaPubKeyHex, byte[] certHash, byte[] ipv4)
        {
            Proto.NodeAddress builder = Proto.NodeAddress.NewBuilder().SetNodeAccountId(Proto.AccountID.NewBuilder().SetAccountNum(accountNum).Build()).AddServiceEndpoint(Proto.ServiceEndpoint.NewBuilder().SetIpAddressV4(ByteString.CopyFrom(ipv4)).SetPort(PORT_NODE_PLAIN).Build()).SetRSAPubKey(rsaPubKeyHex);
            if (certHash != null)
            {
                builder.SetNodeCertHash(ByteString.CopyFrom(certHash));
            }

            return builder;
        }

        public virtual void SetNetworkFromAddressBook()
        {
            using (Client client = Client.ForNetwork(Map.Of()))
            {
                Function<int, NodeAddress> nodeAddress = (accountNum) => client.network.network[new AccountId(0, 0, accountNum)][0].GetAddressBookEntry();

                // reconfigure client network from addressbook (add new nodes)
                client.SetNetworkFromAddressBook(NodeAddressBook.FromBytes(Proto.NodeAddressBook.NewBuilder().AddNodeAddress(NodeAddress(10001, "10001", new byte[] { 1, 0, 1 }, new byte[] { 10, 0, 0, 1 })).AddNodeAddress(NodeAddress(10002, "10002", new byte[] { 1, 0, 2 }, new byte[] { 10, 0, 0, 2 })).Build().ToByteString()));

                // verify security parameters in client
                Assert.Equal(nodeAddress.Apply(10001).certHash, ByteString.CopyFrom(new byte[] { 1, 0, 1 }));
                Assert.Equal(nodeAddress.Apply(10001).publicKey, "10001");
                Assert.Equal(nodeAddress.Apply(10002).certHash, ByteString.CopyFrom(new byte[] { 1, 0, 2 }));
                Assert.Equal(nodeAddress.Apply(10002).publicKey, "10002");

                // reconfigure client network from addressbook without `certHash`
                client.SetNetworkFromAddressBook(NodeAddressBook.FromBytes(Proto.NodeAddressBook.NewBuilder().AddNodeAddress(NodeAddress(10001, "10001", null, new byte[] { 10, 0, 0, 1 })).AddNodeAddress(NodeAddress(10002, "10002", null, new byte[] { 10, 0, 0, 2 })).Build().ToByteString()));

                // verify security parameters in client (unchanged)
                Assert.Equal(nodeAddress.Apply(10001).certHash, ByteString.CopyFrom(new byte[] { 1, 0, 1 }));
                Assert.Equal(nodeAddress.Apply(10001).publicKey, "10001");
                Assert.Equal(nodeAddress.Apply(10002).certHash, ByteString.CopyFrom(new byte[] { 1, 0, 2 }));
                Assert.Equal(nodeAddress.Apply(10002).publicKey, "10002");

                // reconfigure client network from addressbook (update existing nodes)
                client.SetNetworkFromAddressBook(NodeAddressBook.FromBytes(Proto.NodeAddressBook.NewBuilder().AddNodeAddress(NodeAddress(10001, "810001", new byte[] { 8, 1, 0, 1 }, new byte[] { 10, 0, 0, 1 })).AddNodeAddress(NodeAddress(10002, "810002", new byte[] { 8, 1, 0, 2 }, new byte[] { 10, 0, 0, 2 })).Build().ToByteString()));

                // verify security parameters in client
                Assert.Equal(nodeAddress.Apply(10001).certHash, ByteString.CopyFrom(new byte[] { 8, 1, 0, 1 }));
                Assert.Equal(nodeAddress.Apply(10001).publicKey, "810001");
                Assert.Equal(nodeAddress.Apply(10002).certHash, ByteString.CopyFrom(new byte[] { 8, 1, 0, 2 }));
                Assert.Equal(nodeAddress.Apply(10002).publicKey, "810002");
            }
        }

        public virtual void AssignAddressBookOnNodeCreationWhenAddressBookPresentShouldHaveTLSParametersPresent()
        {
            var client = Client.ForTestnet();
            client.SetNetwork(Map.Of("1.2.3.4:50211", AccountId.FromString("0.0.3")));
            AssertThat(client.network.nodes[0].GetChannelCredentials()).IsNotNull();
            var addressBookEntry = client.network.nodes[0].GetAddressBookEntry();
            AssertThat(addressBookEntry).IsNotNull();
            AssertThat(addressBookEntry.certHash).IsNotNull();
            AssertThat(addressBookEntry.addresses).IsNotNull();
            AssertThat(addressBookEntry.accountId).IsNotNull();
            AssertThat(addressBookEntry.description).IsNotNull();
            client.Dispose();
        }

        public virtual void ClientPersistsShardAndRealm()
        {
            var network = Network.ForNetwork(CreateExecutor(), new HashMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(CreateExecutor(), new List());
            var client = new Client(CreateExecutor(), network, mirrorNetwork, null, true, null, 2, 1);
            Assert.Equal(client.GetShard(), 2);
            Assert.Equal(client.GetRealm(), 1);
            client.Dispose();
        }

        public virtual void ForNetworkValidatesSameShardAndRealm()
        {
            var network = Map.Of("127.0.0.1:50211", new AccountId(1, 2, 3), "127.0.0.1:50212", new AccountId(1, 2, 4), "127.0.0.1:50213", new AccountId(1, 2, 5));
            var client = Client.ForNetwork(network);
            Assert.Equal(client.GetShard(), 1);
            Assert.Equal(client.GetRealm(), 2);
            client.Dispose();
        }

        public virtual void ForNetworkThrowsExceptionForDifferentShards()
        {
            var network = Map.Of("127.0.0.1:50211", new AccountId(2, 2, 3), "127.0.0.1:50212", new AccountId(1, 2, 4), "127.0.0.1:50213", new AccountId(1, 2, 5));
            AssertThatThrownBy(() => Client.ForNetwork(network)).IsInstanceOf(typeof(ArgumentException)).HasMessage("Network is not valid, all nodes must be in the same shard and realm");
        }

        public virtual void ForNetworkThrowsExceptionForDifferentRealms()
        {
            var network = Map.Of("127.0.0.1:50211", new AccountId(1, 1, 3), "127.0.0.1:50212", new AccountId(1, 2, 4), "127.0.0.1:50213", new AccountId(1, 2, 5));
            AssertThatThrownBy(() => Client.ForNetwork(network)).IsInstanceOf(typeof(ArgumentException)).HasMessage("Network is not valid, all nodes must be in the same shard and realm");
        }

        public virtual void ForNetworkWithExecutorValidatesSameShardAndRealm()
        {
            var network = Map.Of("127.0.0.1:50211", new AccountId(1, 2, 3), "127.0.0.1:50212", new AccountId(1, 2, 4), "127.0.0.1:50213", new AccountId(1, 2, 5));
            var client = Client.ForNetwork(network);
            Assert.Equal(client.GetShard(), 1);
            Assert.Equal(client.GetRealm(), 2);
            client.Dispose();
        }

        public virtual void ForNetworkWithExecutorThrowsExceptionForDifferentShards()
        {
            var network = Map.Of("127.0.0.1:50211", new AccountId(2, 2, 3), "127.0.0.1:50212", new AccountId(1, 2, 4), "127.0.0.1:50213", new AccountId(1, 2, 5));
            AssertThatThrownBy(() => Client.ForNetwork(network)).IsInstanceOf(typeof(ArgumentException)).HasMessage("Network is not valid, all nodes must be in the same shard and realm");
        }

        public virtual void ForNetworkWithExecutorThrowsExceptionForDifferentRealms()
        {
            var network = Map.Of("127.0.0.1:50211", new AccountId(1, 1, 3), "127.0.0.1:50212", new AccountId(1, 2, 4), "127.0.0.1:50213", new AccountId(1, 2, 5));
            AssertThatThrownBy(() => Client.ForNetwork(network)).HasMessageEndingWith("Network is not valid, all nodes must be in the same shard and realm");
        }

        public virtual void ForNetworkHandlesEmptyNetworkMap()
        {
            var network = Map.Of();
            var client = Client.ForNetwork(network);

            // When network is empty, should use default values
            Assert.Equal(client.GetShard(), 0);
            Assert.Equal(client.GetRealm(), 0);
            client.Dispose();
        }

        public virtual void ForNetworkHandlesSingleNodeNetwork()
        {
            var network = Map.Of("127.0.0.1:50211", new AccountId(3, 4, 5));
            var client = Client.ForNetwork(network);
            Assert.Equal(client.GetShard(), 3);
            Assert.Equal(client.GetRealm(), 4);
            client.Dispose();
        }
    }
}