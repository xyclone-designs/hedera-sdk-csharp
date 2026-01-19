using Google.Protobuf.WellKnownTypes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Managed client for use on the Hedera Hashgraph network.
     */
    public sealed partial class Client 
    {
		/**
         * Extract the executor.
         *
         * @return the executor service
         */
		static ExecutorService createExecutor()
		{
			var threadFactory = new ThreadFactoryBuilder()
					.setNameFormat("hedera-sdk-%d")
					.setDaemon(true)
					.build();

			int nThreads = Runtime.getRuntime().availableProcessors();
			return new ThreadPoolExecutor(
					nThreads,
					nThreads,
					0L,
					TimeUnit.MILLISECONDS,
					new LinkedBlockingQueue<>(),
					threadFactory,
					new ThreadPoolExecutor.CallerRunsPolicy());
		}

		/**
         * Set up the client for the selected network.
         *
         * @param name the selected network
         * @return the configured client
         */
		public static Client ForName(string name)
		{
			return switch (name)
			{
				case MAINNET->Client.forMainnet();
				case TESTNET->Client.forTestnet();
				case PREVIEWNET->Client.forPreviewnet();
				default -> throw new ArgumentException("Name must be one-of `mainnet`, `testnet`, or `previewnet`");
			}
			;
		}
		/**
         *
         * Construct a client given a set of nodes.
         * It is the responsibility of the caller to ensure that all nodes in the map are part of the
         * same Hedera network. Failure to do so will result in undefined behavior.
         * The client will load balance all requests to Hedera using a simple round-robin scheme to
         * chose nodes to send transactions to. For one transaction, at most 1/3 of the nodes will be tried.
         *
         * @param networkMap the map of node IDs to node addresses that make up the network.
         * @param executor runs the grpc requests asynchronously.
         * @return {@link Client}
         */
		public static Client ForNetwork(Dictionary<string, AccountId> networkMap, ExecutorService executor) {
            var network = Network.forNetwork(executor, networkMap);
            var mirrorNetwork = MirrorNetwork.forNetwork(executor, new ArrayList<>());

            return new Client(executor, network, mirrorNetwork, null, false, null, 0, 0);
        }
        /**
         * Construct a client given a set of nodes.
         *
         * <p>It is the responsibility of the caller to ensure that all nodes in the map are part of the
         * same Hedera network. Failure to do so will result in undefined behavior.
         *
         * <p>The client will load balance all requests to Hedera using a simple round-robin scheme to
         * chose nodes to send transactions to. For one transaction, at most 1/3 of the nodes will be tried.
         *
         * @param networkMap the map of node IDs to node addresses that make up the network.
         * @return {@link Client}
         */
        public static Client ForNetwork(Dictionary<string, AccountId> networkMap) {
            var executor = createExecutor();
            var isValidNetwork = true;
            var Shard = 0L;
            var Realm = 0L;

            foreach (AccountId accountId in networkMap.values())
            {
                if (Shard == 0) Shard = accountId.Shard;
                if (Realm == 0) Realm = accountId.Realm;
				if (Shard != accountId.Shard || Realm != accountId.Realm) {
                    isValidNetwork = false;
                    break;
                }
            }

            if (!isValidNetwork) throw new ArgumentException("Network is not valid, all nodes must be in the same Shard and Realm");

			var network = Network.ForNetwork(executor, networkMap);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, new []);

            return new Client(executor, network, mirrorNetwork, null, true, null, Shard, Realm);
        }
        /**
         * Set up the client from selected mirror network.
         * Using default `0` values for Realm and Shard for retrieving addressBookFileId
         *
         * @param mirrorNetworkList
         * @return
         */
        public static Client ForMirrorNetwork(List<string> mirrorNetworkList)
        {
            return forMirrorNetwork(mirrorNetworkList, 0, 0);
        }
        /**
         * Set up the client from selected mirror network and given Realm and Shard
         *
         * @param mirrorNetworkList
         * @param Realm
         * @param Shard
         * @return
         */
        public static Client ForMirrorNetwork(List<string> mirrorNetworkList, long Shard, long Realm)
        {
            var executor = createExecutor();
            var network = Network.forNetwork(executor, new HashMap<>());
            var mirrorNetwork = MirrorNetwork.forNetwork(executor, mirrorNetworkList);
            var client = new Client(executor, network, mirrorNetwork, null, true, null, Shard, Realm);
            var addressBook = new AddressBookQuery()
                    .setFileId(FileId.getAddressBookFileIdFor(Shard, Realm))
                    .execute(client);
            client.setNetworkFromAddressBook(addressBook);
            return client;
        }
        /**
         * Construct a Hedera client pre-configured for <a
         * href="https://docs.hedera.com/guides/mainnet/address-book#mainnet-address-book">Mainnet access</a>.
         *
         * @param executor runs the grpc requests asynchronously.
         * @return {@link Client}
         */
        public static Client ForMainnet(ExecutorService executor) {
            var network = Network.forMainnet(executor);
            var mirrorNetwork = MirrorNetwork.forMainnet(executor);

            return new Client(
                    executor,
                    network,
                    mirrorNetwork,
                    NETWORK_UPDATE_INITIAL_DELAY,
                    false,
                    DEFAULT_NETWORK_UPDATE_PERIOD,
                    0,
                    0);
        }
        /**
         * Construct a Hedera client pre-configured for <a href="https://docs.hedera.com/guides/testnet/nodes">Testnet
         * access</a>.
         *
         * @param executor runs the grpc requests asynchronously.
         * @return {@link Client}
         */
        public static Client ForTestnet(ExecutorService executor) {
            var network = Network.forTestnet(executor);
            var mirrorNetwork = MirrorNetwork.forTestnet(executor);

            return new Client(
                    executor,
                    network,
                    mirrorNetwork,
                    NETWORK_UPDATE_INITIAL_DELAY,
                    false,
                    DEFAULT_NETWORK_UPDATE_PERIOD,
                    0,
                    0);
        }
        /**
         * Construct a Hedera client pre-configured for <a
         * href="https://docs.hedera.com/guides/testnet/testnet-nodes#previewnet-node-public-keys">Preview Testnet
         * nodes</a>.
         *
         * @param executor runs the grpc requests asynchronously.
         * @return {@link Client}
         */
        public static Client ForPreviewnet(ExecutorService executor) {
            var network = Network.forPreviewnet(executor);
            var mirrorNetwork = MirrorNetwork.forPreviewnet(executor);

            return new Client(
                    executor,
                    network,
                    mirrorNetwork,
                    NETWORK_UPDATE_INITIAL_DELAY,
                    false,
                    DEFAULT_NETWORK_UPDATE_PERIOD,
                    0,
                    0);
        }
        /**
         * Construct a Hedera client pre-configured for <a
         * href="https://docs.hedera.com/guides/mainnet/address-book#mainnet-address-book">Mainnet access</a>.
         *
         * @return {@link Client}
         */
        public static Client ForMainnet() {
            var executor = createExecutor();
            var network = Network.forMainnet(executor);
            var mirrorNetwork = MirrorNetwork.forMainnet(executor);

            return new Client(
                    executor,
                    network,
                    mirrorNetwork,
                    NETWORK_UPDATE_INITIAL_DELAY,
                    true,
                    DEFAULT_NETWORK_UPDATE_PERIOD,
                    0,
                    0);
        }
        /**
         * Construct a Hedera client pre-configured for <a href="https://docs.hedera.com/guides/testnet/nodes">Testnet
         * access</a>.
         *
         * @return {@link Client}
         */
        public static Client ForTestnet() {
            var executor = createExecutor();
            var network = Network.ForTestnet(executor);
            var mirrorNetwork = MirrorNetwork.ForTestnet(executor);

            return new Client(
                    executor,
                    network,
                    mirrorNetwork,
                    NETWORK_UPDATE_INITIAL_DELAY,
                    true,
                    DEFAULT_NETWORK_UPDATE_PERIOD,
                    0,
                    0);
        }
        /**
         * Construct a Hedera client pre-configured for <a
         * href="https://docs.hedera.com/guides/testnet/testnet-nodes#previewnet-node-public-keys">Preview Testnet
         * nodes</a>.
         *
         * @return {@link Client}
         */
        public static Client ForPreviewnet() {
            var executor = createExecutor();
            var network = Network.ForPreviewnet(executor);
            var mirrorNetwork = MirrorNetwork.ForPreviewnet(executor);

            return new Client(
                    executor,
                    network,
                    mirrorNetwork,
                    NETWORK_UPDATE_INITIAL_DELAY,
                    true,
                    DEFAULT_NETWORK_UPDATE_PERIOD,
                    0,
                    0);
        }
        /**
         * Configure a client based off the given JSON string.
         *
         * @param json The json string containing the client configuration
         * @return {@link Client}
         * @ if the config is incorrect
         */
        public static Client FromConfig(string json)  
        {
            return Config.FromString(json).toClient();
        }
        /**
         * Configure a client based off the given JSON reader.
         *
         * @param json The Reader containing the client configuration
         * @return {@link Client}
         * @ if the config is incorrect
         */
        public static Client FromConfig(Reader json)  
        {
            return Config.FromJson(json).toClient();
        }

        private static Dictionary<string, AccountId> getNetworkNodes(JsonObject networks) 
        {
            return networks.ToDictionary(
				entry => entry.Value?.ToString().Replace("\"", ""),
				entry => AccountId.FromString(entry.Key.Replace("\"", "")));


			for (Map.Entry<string, JsonElement> entry : networks.entrySet()) {
                nodes.put(
                        entry.getValue().toString().replace(),
                        AccountId.FromString(entry.getKey().replace("\"", "")));
            }
            return nodes;
        }

        /**
         * Configure a client based on a JSON file at the given path.
         *
         * @param fileName The string containing the file path
         * @return {@link Client}
         * @ if IO operations fail
         */
        public static Client FromConfigFile(string fileName)  
        {
            return FromConfigFile(new File(fileName));
        }

        /**
         * Configure a client based on a JSON file.
         *
         * @param file The file containing the client configuration
         * @return {@link Client}
         * @ if IO operations fail
         */
        public static Client FromConfigFile(File file)  
        {
            return FromConfig(Files.newBufferedReader(file.toPath(), StandardCharsets.UTF_8));
        }
    }
}