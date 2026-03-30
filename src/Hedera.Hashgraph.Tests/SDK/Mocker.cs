// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Grpc.Core;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class Mocker : IDisposable
    {
        private static readonly PrivateKey PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420d45e1557156908c967804615af59a000be88c7aa7058bfcbe0f46b16c28f887d");
        public readonly Client client;
        private readonly List<ServiceDescriptor> services = 
        [
            Proto.CryptoService.Descriptor, 
            Proto.FileService.Descriptor, 
            Proto.SmartContractService.Descriptor, 
            Proto.ConsensusService.Descriptor, 
            Proto.TokenService.Descriptor 
        ];
        private readonly List<IList<object>> responses;
        private readonly List<Server> servers = [];
        
        Mocker(IList<IList<object>> responses)
        {
            this.responses = [.. responses];
            var network = new Dictionary<string, AccountId>(responses.Count);
            for (var i = 0; i < responses.Count; i++)
            {
                var index = 0;
                var response = responses[i];
                var name = InProcessServerBuilder.GenerateName();
                var nodeAccountId = new AccountId(0, 0, 3 + i);
                var builder = InProcessServerBuilder.ForName(name);
                ConfigureServerBuilder(builder);
                network.Add("in-process:" + name, nodeAccountId);
                foreach (var service in services)
                {
                    foreach (MethodDescriptor method in service.Methods)
                    {
                        var methodDefinition = ServerMethodDefinition.Create((MethodDescriptor)method, ServerCalls.AsyncUnaryCall((request, responseObserver) =>
                        {
                            var responseIndex = Interlocked.Increment(ref index);
                            if (responseIndex >= response.Count)
                            {
                                responseObserver.OnError(ResponseStatus.Code.ABORTED.ToStatus().AsRuntimeException());
                                return;
                            }

                            var r = response[responseIndex];
                            
                            if (r is Func)
                                try
                                {
                                    r = ((Function<object, object>)r).Apply(request);
                                }
                                catch (Exception e)
                                {
                                    r = Status.ABORTED.WithDescription(e.GetMessage()).AsRuntimeException();
                                }

                            if (r is Exception rexception)
                            {
                                responseObserver.OnError(rexception);
                            }
                            else
                            {
                                responseObserver.OnNext(r);
                                responseObserver.OnCompleted();
                            }
                        }));

                        service.Methods.Add(methodDefinition);
                    }

                    builder.AddService(service);
                }

                try
                {
                    servers.Add(builder.DirectExecutor().Build().Start());
                }
                catch (IOException e)
                {
                    throw new Exception(e.Message, e);
                }
            }

            client = Client.ForNetwork(network, client =>
            {
                client.MinBackoff = TimeSpan.FromMilliseconds(0);
                client.MaxBackoff = TimeSpan.FromMilliseconds(0);
                client.NodeMinBackoff = TimeSpan.FromMilliseconds(0);
                client.NodeMaxBackoff = TimeSpan.FromMilliseconds(0);
                client.MinNodeReadmitTime = TimeSpan.FromMilliseconds(0);
                client.MaxNodeReadmitTime = TimeSpan.FromMilliseconds(0);

                client.OperatorSet(new AccountId(0, 0, 1800), PRIVATE_KEY);
            });
        }

        protected virtual void ConfigureServerBuilder(InProcessServerBuilder builder) { }

        public static Mocker WithResponses(IList<IList<object>> responses)
        {
            return new Mocker(responses);
        }

        public virtual void Dispose()
        {
            client.Dispose();

            foreach (var server in servers)
            {
                server.ShutdownAsync();
            }
        }
    }
}