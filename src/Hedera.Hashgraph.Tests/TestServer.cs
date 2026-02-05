// SPDX-License-Identifier: Apache-2.0
using Io.Grpc;
using Io.Grpc.Inprocess;
using Java.Io;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
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

            var network = new HashMap<string, AccountId>();
            network.Put("in-process:" + name + "[0]", AccountId.FromString("1.1.1"));
            network.Put("in-process:" + name + "[1]", AccountId.FromString("1.1.2"));
            client = Client.ForNetwork(network).SetNodeMinBackoff(Duration.OfMillis(500)).SetNodeMaxBackoff(Duration.OfMillis(500)).SetOperator(AccountId.FromString("2.2.2"), PrivateKey.Generate());
        }

        public virtual void Dispose()
        {
            client.Dispose();
            foreach (var server in grpcServers)
            {
                server.Shutdown();
                server.AwaitTermination();
            }
        }
    }
}