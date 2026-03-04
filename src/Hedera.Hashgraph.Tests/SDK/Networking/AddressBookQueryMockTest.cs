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
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK;
using Grpc.Core;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;
using System.Threading.Tasks;

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
			client = Client.ForNetwork([]);
			client.MirrorNetwork_.Network.SetNetwork();
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
			addressBookServiceStub.requests.Add(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf(), Limit = 3 });
			addressBookServiceStub.responses.Add(new NodeAddress { AccountId = AccountId.FromString("0.0.3") }.ToProtobuf());
			addressBookQuery.Limit = 3;
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
			addressBookServiceStub.requests.Add(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.responses.Add(new NodeAddress { AccountId = AccountId.FromString("0.0.3"), Addresses = [SpawnEndpoint()] }.ToProtobuf());
			client.SetNetworkUpdatePeriod(TimeSpan.FromSeconds(1));
			Thread.Sleep(1400);
			var clientNetwork = client.Network;
			Assert.Single(clientNetwork);
			Assert.Contains(clientNetwork.Values(), AccountId.FromString("0.0.3"));
		}

		public virtual void AddressBookQueryRetries(string executeVersion, Status code, string description)
		{
			addressBookServiceStub.requests.Add(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.requests.Add(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
			addressBookServiceStub.responses.Add(new NodeAddress().SetAccountId(AccountId.FromString("0.0.3")).ToProtobuf());
			var nodes = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
			Assert.Single(nodes.nodeAddresses);
			Assert.Equal(nodes.nodeAddresses[0].accountId, AccountId.FromString("0.0.3"));
		}

		public virtual void AddressBookQueryFails(string executeVersion, Status code, string description)
		{
			addressBookServiceStub.requests.Add(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
			AssertThatException(, () =>
			{
				var result = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
			});
		}

		public virtual void AddressBookQueryStopsAtMaxAttempts(string executeVersion, Status code, string description)
		{
			addressBookQuery.MaxAttempts = 2;
			addressBookServiceStub.requests.Add(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.requests.Add(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
			addressBookServiceStub.responses.Add(code.ToStatus().WithDescription(description).AsRuntimeException());
			AssertThatException(, () =>
			{
				var result = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
			});
		}

		private class AddressBookQueryStub : Proto.NetworkService.NetworkServiceBase
		{
			public readonly Queue<Proto.AddressBookQuery> requests = new ();
			public readonly Queue<object> responses = new ();

            public override Task<Proto.NodeAddress> getNodes(Proto.AddressBookQuery addressBookQuery, ServerCallContext context)
			{
				var request = requests.Dequeue();
				Assert.NotNull(request);
				Assert.Equal(addressBookQuery, request);
				while (responses.Count != 0)
				{
					var response = responses.Dequeue();
					Assert.NotNull(response);

					if (response is Exception ex)
					{
						throw new RpcException(new Status(StatusCode.Internal, ex.Message));
					}

					return Task.FromResult((Proto.NodeAddress)response);
				}

				return Task.FromResult<Proto.NodeAddress>(null);
			}

			public virtual void Verify()
			{
				Assert.Empty(requests);
				Assert.Empty(responses);
			}
		}
	}
}