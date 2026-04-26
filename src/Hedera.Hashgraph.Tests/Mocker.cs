using Google.Protobuf.Reflection;

using Grpc.Core;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Hedera.Hashgraph.Forks.HieroTCK.src.tests
{
    public class Mocker : IDisposable
    {
        private static readonly PrivateKey PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420d45e1557156908c967804615af59a000be88c7aa7058bfcbe0f46b16c28f887d");
        public readonly Client client;
        private readonly List<ServiceDescriptor> services =
        [
            Proto.Services.CryptoService.Descriptor,
            Proto.Services.FileService.Descriptor,
            Proto.Services.SmartContractService.Descriptor,
            Proto.Services.ConsensusService.Descriptor,
            Proto.Services.TokenService.Descriptor
        ];
        private readonly List<List<object>> responses;
        private readonly List<Server> servers = [];

        internal Mocker(List<List<object>> responses)
        {
            this.responses = responses;

            var network = new Dictionary<string, AccountId>(responses.Count);

            //for (var i = 0; i < responses.Count; i++)
            //{
            //    var index = new AtomicInteger();
            //    var response = responses.get(i);
            //    var name = InProcessServerBuilder.generateName();
            //    var nodeAccountId = new AccountId(0, 0, 3 + i);
            //    var builder = InProcessServerBuilder.forName(name);

            //    configureServerBuilder(builder);

            //    network.put("in-process:" + name, nodeAccountId);

            //    foreach (var service in services)
            //    {
            //        var descriptor = new ServerServiceDefinition.Builder(service);

            //        foreach (MethodDescriptor method in service.Methods)
            //        {
            //            var methodDefinition = ServerMethodDefinition.create(
            //                    (MethodDescriptor<Object, Object>)method,
            //                    ServerCalls.asyncUnaryCall((request, responseObserver) => 
            //                    {
            //                        var responseIndex = index.getAndIncrement();

            //                        if (responseIndex >= response.Count)
            //                        {
            //                            responseObserver.onError(
            //                                    Status.Code.ABORTED.toStatus().asRuntimeException());
            //                            return;
            //                        }

            //                        var r = response.get(responseIndex);

            //                        if (r is Func<object, object>)
            //                            {
            //                                try
            //                                {
            //                                    r = ((Func<object, object>)r).Invoke(request);
            //                                }
            //                                catch (Exception e)
            //                                {
            //                                    r = Status.ABORTED
            //                                            .withDescription(e.getMessage())
            //                                            .asRuntimeException();
            //                                }
            //                            }

            //                        if (r is Exception) {
            //                            responseObserver.OnError((Throwable)r);
            //                        } else
            //                        {
            //                            responseObserver.onNext(r);
            //                            responseObserver.onCompleted();
            //                        }
            //                    }));

            //                descriptor.addMethod(methodDefinition);
            //        }

            //        builder.addService(descriptor.build());
            //    }

            //    try
            //    {
            //        servers.Add(builder.directExecutor().build().start());
            //    }
            //    catch (IOException e)
            //    {
            //        throw new RuntimeWrappedException(e);
            //    }
            //}

            client = Client.ForNetwork(network, _client =>
            {
                _client.OperatorSet(new AccountId(0, 0, 1800), PRIVATE_KEY);
                _client.MinBackoff = TimeSpan.FromMilliseconds(0);
                _client.MaxBackoff = TimeSpan.FromMilliseconds(0);
                _client.NodeMinBackoff = TimeSpan.FromMilliseconds(0);
                _client.NodeMaxBackoff = TimeSpan.FromMilliseconds(0);
                _client.MinNodeReadmitTime = TimeSpan.FromMilliseconds(0);
                _client.MaxNodeReadmitTime = TimeSpan.FromMilliseconds(0);
                //_client.Logger = new Logger(LogLevel.SILENT));
            });                
        }

        //protected void configureServerBuilder(InProcessServerBuilder builder)
        //{
        //    // Override this method to configure the server builder
        //}

        public static Mocker withResponses(List<List<Object>> responses)
        {
            return new Mocker(responses);
        }

        public async void Dispose() 
        {
             client.Dispose();

            foreach (var server in servers)
            {
                await server.ShutdownAsync();
            }
        }
    }
}