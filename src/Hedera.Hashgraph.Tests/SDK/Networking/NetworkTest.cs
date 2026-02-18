// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    class NetworkTest
    {
        private ExecutorService executor;

        public virtual void SetUp()
        {
            executor = new ThreadPoolExecutor(2, 2, 0, TimeUnit.MILLISECONDS, new LinkedBlockingQueue(), new CallerRunsPolicy());
        }
        public virtual void TearDown()
        {
            if (executor != null)
            {
                executor.Shutdown();
            }
        }

        public virtual void GetNumberOfNodesForRequestReturnsFullNetworkSizeWhenNotSet()
        {
            Network network = CreateNetwork(3);

            // When maxNodesPerRequest is not set, should return full network size
            int numberOfNodes = network.NumberOfNodesForRequest;
            Assert.Equal(numberOfNodes, 3);
        }

        public virtual void GetNumberOfNodesForRequestReturnsMaxWhenSetAndLessThanNetworkSize()
        {
            Network network = CreateNetwork(5);

            // Set maxNodesPerRequest to 2
            network.MaxNodesPerRequest = 2;

            // Should return 2
            int numberOfNodes = network.NumberOfNodesForRequest;
            Assert.Equal(numberOfNodes, 2);
        }

        public virtual void GetNumberOfNodesForRequestReturnsNetworkSizeWhenMaxIsGreater()
        {
            Network network = CreateNetwork(2);

            // Set maxNodesPerRequest to 10 (greater than network size)
            network.MaxNodesPerRequest = 10;

            // Should return 2 (the network size, not 10)
            int numberOfNodes = network.NumberOfNodesForRequest;
            Assert.Equal(numberOfNodes, 2);
        }

        public virtual void GetNumberOfNodesForRequestReturnsNetworkSizeWhenMaxEqualsNetworkSize()
        {
            Network network = CreateNetwork(4);

            // Set maxNodesPerRequest to 4 (equals network size)
            network.MaxNodesPerRequest = 4;

            // Should return 4
            int numberOfNodes = network.NumberOfNodesForRequest;
            Assert.Equal(numberOfNodes, 4);
        }

        public virtual void GetNumberOfNodesForRequestReturnsOneForSingleNodeNetwork()
        {
            Network network = CreateNetwork(1);

            // Should return 1
            int numberOfNodes = network.NumberOfNodesForRequest;
            Assert.Equal(numberOfNodes, 1);
        }

        public virtual void GetNumberOfNodesForRequestReturnsZeroForEmptyNetwork()
        {
            Network network = CreateNetwork(0);

            // Should return 0
            int numberOfNodes = network.NumberOfNodesForRequest;
            Assert.Equal(numberOfNodes, 0);
        }

        /// <summary>
        /// Helper method to generate a network of a specific size.
        /// </summary>
        private Network CreateNetwork(int nodeCount)
        {
            Dictionary<string, AccountId> networkMap = new ();
            for (int i = 0; i < nodeCount; i++)
            {
                // Generate dummy node addresses and IDs
                networkMap.Add(i + ".testnet.hedera.com:50211", new AccountId(0, 0, 3 + i));
            }

            return Network.ForNetwork(executor, networkMap);
        }
    }
}