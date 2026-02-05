// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.BaseNodeAddress;
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk.Proto.Mirror;
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

namespace Com.Hedera.Hashgraph.Sdk
{
    class AddressBookQueryMockTest
    {
        private Client client;
        private readonly AddressBookQueryStub addressBookServiceStub = new AddressBookQueryStub();
        private Server server;
        private AddressBookQuery addressBookQuery;
        virtual void Setup()
        {
            client = Client.ForNetwork(Collections.EmptyMap());
            client.SetMirrorNetwork(List.Of("in-process:test"));
            server = InProcessServerBuilder.ForName("test").AddService(addressBookServiceStub).DirectExecutor().Build().Start();
            addressBookQuery = new AddressBookQuery();
            addressBookQuery.SetFileId(FileId.ADDRESS_BOOK);
        }

        virtual void Teardown()
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

        virtual void AddressBookQueryWorks(string executeVersion)
        {
            addressBookServiceStub.requests.Add(com.hedera.hashgraph.sdk.proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).SetLimit(3).Build());
            addressBookServiceStub.responses.Add(new NodeAddress().SetAccountId(AccountId.FromString("0.0.3")).ToProtobuf());
            addressBookQuery.SetLimit(3);
            var nodes = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
            AssertThat(nodes.nodeAddresses).HasSize(1);
            Assert.Equal(nodes.nodeAddresses[0].accountId, AccountId.FromString("0.0.3"));
        }

        virtual Endpoint SpawnEndpoint()
        {
            return new Endpoint().SetAddress(new byte[] { 0x00, 0x01, 0x02, 0x03 }).SetDomainName("unit.test.com").SetPort(PORT_NODE_PLAIN);
        }

        virtual void NetworkUpdatePeriodWorks()
        {
            addressBookServiceStub.requests.Add(com.hedera.hashgraph.sdk.proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.responses.Add(new NodeAddress().SetAccountId(AccountId.FromString("0.0.3")).SetAddresses(Collections.SingletonList(SpawnEndpoint())).ToProtobuf());
            client.SetNetworkUpdatePeriod(Duration.OfSeconds(1));
            Thread.Sleep(1400);
            var clientNetwork = client.GetNetwork();
            AssertThat(clientNetwork).HasSize(1);
            AssertThat(clientNetwork.Values()).Contains(AccountId.FromString("0.0.3"));
        }

        virtual void AddressBookQueryRetries(string executeVersion, Status.Code code, string description)
        {
            addressBookServiceStub.requests.Add(com.hedera.hashgraph.sdk.proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.requests.Add(com.hedera.hashgraph.sdk.proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            addressBookServiceStub.responses.Add(new NodeAddress().SetAccountId(AccountId.FromString("0.0.3")).ToProtobuf());
            var nodes = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
            AssertThat(nodes.nodeAddresses).HasSize(1);
            Assert.Equal(nodes.nodeAddresses[0].accountId, AccountId.FromString("0.0.3"));
        }

        virtual void AddressBookQueryFails(string executeVersion, Status.Code code, string description)
        {
            addressBookServiceStub.requests.Add(com.hedera.hashgraph.sdk.proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            AssertThatException().IsThrownBy(() =>
            {
                var result = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
            });
        }

        virtual void AddressBookQueryStopsAtMaxAttempts(string executeVersion, Status.Code code, string description)
        {
            addressBookQuery.SetMaxAttempts(2);
            addressBookServiceStub.requests.Add(com.hedera.hashgraph.sdk.proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.requests.Add(com.hedera.hashgraph.sdk.proto.mirror.AddressBookQuery.NewBuilder().SetFileId(FileId.ADDRESS_BOOK.ToProtobuf()).Build());
            addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
            AssertThatException().IsThrownBy(() =>
            {
                var result = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
            });
        }

        private class AddressBookQueryStub : NetworkServiceImplBase
        {
            private readonly Queue<com.hedera.hashgraph.sdk.proto.mirror.AddressBookQuery> requests = new ArrayDeque();
            private readonly Queue<object> responses = new ArrayDeque();
            public override void GetNodes(com.hedera.hashgraph.sdk.proto.mirror.AddressBookQuery addressBookQuery, StreamObserver<com.hedera.hashgraph.sdk.proto.NodeAddress> streamObserver)
            {
                var request = requests.Poll();
                AssertThat(request).IsNotNull();
                Assert.Equal(addressBookQuery, request);
                while (!responses.IsEmpty())
                {
                    var response = responses.Poll();
                    AssertThat(response).IsNotNull();
                    if (response is Throwable)
                    {
                        streamObserver.OnError((Throwable)response);
                        return;
                    }

                    streamObserver.OnNext((com.hedera.hashgraph.sdk.proto.NodeAddress)response);
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