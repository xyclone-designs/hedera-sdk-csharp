// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Networking;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using IOFile = System.IO.File;
using IOFileInfo = System.IO.FileInfo;

namespace Hedera.Hashgraph.SDK
{
	public sealed partial class Client
    {
		public const string MAINNET = "mainnet", TESTNET = "testnet", PREVIEWNET = "previewnet";

		public const int DEFAULT_MAX_ATTEMPTS = 10;

		public static readonly Hbar DEFAULT_MAX_QUERY_PAYMENT = new(1);
		public static readonly TimeSpan
			DEFAULT_MAX_BACKOFF = TimeSpan.FromSeconds(8),
			DEFAULT_MIN_BACKOFF = TimeSpan.FromMilliseconds(250),
			DEFAULT_MAX_NODE_BACKOFF = TimeSpan.FromHours(1),
			DEFAULT_MIN_NODE_BACKOFF = TimeSpan.FromSeconds(8),
			DEFAULT_CLOSE_TIMEOUT = TimeSpan.FromSeconds(30),
			DEFAULT_REQUEST_TIMEOUT = TimeSpan.FromMinutes(2),
			DEFAULT_GRPC_DEADLINE = TimeSpan.FromSeconds(10),
			DEFAULT_NETWORK_UPDATE_PERIOD = TimeSpan.FromHours(24),
			NETWORK_UPDATE_INITIAL_DELAY = TimeSpan.FromSeconds(10); // Initial delay of 10 seconds before we update the Network for the first time, so that this doesn't happen in unit tests.

		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForName(System.String)"]/*' />
		public static Client ForName(string name)
		{
			return name switch
			{
				MAINNET => Client.ForMainnet(),
				TESTNET => Client.ForTestnet(),
				PREVIEWNET => Client.ForPreviewnet(),

				_ => throw new ArgumentException("Name must be one-of `mainnet`, `testnet`, or `previewnet`")
			};
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForNetwork(System.Collections.Generic.Dictionary{System.String,AccountId},ExecutorService,System.Action{Client})"]/*' />
		public static Client ForNetwork(Dictionary<string, AccountId> NetworkMap, ExecutorService executor, Action<Client>? oncreate = null)
		{
			var network = Network.ForNetwork(executor, NetworkMap);
			var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
			Client client = new (executor, network, mirrorNetwork, null, false, null, 0, 0);

			oncreate?.Invoke(client);

			return client;
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForNetwork(System.Collections.Generic.Dictionary{System.String,AccountId},System.Action{Client})"]/*' />
		public static Client ForNetwork(Dictionary<string, AccountId> NetworkMap, Action<Client>? oncreate = null)
		{
			ExecutorService executor = new ();

			var isValidNetwork = true;
			long shard = 0;
			long realm = 0;
			foreach (AccountId accountId in NetworkMap.Values)
			{
				if (shard == 0)
				{
					shard = accountId.Shard;
				}

				if (realm == 0)
				{
					realm = accountId.Realm;
				}

				if (shard != accountId.Shard || realm != accountId.Realm)
				{
					isValidNetwork = false;
					break;
				}
			}

			if (!isValidNetwork)
			{
				throw new ArgumentException("Network is not valid, all nodes must be in the same shard and realm");
			}

			var network = Network.ForNetwork(executor, NetworkMap);
			var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
			Client client = new (executor, network, mirrorNetwork, null, true, null, shard, realm);

			oncreate?.Invoke(client);

			return client;
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForMirrorNetwork(System.Collections.Generic.IList{System.String})"]/*' />
		public static Client ForMirrorNetwork(IList<string> mirrorNetworkList)
		{
			return ForMirrorNetwork(mirrorNetworkList, 0, 0);
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForMirrorNetwork(System.Collections.Generic.IList{System.String},System.Int64,System.Int64)"]/*' />
		public static Client ForMirrorNetwork(IList<string> mirrorNetworkList, long shard, long realm)
		{
			var executor = new ExecutorService();
			var network = Network.ForNetwork(executor, []);
			var mirrorNetwork = MirrorNetwork.ForNetwork(executor, mirrorNetworkList);
			var client = new Client(executor, network, mirrorNetwork, null, true, null, shard, realm);

			client.NetworkFromAddressBook = new AddressBookQuery
			{
				FileId = FileId.GetAddressBookFileIdFor(shard, realm)

			}.Execute(client);

			return client;
		}

		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForMainnet(System.Action{Client})"]/*' />
		public static Client ForMainnet(Action<Client>? oncreate = null)
		{
			var executor = new ExecutorService();
			var network = Network.ForMainnet(executor);
			var mirrorNetwork = MirrorNetwork.ForMainnet(executor);
			
			Client client = new (executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, true, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);

			oncreate?.Invoke(client);

			return client;
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForMainnet(ExecutorService,System.Action{Client})"]/*' />
		public static Client ForMainnet(ExecutorService executor, Action<Client>? oncreate = null)
		{
			var network = Network.ForMainnet(executor);
			var mirrorNetwork = MirrorNetwork.ForMainnet(executor);

			Client client = new (executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, false, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);

			oncreate?.Invoke(client);

			return client;
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForTestnet(System.Action{Client})"]/*' />
		public static Client ForTestnet(Action<Client>? oncreate = null)
		{
			var executor = new ExecutorService();
			var network = Network.ForTestnet(executor);
			var mirrorNetwork = MirrorNetwork.ForTestnet(executor);

			Client client = new (executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, true, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);

			oncreate?.Invoke(client);

			return client;
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForTestnet(ExecutorService,System.Action{Client})"]/*' />
		public static Client ForTestnet(ExecutorService executor, Action<Client>? oncreate = null)
		{
			var network = Network.ForTestnet(executor);
			var mirrorNetwork = MirrorNetwork.ForTestnet(executor);

			Client client = new (executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, false, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);

			oncreate?.Invoke(client);

			return client;
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForPreviewnet(System.Action{Client})"]/*' />
		public static Client ForPreviewnet(Action<Client>? oncreate = null)
		{
			var executor = new ExecutorService();
			var network = Network.ForPreviewnet(executor);
			var mirrorNetwork = MirrorNetwork.ForPreviewnet(executor);

			Client client = new (executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, true, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);

			oncreate?.Invoke(client);

			return client;
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:ForPreviewnet(ExecutorService,System.Action{Client})"]/*' />
		public static Client ForPreviewnet(ExecutorService executor, Action<Client>? oncreate = null)
		{
			var network = Network.ForPreviewnet(executor);
			var mirrorNetwork = MirrorNetwork.ForPreviewnet(executor);

			Client client = new (executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, false, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);

			oncreate?.Invoke(client);

			return client;
		}

		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:FromConfig(System.String)"]/*' />
		public static Client FromConfig(string json)
		{
			return Config.FromString(json).ToClient();
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:FromConfigFile(IOFileInfo)"]/*' />
		public static Client FromConfigFile(IOFileInfo file)
		{
			return FromConfig(IOFile.ReadAllText(file.FullName, Encoding.UTF8));
		}
		/// <include file="Client.Statics.cs.xml" path='docs/member[@name="M:FromConfigFile(System.String)"]/*' />
		public static Client FromConfigFile(string fileName)
		{
			return FromConfigFile(new IOFileInfo(fileName));
		}
	}
}