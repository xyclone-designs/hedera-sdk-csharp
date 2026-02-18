// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.BaseNodeAddress;
using Org.Assertj.Core.Api.Assertions;
using Proto.Mirror;
using Io.Grpc;
using Io.Grpc.Inprocess;
using Io.Grpc.Stub;
using Java.Time;
using Java.Util;
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
    class AddressBookQueryMockTest
    {
        private Client client;
        private readonly AddressBookQueryStub addressBookServiceStub = new AddressBookQueryStub();
        private Server server;
        private AddressBookQuery addressBookQuery;
        public virtual void Setup()
        {
            client = Client.ForNetwork(Collections.EmptyMap());
            client.SetMirrorNetwork(List.Of("in-process:test"));
            server = InProcessServerBuilder.ForName("test").AddService(addressBookServiceStub).DirectExecutor().Build().Start();
            addressBookQuery = new AddressBookQuery();
            addressBookQuery.SetFileId(FileId.ADDRESS_BOOK);
        }

        public virtual void Teardown()
        {
            addressBookServiceStub.Verify();
            if (client != null)
            {
                client.Dispose();
            }

            if (server != null)
            {
                server.Shutdown();
                server.AwaitTermination();
            }
        }

        public virtual void AddressBookQueryWorks(string executeVersion)
        {
            addressBookServiceStub.requests.Add(Proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).SetLimit(3).Build());
            addressBookServiceStub.responses.Add(new NodeAddress().SetAccountId(AccountId.FromString("0.0.3")).ToProtobuf());
            addressBookQuery.SetLimit(3);
            var nodes = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
            Assert.Single(nodes.nodeAddresses);
            Assert.Equal(nodes.nodeAddresses[0].accountId, AccountId.FromString("0.0.3"));
        }

        public virtual Endpoint SpawnEndpoint()
        {
            return new Endpoint().SetAddress(new byte[] { 0x00, 0x01, 0x02, 0x03 }).SetDomainName("unit.test.com").SetPort(PORT_NODE_PLAIN);
        }

        public virtual void NetworkUpdatePeriodWorks()
        {
            addressBookServiceStub.requests.Add(Proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.responses.Add(new NodeAddress().SetAccountId(AccountId.FromString("0.0.3")).SetAddresses(Collections.SingletonList(SpawnEndpoint())).ToProtobuf());
            client.SetNetworkUpdatePeriod(Duration.OfSeconds(1));
            Thread.Sleep(1400);
            var clientNetwork = client.Network;
            Assert.Single(clientNetwork);
            Assert.Contains(clientNetwork.Values(), AccountId.FromString("0.0.3"));
        }

        public virtual void AddressBookQueryRetries(string executeVersion, Status.Code code, string description)
        {
            addressBookServiceStub.requests.Add(Proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.requests.Add(Proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            addressBookServiceStub.responses.Add(new NodeAddress().SetAccountId(AccountId.FromString("0.0.3")).ToProtobuf());
            var nodes = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
            Assert.Single(nodes.nodeAddresses);
            Assert.Equal(nodes.nodeAddresses[0].accountId, AccountId.FromString("0.0.3"));
        }

        public virtual void AddressBookQueryFails(string executeVersion, Status.Code code, string description)
        {
            addressBookServiceStub.requests.Add(Proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            AssertThatException(, () =>
            {
                var result = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
            });
        }

        public virtual void AddressBookQueryStopsAtMaxAttempts(string executeVersion, Status.Code code, string description)
        {
            addressBookQuery.SetMaxAttempts(2);
            addressBookServiceStub.requests.Add(Proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.requests.Add(Proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            AssertThatException(, () =>
            {
                var result = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
            });
        }

        private class AddressBookQueryStub : NetworkServiceImplBase
        {
            private readonly Queue<Proto.mirror.AddressBookQuery> requests = new ArrayDeque();
            private readonly Queue<object> responses = new ArrayDeque();
            public override void GetNodes(Proto.mirror.AddressBookQuery addressBookQuery, StreamObserver<Proto.NodeAddress> streamObserver)
            {
                var request = requests.Poll();
                Assert.NotNull(request);
                Assert.Equal(addressBookQuery, request);
                while (!responses.IsEmpty())
                {
                    var response = responses.Poll();
                    Assert.NotNull(response);
                    if (response is Throwable)
                    {
                        streamObserver.OnError((Throwable)response);
                        return;
                    }

                    streamObserver.OnNext((Proto.NodeAddress)response);
                }

                streamObserver.OnCompleted();
            }

            public virtual void Verify()
            {
                Assert.Empty(requests);
                Assert.Empty(responses);
            }
        }
    }
}