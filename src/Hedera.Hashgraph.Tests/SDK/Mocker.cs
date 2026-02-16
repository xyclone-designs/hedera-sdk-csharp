// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.Logger;
using Proto;
using Io.Grpc;
using Io.Grpc.Inprocess;
using Io.Grpc.Stub;
using Java.Io;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Concurrent.Atomic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class Mocker : IDisposable
    {
        private static readonly PrivateKey PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420d45e1557156908c967804615af59a000be88c7aa7058bfcbe0f46b16c28f887d");
        public readonly Client client;
        private readonly IList<ServiceDescriptor> services = List.Of(CryptoServiceGrpc.GetServiceDescriptor(), FileServiceGrpc.GetServiceDescriptor(), SmartContractServiceGrpc.GetServiceDescriptor(), ConsensusServiceGrpc.GetServiceDescriptor(), TokenServiceGrpc.GetServiceDescriptor());
        private readonly IList<IList<object>> responses;
        private readonly IList<Server> servers = new List();
        Mocker(IList<IList<object>> responses)
        {
            this.responses = responses;
            var network = new HashMap<string, AccountId>(responses.Count);
            for (var i = 0; i < responses.Count; i++)
            {
                var index = new AtomicInteger();
                var response = responses[i];
                var name = InProcessServerBuilder.GenerateName();
                var nodeAccountId = new AccountId(0, 0, 3 + i);
                var builder = InProcessServerBuilder.ForName(name);
                ConfigureServerBuilder(builder);
                network.Put("in-process:" + name, nodeAccountId);
                foreach (var service in services)
                {
                    var descriptor = ServerServiceDefinition(service);
                    foreach (MethodDescriptor<?, ?> method in service.GetMethods())
                    {
                        var methodDefinition = ServerMethodDefinition.Create((MethodDescriptor<object, object>)method, ServerCalls.AsyncUnaryCall((request, responseObserver) =>
                        {
                            var responseIndex = index.GetAndIncrement();
                            if (responseIndex >= response.Count)
                            {
                                responseObserver.OnError(Status.Code.ABORTED.ToStatus().AsRuntimeException());
                                return;
                            }

                            var r = response[responseIndex];
                            if (r is Function<?, ?>)
                            {
                                try
                                {
                                    r = ((Function<object, object>)r).Apply(request);
                                }
                                catch (Throwable e)
                                {
                                    r = Status.ABORTED.WithDescription(e.GetMessage()).AsRuntimeException();
                                }
                            }

                            if (r is Throwable)
                            {
                                responseObserver.OnError((Throwable)r);
                            }
                            else
                            {
                                responseObserver.OnNext(r);
                                responseObserver.OnCompleted();
                            }
                        }));
                        descriptor.AddMethod(methodDefinition);
                    }

                    builder.AddService(descriptor.Build());
                }

                try
                {
                    this.servers.Add(builder.DirectExecutor().Build().Start());
                }
                catch (IOException e)
                {
                    throw new Exception(e);
                }
            }

            this.client = Client.ForNetwork(network).SetOperator(new AccountId(0, 0, 1800), PRIVATE_KEY).SetMinBackoff(Duration.OfMillis(0)).SetMaxBackoff(Duration.OfMillis(0)).SetNodeMinBackoff(Duration.OfMillis(0)).SetNodeMaxBackoff(Duration.OfMillis(0)).SetMinNodeReadmitTime(Duration.OfMillis(0)).SetMaxNodeReadmitTime(Duration.OfMillis(0)).SetLogger(new Logger(LogLevel.SILENT));
        }

        protected virtual void ConfigureServerBuilder(InProcessServerBuilder builder)
        {
        }

        public static Mocker WithResponses(IList<IList<object>> responses)
        {
            return new Mocker(responses);
        }

        public virtual void Dispose()
        {
            client.Dispose();
            foreach (var server in servers)
            {
                server.Shutdown();
                server.AwaitTermination();
            }
        }
    }
}