// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Grpc.Core;

using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;

using System.Threading.Tasks;
using System.Threading;

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
			addressBookQuery = new AddressBookQuery { FileId = FileId.ADDRESS_BOOK };
		}

		public virtual void Teardown()
		{
			addressBookServiceStub.Verify();

            client?.Dispose();
            server?.ShutdownAsync();
        }

		public virtual void AddressBookQueryWorks(string executeVersion)
		{
			addressBookServiceStub.requests.Append(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf(), Limit = 3 });
			addressBookServiceStub.responses.Append(new NodeAddress { AccountId = AccountId.FromString("0.0.3") }.ToProtobuf());
			addressBookQuery.Limit = 3;
			var nodes = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).Get();
			Assert.Single(nodes.nodeAddresses);
			Assert.Equal(nodes.nodeAddresses[0].accountId, AccountId.FromString("0.0.3"));
		}

		public virtual Endpoint SpawnEndpoint()
		{
			return new Endpoint
			{
                Address = new byte[] { 0x00, 0x01, 0x02, 0x03 },
                DomainName = "unit.test.com",
                Port = PORT_NODE_PLAIN
            };
		}

		public virtual void NetworkUpdatePeriodWorks()
		{
			addressBookServiceStub.requests.Append(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.responses.Append(new NodeAddress { AccountId = AccountId.FromString("0.0.3"), Addresses = [SpawnEndpoint()] }.ToProtobuf());
			
			client.NetworkUpdatePeriod = TimeSpan.FromSeconds(1);
			
			Thread.Sleep(1400);
			Assert.Single(client.Network_.Network_Read);
			Assert.True(client.Network_.Network_Read.Keys.Contains(AccountId.FromString("0.0.3")));
		}

		public virtual void AddressBookQueryRetries(string executeVersion, Status code, string description)
		{
			addressBookServiceStub.requests.Append(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.requests.Append(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.responses.Append(code.ToStatus().WithDescription(description).AsRuntimeException());
			addressBookServiceStub.responses.Append(new NodeAddress { AccountId = AccountId.FromString("0.0.3") });

			var nodes = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).GetAwaiter().GetResult();

			Assert.Single(nodes.NodeAddresses);
			Assert.Equal(nodes.NodeAddresses[0].AccountId, AccountId.FromString("0.0.3"));
		}

		public virtual void AddressBookQueryFails(string executeVersion, Status code, string description)
		{
			addressBookServiceStub.requests.Append(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.responses.Append(code.ToStatus().WithDescription(description).AsRuntimeException());

            var result = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).GetAwaiter().GetResult();
        }

		public virtual void AddressBookQueryStopsAtMaxAttempts(string executeVersion, Status code, string description)
		{
			addressBookQuery.MaxAttempts = 2;
			addressBookServiceStub.requests.Append(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.requests.Append(new Proto.AddressBookQuery { FileId = FileId.ADDRESS_BOOK.ToProtobuf() });
			addressBookServiceStub.responses.Append(code.ToStatus().WithDescription(description).AsRuntimeException());
			addressBookServiceStub.responses.Append(code.ToStatus().WithDescription(description).AsRuntimeException());

            var result = executeVersion.Equals("sync") ? addressBookQuery.Execute(client) : addressBookQuery.ExecuteAsync(client).GetAwaiter().GetResult(); 
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