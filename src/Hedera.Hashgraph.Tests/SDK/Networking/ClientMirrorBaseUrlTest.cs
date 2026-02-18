// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    /// <summary>
    /// Replacement for MirrorNodeUrlBuilderTest validating base URL generation via Client/MirrorNode.
    /// </summary>
    public class ClientMirrorBaseUrlTest
    {
        private ExecutorService executor;
        public virtual void SetUp()
        {
            executor = Executors.NewSingleThreadExecutor();
        }

        public virtual void TearDown()
        {
            if (executor != null)
            {
                executor.Shutdown();
            }
        }

        public virtual void HostPort_customPort_preserved_https_whenLedgerSet()
        {
            var network = Network.ForNetwork(executor, []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, [ "mirror.example.com:8080" ]);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            client.LedgerId = LedgerId.TESTNET;
            string _base = client.MirrorRestBaseUrl;
            Assert.Equal(_base, "https://mirror.example.com:8080/api/v1");
        }

        public virtual void HostPort_defaultHttpsPort_omitted()
        {
            var network = Network.ForNetwork(executor, []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, [ "mirror.example.com:443" ]);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            client.LedgerId = LedgerId.TESTNET;
            string _base = client.MirrorRestBaseUrl;
            Assert.Equal(_base, "https://mirror.example.com/api/v1");
        }

        public virtual void LocalNetwork_regularQuery_swaps5600to5551()
        {
            var network = Network.ForNetwork(executor, []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, [ "localhost:5600" ]);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            string _base = client.MirrorRestBaseUrl;
            Assert.Equal(_base, "http://localhost:5551/api/v1");
        }

        public virtual void LocalNetwork_contractCall_swaps5600to5551()
        {
            var network = Network.ForNetwork(executor, []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, [ "127.0.0.1:5600" ]);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            string _base = client.MirrorRestBaseUrl;
            Assert.Equal(_base, "http://127.0.0.1:5551/api/v1");
        }

        public virtual void EmptyMirrorNetwork_throws_whenAccessingBase()
        {
            var network = Network.ForNetwork(executor, []);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            Assert.Throws<Exception>(() => client.MirrorRestBaseUrl);
        }
    }
}