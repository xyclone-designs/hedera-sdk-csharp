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
		public static readonly Duration 
			DEFAULT_MAX_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromSeconds(8)),
			DEFAULT_MIN_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromMilliseconds(250)),
			DEFAULT_MAX_NODE_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
			DEFAULT_MIN_NODE_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromSeconds(8)),
			DEFAULT_CLOSE_TIMEOUT = Duration.FromTimeSpan(TimeSpan.FromSeconds(30)),
			DEFAULT_REQUEST_TIMEOUT = Duration.FromTimeSpan(TimeSpan.FromMinutes(2)),
			DEFAULT_GRPC_DEADLINE = Duration.FromTimeSpan(TimeSpan.FromSeconds(10)),
			DEFAULT_NETWORK_UPDATE_PERIOD = Duration.FromTimeSpan(TimeSpan.FromHours(24)),
			NETWORK_UPDATE_INITIAL_DELAY = Duration.FromTimeSpan(TimeSpan.FromSeconds(10)); // Initial delay of 10 seconds before we update the Network for the first time, so that this doesn't happen in unit tests.

		/// <summary>
		/// Set up the client for the selected Network.
		/// </summary>
		/// <param name="name">the selected Network</param>
		/// <returns>the configured client</returns>
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
		/// <summary>
		/// 
		/// Construct a client given a set of nodes.
		/// It is the responsibility of the caller to ensure that all nodes in the map are part of the
		/// same Hedera Network. Failure to do so will result in undefined behavior.
		/// The client will load balance all requests to Hedera using a simple round-robin scheme to
		/// chose nodes to send transactions to. For one transaction, at most 1/3 of the nodes will be tried.
		/// </summary>
		/// <param name="NetworkMap">the map of node IDs to node addresses that make up the Network.</param>
		/// <param name="executor">runs the grpc requests asynchronously.</param>
		/// <returns>{@link Client}</returns>
		public static Client ForNetwork(Dictionary<string, AccountId> NetworkMap, ExecutorService executor)
		{
			var network = Network.ForNetwork(executor, NetworkMap);
			var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
			return new Client(executor, network, mirrorNetwork, null, false, null, 0, 0);
		}
		/// <summary>
		/// Construct a client given a set of nodes.
		/// 
		/// <p>It is the responsibility of the caller to ensure that all nodes in the map are part of the
		/// same Hedera Network. Failure to do so will result in undefined behavior.
		/// 
		/// <p>The client will load balance all requests to Hedera using a simple round-robin scheme to
		/// chose nodes to send transactions to. For one transaction, at most 1/3 of the nodes will be tried.
		/// </summary>
		/// <param name="NetworkMap">the map of node IDs to node addresses that make up the Network.</param>
		/// <returns>{@link Client}</returns>
		public static Client ForNetwork(Dictionary<string, AccountId> NetworkMap)
		{
			ExecutorService executor = CreateExecutor();

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
			return new Client(executor, network, mirrorNetwork, null, true, null, shard, realm);
		}
		/// <summary>
		/// Set up the client from selected mirror Network.
		/// Using default `0` values for realm and shard for retrieving addressBookFileId
		/// </summary>
		/// <param name="mirrorNetworkList"></param>
		/// <returns></returns>
		public static Client ForMirrorNetwork(IList<string> mirrorNetworkList)
		{
			return ForMirrorNetwork(mirrorNetworkList, 0, 0);
		}
		/// <summary>
		/// Set up the client from selected mirror Network and given realm and shard
		/// </summary>
		/// <param name="mirrorNetworkList"></param>
		/// <param name="realm"></param>
		/// <param name="shard"></param>
		/// <returns></returns>
		public static Client ForMirrorNetwork(IList<string> mirrorNetworkList, long shard, long realm)
		{
			var executor = CreateExecutor();
			var network = Network.ForNetwork(executor, []);
			var mirrorNetwork = MirrorNetwork.ForNetwork(executor, mirrorNetworkList);
			var client = new Client(executor, network, mirrorNetwork, null, true, null, shard, realm);

			client.NetworkFromAddressBook = new AddressBookQuery
			{
				FileId = FileId.GetAddressBookFileIdFor(shard, realm)

			}.Execute(client);

			return client;
		}

		/// <summary>
		/// Construct a Hedera client pre-configured for <a
		/// href="https://docs.hedera.com/guides/mainnet/address-book#mainnet-address-book">Mainnet access</a>.
		/// </summary>
		/// <returns>{@link Client}</returns>
		public static Client ForMainnet()
		{
			var executor = CreateExecutor();
			var network = Network.ForMainnet(executor);
			var mirrorNetwork = MirrorNetwork.ForMainnet(executor);
			return new Client(executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, true, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);
		}
		/// <summary>
		/// Construct a Hedera client pre-configured for <a
		/// href="https://docs.hedera.com/guides/mainnet/address-book#mainnet-address-book">Mainnet access</a>.
		/// </summary>
		/// <param name="executor">runs the grpc requests asynchronously.</param>
		/// <returns>{@link Client}</returns>
		public static Client ForMainnet(ExecutorService executor)
		{
			var network = Network.ForMainnet(executor);
			var mirrorNetwork = MirrorNetwork.ForMainnet(executor);
			return new Client(executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, false, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);
		}
		/// <summary>
		/// Construct a Hedera client pre-configured for <a href="https://docs.hedera.com/guides/testnet/nodes">Testnet
		/// access</a>.
		/// </summary>
		/// <returns>{@link Client}</returns>
		public static Client ForTestnet()
		{
			var executor = CreateExecutor();
			var network = Network.ForTestnet(executor);
			var mirrorNetwork = MirrorNetwork.ForTestnet(executor);
			return new Client(executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, true, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);
		}
		/// <summary>
		/// Construct a Hedera client pre-configured for <a href="https://docs.hedera.com/guides/testnet/nodes">Testnet
		/// access</a>.
		/// </summary>
		/// <param name="executor">runs the grpc requests asynchronously.</param>
		/// <returns>{@link Client}</returns>
		public static Client ForTestnet(ExecutorService executor)
		{
			var network = Network.ForTestnet(executor);
			var mirrorNetwork = MirrorNetwork.ForTestnet(executor);
			return new Client(executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, false, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);
		}
		/// <summary>
		/// Construct a Hedera client pre-configured for <a
		/// href="https://docs.hedera.com/guides/testnet/testnet-nodes#previewnet-node-public-keys">Preview Testnet
		/// nodes</a>.
		/// </summary>
		/// <returns>{@link Client}</returns>
		public static Client ForPreviewnet()
		{
			var executor = CreateExecutor();
			var network = Network.ForPreviewnet(executor);
			var mirrorNetwork = MirrorNetwork.ForPreviewnet(executor);
			return new Client(executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, true, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);
		}
		/// <summary>
		/// Construct a Hedera client pre-configured for <a
		/// href="https://docs.hedera.com/guides/testnet/testnet-nodes#previewnet-node-public-keys">Preview Testnet
		/// nodes</a>.
		/// </summary>
		/// <param name="executor">runs the grpc requests asynchronously.</param>
		/// <returns>{@link Client}</returns>
		public static Client ForPreviewnet(ExecutorService executor)
		{
			var network = Network.ForPreviewnet(executor);
			var mirrorNetwork = MirrorNetwork.ForPreviewnet(executor);
			return new Client(executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, false, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);
		}

		/// <summary>
		/// Configure a client based off the given JSON string.
		/// </summary>
		/// <param name="json">The json string containing the client configuration</param>
		/// <returns>{@link Client}</returns>
		/// <exception cref="Exception">if the config is incorrect</exception>
		public static Client FromConfig(string json)
		{
			return Config.FromString(json).ToClient();
		}
		/// <summary>
		/// Configure a client based on a JSON file.
		/// </summary>
		/// <param name="file">The file containing the client configuration</param>
		/// <returns>{@link Client}</returns>
		/// <exception cref="IOException">if IO operations fail</exception>
		public static Client FromConfigFile(IOFileInfo file)
		{
			return FromConfig(IOFile.ReadAllText(file.FullName, Encoding.UTF8));
		}
		/// <summary>
		/// Configure a client based on a JSON file at the given path.
		/// </summary>
		/// <param name="fileName">The string containing the file path</param>
		/// <returns>{@link Client}</returns>
		/// <exception cref="IOException">if IO operations fail</exception>
		public static Client FromConfigFile(string fileName)
		{
			return FromConfigFile(new IOFileInfo(fileName));
		}
	}
}