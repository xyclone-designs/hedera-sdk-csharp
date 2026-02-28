// SPDX-License-Identifier: Apache-2.0
using Grpc.Core;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Tests.SDK
{
    // TODO: we may want to refactor to separate TestClient from TestServer.
    //       That way, we can have a client with a network of multiple test servers.
    //       Maybe we can test load-balancing?
    public class TestServer
    {
        public readonly Client client;
        private readonly Server[] grpcServers = new Server[2];
        public TestServer(string name, params BindableService[] services)
        {
            for (int i = 0; i < 2; i++)
            {
                var serverBuilder = InProcessServerBuilder.ForName(name + "[" + i + "]");
                foreach (var service in services)
                {
                    serverBuilder.AddService(service);
                }

                grpcServers[i] = serverBuilder.DirectExecutor().Build().Start();
            }

            var network = new Dictionary<string, AccountId>
            {
				{ "in-process:" + name + "[0]", AccountId.FromString("1.1.1") },
			{ "in-process:" + name + "[1]", AccountId.FromString("1.1.2") },
			};
            
            client = Client.ForNetwork(network, client =>
            {
                client.NodeMinBackoff = TimeSpan.FromMilliseconds(500);
                client.NodeMaxBackoff = TimeSpan.FromMilliseconds(500);
				client.OperatorSet(AccountId.FromString("2.2.2"), PrivateKey.Generate());
			});
        }

        public virtual async void Dispose()
        {
            client.Dispose();
            foreach (var server in grpcServers)
            {
                await server.ShutdownAsync();
            }
        }
    }
}