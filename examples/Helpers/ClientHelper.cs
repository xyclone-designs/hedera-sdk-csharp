// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Examples
{
    public class ClientHelper
    {
        public static readonly string LOCAL_NETWORK_NAME = "localhost";
        private static readonly string LOCAL_CONSENSUS_NODE_ENDPOINT = "127.0.0.1:50211";
        // Local mirror REST port is 8084; 5600 is gRPC-only.
        private static readonly string LOCAL_MIRROR_NODE_GRPC_ENDPOINT = "127.0.0.1:5600";
        private static readonly AccountId LOCAL_CONSENSUS_NODE_ACCOUNT_ID = new (0, 0, 3);

        public static Client ForName(string network, Action<Client>? oninit = null)
        {
            Client client;

            if (network.Equals(LOCAL_NETWORK_NAME))
                client = ForLocalNetwork(oninit);
            else
            {
                client = Client.ForName(network);
                oninit?.Invoke(client);
            }

            return client;
        }
        public static Client ForLocalNetwork(Action<Client>? oninit = null)
        {
            var network = new Dictionary<string, AccountId>
            {
                { LOCAL_CONSENSUS_NODE_ENDPOINT, LOCAL_CONSENSUS_NODE_ACCOUNT_ID }
            };

            return Client.ForNetwork(network, client =>
            {
                client.MirrorNetwork_.Network = [LOCAL_MIRROR_NODE_GRPC_ENDPOINT];

                oninit?.Invoke(client);
            });
        }
    }
}