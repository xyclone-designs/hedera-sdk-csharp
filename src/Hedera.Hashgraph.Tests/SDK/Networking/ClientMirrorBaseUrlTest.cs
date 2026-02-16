// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Java.Util;
using Java.Util.Concurrent;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.SDK.Networking
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
            var network = Network.ForNetwork(executor, new HashMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, List.Of("mirror.example.com:8080"));
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            client.SetLedgerId(LedgerId.TESTNET);
            string base = client.GetMirrorRestBaseUrl();
            Assert.Equal(@base, "https://mirror.example.com:8080/api/v1");
        }

        public virtual void HostPort_defaultHttpsPort_omitted()
        {
            var network = Network.ForNetwork(executor, new HashMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, List.Of("mirror.example.com:443"));
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            client.SetLedgerId(LedgerId.TESTNET);
            string base = client.GetMirrorRestBaseUrl();
            Assert.Equal(@base, "https://mirror.example.com/api/v1");
        }

        public virtual void LocalNetwork_regularQuery_swaps5600to5551()
        {
            var network = Network.ForNetwork(executor, new HashMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, List.Of("localhost:5600"));
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            string base = client.GetMirrorRestBaseUrl();
            Assert.Equal(@base, "http://localhost:5551/api/v1");
        }

        public virtual void LocalNetwork_contractCall_swaps5600to5551()
        {
            var network = Network.ForNetwork(executor, new HashMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, List.Of("127.0.0.1:5600"));
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            string base = client.GetMirrorRestBaseUrl();
            Assert.Equal(@base, "http://127.0.0.1:5551/api/v1");
        }

        public virtual void EmptyMirrorNetwork_throws_whenAccessingBase()
        {
            var network = Network.ForNetwork(executor, new HashMap());
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, List.Of());
            var client = new Client(executor, network, mirrorNetwork, null, true, null, 0, 0);
            Assert.Throws<Exception>(() => client.GetMirrorRestBaseUrl());
        }
    }
}