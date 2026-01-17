using Google.Protobuf.WellKnownTypes;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Managed client for use on the Hedera Hashgraph network.
 */
    public sealed class Client : AutoCloseable 
    {
		internal static readonly int DEFAULT_MAX_ATTEMPTS = 10;
        internal static readonly Duration DEFAULT_MAX_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromSeconds(8L));
        internal static readonly Duration DEFAULT_MIN_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromMilliseconds(250L));
        internal static readonly Duration DEFAULT_MAX_NODE_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromHours(1L));
        internal static readonly Duration DEFAULT_MIN_NODE_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromSeconds(8L));
        internal static readonly Duration DEFAULT_CLOSE_TIMEOUT = Duration.FromTimeSpan(TimeSpan.FromSeconds(30L));
        internal static readonly Duration DEFAULT_REQUEST_TIMEOUT = Duration.FromTimeSpan(TimeSpan.FromMinutes(2L));
        internal static readonly Duration DEFAULT_GRPC_DEADLINE = Duration.FromTimeSpan(TimeSpan.FromSeconds(10L));
        internal static readonly Duration DEFAULT_NETWORK_UPDATE_PERIOD = Duration.FromTimeSpan(TimeSpan.FromHours(24));
        // Initial delay of 10 seconds before we update the network for the first time,
        // so that this doesn't happen in unit tests.
        static readonly Duration NETWORK_UPDATE_INITIAL_DELAY = Duration.FromTimeSpan(TimeSpan.FromSeconds(10));
        private static readonly Hbar DEFAULT_MAX_QUERY_PAYMENT = new Hbar(1);
        private static readonly string MAINNET = "mainnet";
        private static readonly string TESTNET = "testnet";
        private static readonly string PREVIEWNET = "previewnet";
        readonly ExecutorService executor;
        private readonly AtomicReference<Duration> grpcDeadline = new AtomicReference(DEFAULT_GRPC_DEADLINE);
        private readonly Set<SubscriptionHandle> subscriptions = ConcurrentHashMap.newKeySet();

        
        Hbar? defaultMaxTransactionFee = null;

        Hbar defaultMaxQueryPayment = DEFAULT_MAX_QUERY_PAYMENT;
        Network network;
        MirrorNetwork mirrorNetwork;

        private Operator? _operator;

        private Duration requestTimeout = DEFAULT_REQUEST_TIMEOUT;
        private Duration closeTimeout = DEFAULT_CLOSE_TIMEOUT;
        private int maxAttempts = DEFAULT_MAX_ATTEMPTS;
        private volatile Duration maxBackoff = DEFAULT_MAX_BACKOFF;
        private volatile Duration minBackoff = DEFAULT_MIN_BACKOFF;
        private bool autoValidateChecksums = false;
        private bool defaultRegenerateTransactionId = true;
        private readonly bool shouldShutdownExecutor;
        private readonly long Shard;
        private readonly long Realm;
        // If networkUpdatePeriod is null, any network updates in progress will not complete
        @Nullable
        private Duration networkUpdatePeriod;

        @Nullable
        private Task<Void> networkUpdateFuture;

        private Logger logger = new Logger(LogLevel.SILENT);

        /**
         * Constructor.
         *
         * @param executor               the executor
         * @param network                the network
         * @param mirrorNetwork          the mirror network
         * @param shouldShutdownExecutor
         */
        Client(
            ExecutorService executor,
            Network network,
            MirrorNetwork mirrorNetwork,
            Duration? networkUpdateInitialDelay,
            bool shouldShutdownExecutor,
            Duration? networkUpdatePeriod,
            long Shard,
            long Realm) 
        {
            this.executor = executor;
            this.network = network;
            this.mirrorNetwork = mirrorNetwork;
            this.shouldShutdownExecutor = shouldShutdownExecutor;
            this.networkUpdatePeriod = networkUpdatePeriod;
            this.Shard = Shard;
            this.Realm = Realm;
            scheduleNetworkUpdate(networkUpdateInitialDelay);
        }

        /**
         * Extract the executor.
         *
         * @return the executor service
         */
        static ExecutorService createExecutor() {
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
            var network = Network.forTestnet(executor);
            var mirrorNetwork = MirrorNetwork.forTestnet(executor);

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
            var network = Network.forPreviewnet(executor);
            var mirrorNetwork = MirrorNetwork.forPreviewnet(executor);

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
        public static Client FromConfig(Reader json)  {
            return Config.FromJson(json).toClient();
        }

        private static Dictionary<string, AccountId> getNetworkNodes(JsonObject networks) {
            Dictionary<string, AccountId> nodes = new HashMap<>(networks.size());
            for (Map.Entry<string, JsonElement> entry : networks.entrySet()) {
                nodes.put(
                        entry.getValue().toString().replace("\"", ""),
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
            return fromConfigFile(new File(fileName));
        }

        /**
         * Configure a client based on a JSON file.
         *
         * @param file The file containing the client configuration
         * @return {@link Client}
         * @ if IO operations fail
         */
        public static Client FromConfigFile(File file)  {
            return fromConfig(Files.newBufferedReader(file.toPath(), StandardCharsets.UTF_8));
        }

        /**
         * Extract the mirror network node list.
         *
         * @return the list of mirror nodes
         */
        public List<string> GetMirrorNetwork() 
        {
            lock (this) { return mirrorNetwork.GetNetwork(); }
        }

        /**
         * Build the REST base URL for the next healthy mirror node.
         * Returns a string like `https://host[:port]/api/v1`.
         * If the selected mirror node is a local host (localhost/127.0.0.1) returns `http://localhost:{5551|8545}/api/v1`.
         */
        public string GetMirrorRestBaseUrl() 
        {
            try {
                return mirrorNetwork.getRestBaseUrl();
            } catch (InterruptedException e) {
                Thread.currentThread().interrupt();
                throw new IllegalStateException("Interrupted while retrieving mirror base URL", e);
            }
        }

        /**
         * Set the mirror network nodes.
         *
         * @param network list of network nodes
         * @return {@code this}
         * @ when a thread is interrupted while it's waiting, sleeping, or otherwise occupied
         */
        public Client setMirrorNetwork(List<string> network)
        {
            lock (this)
            {
				try
				{
					this.mirrorNetwork.setNetwork(network);
				}
				catch (TimeoutException e)
				{
					throw new RuntimeException(e);
				}

				return this;
			}
        }

        private void scheduleNetworkUpdate(@Nullable Duration delay) 
        {
            lock (this)
            {
				if (delay == null)
				{
					networkUpdateFuture = null;
					return;
				}

				networkUpdateFuture = Delayer.delayFor(delay.toMillis(), executor);
				networkUpdateFuture.thenRun(()-> {
					// Checking networkUpdatePeriod != null must be synchronized, so I've put it in a synchronized method.
					requireNetworkUpdatePeriodNotNull(()-> {
						var fileId = FileId.getAddressBookFileIdFor(this.Shard, this.Realm);

						new AddressBookQuery()
								.setFileId(fileId)
								.executeAsync(this)
								.thenCompose(addressBook->requireNetworkUpdatePeriodNotNull(()-> {
							try
							{
								this.setNetworkFromAddressBook(addressBook);
							}
							catch (Throwable error)
							{
								return Task.failedFuture(error);
							}
							return Task.completedFuture(null);
						}))
                            .exceptionally(error-> {
							logger.warn("Failed to update address book via mirror node query ", error);
							return null;
						});

						scheduleNetworkUpdate(networkUpdatePeriod);
						return null;
					});
				});
			}
        }

        private CompletionStage<?> requireNetworkUpdatePeriodNotNull(Supplier<CompletionStage<?>> task) 
        {
            lock (this)
            {
				return networkUpdatePeriod != null ? task.get() : Task.completedFuture(null);
			}
        }

        private void cancelScheduledNetworkUpdate() {
            if (networkUpdateFuture != null) {
                networkUpdateFuture.cancel(true);
            }
        }

        private void cancelAllSubscriptions() {
            subscriptions.forEach(SubscriptionHandle::unsubscribe);
        }

        void trackSubscription(SubscriptionHandle subscriptionHandle) {
            subscriptions.Add(subscriptionHandle);
        }

        void untrackSubscription(SubscriptionHandle subscriptionHandle) {
            subscriptions.remove(subscriptionHandle);
        }

        /**
         * Replace all nodes in this Client with the nodes in the Address Book
         * and update the address book if necessary.
         *
         * @param addressBook A list of nodes and their metadata
         * @return {@code this}
         */
        public Client setNetworkFromAddressBook(NodeAddressBook addressBook)
        {
            lock (this)
            {
				network.setNetwork(Network.AddressBookToNetwork(addressBook.nodeAddresses));
				network.setAddressBook(addressBook);
				return this;
			}
        }

        /**
         * Extract the network.
         *
         * @return the client's network
         */
        public Dictionary<string, AccountId> GetNetwork() 
        {
            lock (this) return network.getNetwork();
		}

        /**
         * Replace all nodes in this Client with a new set of nodes (e.g. for an Address Book update).
         *
         * @param network a map of node account ID to node URL.
         * @return {@code this} for fluent API usage.
         * @     when shutting down nodes
         * @ when a thread is interrupted while it's waiting, sleeping, or otherwise occupied
         */
        public Client setNetwork(Dictionary<string, AccountId> network)
        {
            lock (this) { this.network.setNetwork(network); }
        }

        /**
         * Set if transport security should be used to connect to mirror nodes.
         * <br>
         * If transport security is enabled all connections to mirror nodes will use TLS.
         *
         * @param transportSecurity - enable or disable transport security for mirror nodes
         * @return {@code this} for fluent API usage.
         * @deprecated Mirror nodes can only be accessed using TLS
         */
        [Obsolete]
        public Client setMirrorTransportSecurity(bool transportSecurity) {
            return this;
        }

        /**
         * Is tls enabled for consensus nodes.
         *
         * @return is tls enabled
         */
        public bool isTransportSecurity() {
            return network.isTransportSecurity();
        }

        /**
         * Set if transport security should be used to connect to consensus nodes.
         * <br>
         * If transport security is enabled all connections to consensus nodes will use TLS, and the server's certificate
         * hash will be compared to the hash stored in the {@link NodeAddressBook} for the given network.
         * <br>
         * *Note*: If transport security is enabled, but {@link Client#isVerifyCertificates()} is disabled then server
         * certificates will not be verified.
         *
         * @param transportSecurity enable or disable transport security for consensus nodes
         * @return {@code this} for fluent API usage.
         * @ when a thread is interrupted while it's waiting, sleeping, or otherwise occupied
         */
        public Client setTransportSecurity(bool transportSecurity)  {
            network.setTransportSecurity(transportSecurity);
            return this;
        }

        /**
         * Is tls enabled for mirror nodes.
         *
         * @return is tls enabled
         */
        public bool mirrorIsTransportSecurity() {
            return mirrorNetwork.isTransportSecurity();
        }

        /**
         * Is certificate verification enabled.
         *
         * @return is certificate verification enabled
         */
        public bool isVerifyCertificates() {
            return network.isVerifyCertificates();
        }

        /**
         * Set if server certificates should be verified against an existing address book
         *
         * @param verifyCertificates - enable or disable certificate verification
         * @return {@code this}
         */
        public Client setVerifyCertificates(bool verifyCertificates) {
            network.setVerifyCertificates(verifyCertificates);
            return this;
        }

        /**
         * Send a ping to the given node.
         *
         * @param nodeAccountId Account ID of the node to ping
         * @        when the transaction times out
         * @ when the precheck fails
         */
        public void Ping(AccountId nodeAccountId) 
        {
            return Ping(nodeAccountId, getRequestTimeout());
        }

        /**
         * Send a ping to the given node.
         *
         * @param nodeAccountId Account ID of the node to ping
         * @param timeout       The timeout after which the execution attempt will be cancelled.
         * @        when the transaction times out
         * @ when the precheck fails
         */
        public void Ping(AccountId nodeAccountId, Duration timeout) 
        {
            new AccountBalanceQuery
            {
                AccountId = 
            }.


			new ()
                    .setAccountId(nodeAccountId)
                    .setNodeAccountIds(Collections.singletonList(nodeAccountId))
                    .execute(this, timeout);

            return null;
        }

        /**
         * Send a ping to the given node asynchronously.
         *
         * @param nodeAccountId Account ID of the node to ping
         * @return an empty future that throws exception if there was an error
         */
        public Task<Void> pingAsync(AccountId nodeAccountId) {
            return pingAsync(nodeAccountId, getRequestTimeout());
        }

        /**
         * Send a ping to the given node asynchronously.
         *
         * @param nodeAccountId Account ID of the node to ping
         * @param timeout       The timeout after which the execution attempt will be cancelled.
         * @return an empty future that throws exception if there was an error
         */
        public Task<Void> pingAsync(AccountId nodeAccountId, Duration timeout) {
            var result = new Task<Void>();
            new AccountBalanceQuery()
                    .setAccountId(nodeAccountId)
                    .setNodeAccountIds(Collections.singletonList(nodeAccountId))
                    .executeAsync(this, timeout)
                    .whenComplete((balance, error) -> {
                        if (error == null) {
                            result.complete(null);
                        } else {
                            result.completeExceptionally(error);
                        }
                    });
            return result;
        }

        /**
         * Send a ping to the given node asynchronously.
         *
         * @param nodeAccountId Account ID of the node to ping
         * @param callback      a BiConsumer which handles the result or error.
         */
        public void pingAsync(AccountId nodeAccountId, BiConsumer<Void, Throwable> callback) {
            ConsumerHelper.biConsumer(pingAsync(nodeAccountId), callback);
        }

        /**
         * Send a ping to the given node asynchronously.
         *
         * @param nodeAccountId Account ID of the node to ping
         * @param timeout       The timeout after which the execution attempt will be cancelled.
         * @param callback      a BiConsumer which handles the result or error.
         */
        public void pingAsync(AccountId nodeAccountId, Duration timeout, BiConsumer<Void, Throwable> callback) {
            ConsumerHelper.biConsumer(pingAsync(nodeAccountId, timeout), callback);
        }

        /**
         * Send a ping to the given node asynchronously.
         *
         * @param nodeAccountId Account ID of the node to ping
         * @param onSuccess     a Consumer which consumes the result on success.
         * @param onFailure     a Consumer which consumes the error on failure.
         */
        public void pingAsync(AccountId nodeAccountId, Consumer<Void> onSuccess, Consumer<Throwable> onFailure) {
            ConsumerHelper.twoConsumers(pingAsync(nodeAccountId), onSuccess, onFailure);
        }

        /**
         * Send a ping to the given node asynchronously.
         *
         * @param nodeAccountId Account ID of the node to ping
         * @param timeout       The timeout after which the execution attempt will be cancelled.
         * @param onSuccess     a Consumer which consumes the result on success.
         * @param onFailure     a Consumer which consumes the error on failure.
         */
        public void pingAsync(AccountId nodeAccountId, Duration timeout, Consumer<Void> onSuccess, Consumer<Throwable> onFailure) {
            ConsumerHelper.twoConsumers(pingAsync(nodeAccountId, timeout), onSuccess, onFailure);
        }

        /**
         * Sends pings to all nodes in the client's network. Combines well with setMaxAttempts(1) to remove all dead nodes
         * from the network.
         *
         * @        when the transaction times out
         * @ when the precheck fails
         */
        public void pingAll() 
        {
            lock (this)  PingAll(GetRequestTimeout());
        }

        /**
         * Sends pings to all nodes in the client's network. Combines well with setMaxAttempts(1) to remove all dead nodes
         * from the network.
         *
         * @param timeoutPerPing The timeout after which each execution attempt will be cancelled.
         * @        when the transaction times out
         * @ when the precheck fails
         */
        public void PingAll(Duration timeoutPerPing) 
        {
            lock (this)
				foreach (var nodeAccountId in network.getNetwork().values())
					Ping(nodeAccountId, timeoutPerPing);
		}

        /**
         * Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
         * all dead nodes from the network.
         *
         * @return an empty future that throws exception if there was an error
         */
        public Task pingAllAsync() {

            lock (this) return PingAllAsync(getRequestTimeout());
		}

        /**
         * Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
         * all dead nodes from the network.
         *
         * @param timeoutPerPing The timeout after which each execution attempt will be cancelled.
         * @return an empty future that throws exception if there was an error
         */
        public Task pingAllAsync(Duration timeoutPerPing) 
        {
            lock (this)
            {
		        var network = this.network.getNetwork();
		        var list = new ArrayList<Task<Void>>(network.size());

		        for (var nodeAccountId : network.values()) {
	                list.Add(pingAsync(nodeAccountId, timeoutPerPing));
                }

                return Task.allOf(list.toArray(new Task<?>[0]))
		                .thenApply((v)-> null);
            }
        }

        /**
         * Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
         * all dead nodes from the network.
         *
         * @param callback a BiConsumer which handles the result or error.
         */
        public void pingAllAsync(BiConsumer<Void, Throwable> callback) {
            ConsumerHelper.biConsumer(pingAllAsync(), callback);
        }

        /**
         * Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
         * all dead nodes from the network.
         *
         * @param timeoutPerPing The timeout after which each execution attempt will be cancelled.
         * @param callback       a BiConsumer which handles the result or error.
         */
        public void pingAllAsync(Duration timeoutPerPing, BiConsumer<Void, Throwable> callback) {
            ConsumerHelper.biConsumer(pingAllAsync(timeoutPerPing), callback);
        }

        /**
         * Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
         * all dead nodes from the network.
         *
         * @param onSuccess a Consumer which consumes the result on success.
         * @param onFailure a Consumer which consumes the error on failure.
         */
        public void pingAllAsync(Consumer<Void> onSuccess, Consumer<Throwable> onFailure) {
            ConsumerHelper.twoConsumers(pingAllAsync(), onSuccess, onFailure);
        }

        /**
         * Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
         * all dead nodes from the network.
         *
         * @param timeoutPerPing The timeout after which each execution attempt will be cancelled.
         * @param onSuccess      a Consumer which consumes the result on success.
         * @param onFailure      a Consumer which consumes the error on failure.
         */
        public void pingAllAsync(Duration timeoutPerPing, Consumer<Void> onSuccess, Consumer<Throwable> onFailure) {
            ConsumerHelper.twoConsumers(pingAllAsync(timeoutPerPing), onSuccess, onFailure);
        }

        /**
         * Set the account that will, by default, be paying for transactions and queries built with this client.
         * <p>
         * The operator account ID is used to generate the default transaction ID for all transactions executed with this
         * client.
         * <p>
         * The operator private key is used to sign all transactions executed by this client.
         *
         * @param accountId  The AccountId of the operator
         * @param privateKey The PrivateKey of the operator
         * @return {@code this}
         */
        public Client setOperator(AccountId accountId, PrivateKey privateKey) 
        {
            lock (this) { return setOperatorWith(accountId, privateKey.getPublicKey(), privateKey::sign); }
        }

        /**
         * Sets the account that will, by default, by paying for transactions and queries built with this client.
         * <p>
         * The operator account ID is used to generate a default transaction ID for all transactions executed with this
         * client.
         * <p>
         * The `transactionSigner` is invoked to sign all transactions executed by this client.
         *
         * @param accountId         The AccountId of the operator
         * @param publicKey         The PrivateKey of the operator
         * @param transactionSigner The signer for the operator
         * @return {@code this}
         */
        public Client setOperatorWith(AccountId accountId, PublicKey publicKey, UnaryOperator<byte[]> transactionSigner) 
        {
            lock (this)
            {
				if (getNetworkName() != null)
				{
					try
					{
						accountId.validateChecksum(this);
					}
					catch (BadEntityIdException exc)
					{
						throw new ArgumentException(
								"Tried to set the client operator account ID to an account ID with an invalid Checksum: "
										+ exc.getMessage());
					}
				}

				this.operator = new Operator(accountId, publicKey, transactionSigner);
				return this;
			}
        }

        /**
         * Current name of the network; corresponds to ledger ID in entity ID Checksum calculations.
         *
         * @return the network name
         * @deprecated use {@link #getLedgerId()} instead
         */
        [Obsolete]
        public NetworkName getNetworkName() 
        {
            lock (this)
            {
				var ledgerId = network.getLedgerId();
				return ledgerId == null ? null : ledgerId.toNetworkName();
			}
        }

        /**
         * Set the network name to a particular value. Useful when constructing a network which is a subset of an existing
         * known network.
         *
         * @param networkName the desired network
         * @return {@code this}
         * @deprecated use {@link #setLedgerId(LedgerId)} instead
         */
        [Obsolete]
        public Client setNetworkName(@Nullable NetworkName networkName) {
            lock (this)
        }

        /**
         * Current LedgerId of the network; corresponds to ledger ID in entity ID Checksum calculations.
         *
         * @return the ledger id
         */
        @Nullable
        public synchronized LedgerId getLedgerId() {
            return network.getLedgerId();
        }

        /**
         * Set the LedgerId to a particular value. Useful when constructing a network which is a subset of an existing known
         * network.
         *
         * @param ledgerId the desired ledger id
         * @return {@code this}
         */
        public synchronized Client setLedgerId(@Nullable LedgerId ledgerId) {
            this.network.setLedgerId(ledgerId);
            return this;
        }

        /**
         * Max number of attempts a request executed with this client will do.
         *
         * @return the maximus attempts
         */
        public synchronized int getMaxAttempts() 

        /**
         * Set the max number of attempts a request executed with this client will do.
         *
         * @param maxAttempts the desired max attempts
         * @return {@code this}
         */
        public synchronized Client setMaxAttempts(int maxAttempts) {
            if (maxAttempts <= 0) {
                throw new ArgumentException("maxAttempts must be greater than zero");
            }
            this.maxAttempts = maxAttempts;
            return this;
        }

        /**
         * The maximum amount of time to wait between retries
         *
         * @return maxBackoff
         */
        public Duration getMaxBackoff() {
            return maxBackoff;
        }

        /**
         * The maximum amount of time to wait between retries. Every retry attempt will increase the wait time exponentially
         * until it reaches this time.
         *
         * @param maxBackoff The maximum amount of time to wait between retries
         * @return {@code this}
         */
        public Client setMaxBackoff(Duration maxBackoff) {
            if (maxBackoff == null || maxBackoff.toNanos() < 0) {
                throw new ArgumentException("maxBackoff must be a positive duration");
            } else if (maxBackoff.compareTo(minBackoff) < 0) {
                throw new ArgumentException("maxBackoff must be greater than or equal to minBackoff");
            }
            this.maxBackoff = maxBackoff;
            return this;
        }

        /**
         * The minimum amount of time to wait between retries
         *
         * @return minBackoff
         */
        public Duration getMinBackoff() {
            return minBackoff;
        }

        /**
         * The minimum amount of time to wait between retries. When retrying, the delay will start at this time and increase
         * exponentially until it reaches the maxBackoff.
         *
         * @param minBackoff The minimum amount of time to wait between retries
         * @return {@code this}
         */
        public Client setMinBackoff(Duration minBackoff) {
            if (minBackoff == null || minBackoff.toNanos() < 0) {
                throw new ArgumentException("minBackoff must be a positive duration");
            } else if (minBackoff.compareTo(maxBackoff) > 0) {
                throw new ArgumentException("minBackoff must be less than or equal to maxBackoff");
            }
            this.minBackoff = minBackoff;
            return this;
        }

        /**
         * Max number of times any node in the network can receive a bad gRPC status before being removed from the network.
         *
         * @return the maximum node attempts
         */
        public synchronized int getMaxNodeAttempts() {
            return network.getMaxNodeAttempts();
        }

        /**
         * Set the max number of times any node in the network can receive a bad gRPC status before being removed from the
         * network.
         *
         * @param maxNodeAttempts the desired minimum attempts
         * @return {@code this}
         */
        public synchronized Client setMaxNodeAttempts(int maxNodeAttempts) {
            this.network.setMaxNodeAttempts(maxNodeAttempts);
            return this;
        }

        /**
         * The minimum backoff time for any node in the network.
         *
         * @return the wait time
         * @deprecated - Use {@link Client#getNodeMaxBackoff()} instead
         */
        [Obsolete]
        public synchronized Duration getNodeWaitTime() {
            return getNodeMinBackoff();
        }

        /**
         * Set the minimum backoff time for any node in the network.
         *
         * @param nodeWaitTime the wait time
         * @return the updated client
         * @deprecated - Use {@link Client#setNodeMinBackoff(Duration)} ()} instead
         */
        [Obsolete]
        public synchronized Client setNodeWaitTime(Duration nodeWaitTime) {
            return setNodeMinBackoff(nodeWaitTime);
        }

        /**
         * The minimum backoff time for any node in the network.
         *
         * @return the minimum backoff time
         */
        public synchronized Duration getNodeMinBackoff() {
            return network.getMinNodeBackoff();
        }

        /**
         * Set the minimum backoff time for any node in the network.
         *
         * @param minBackoff the desired minimum backoff time
         * @return {@code this}
         */
        public synchronized Client setNodeMinBackoff(Duration minBackoff) {
            network.setMinNodeBackoff(minBackoff);
            return this;
        }

        /**
         * The maximum backoff time for any node in the network.
         *
         * @return the maximum node backoff time
         */
        public synchronized Duration getNodeMaxBackoff() {
            return network.getMaxNodeBackoff();
        }

        /**
         * Set the maximum backoff time for any node in the network.
         *
         * @param maxBackoff the desired max backoff time
         * @return {@code this}
         */
        public synchronized Client setNodeMaxBackoff(Duration maxBackoff) {
            network.setMaxNodeBackoff(maxBackoff);
            return this;
        }

        /**
         * Extract the minimum node readmit time.
         *
         * @return the minimum node readmit time
         */
        public Duration getMinNodeReadmitTime() {
            return network.getMinNodeReadmitTime();
        }

        /**
         * Assign the minimum node readmit time.
         *
         * @param minNodeReadmitTime the requested duration
         * @return {@code this}
         */
        public Client setMinNodeReadmitTime(Duration minNodeReadmitTime) {
            network.setMinNodeReadmitTime(minNodeReadmitTime);
            return this;
        }

        /**
         * Extract the node readmit time.
         *
         * @return the maximum node readmit time
         */
        public Duration getMaxNodeReadmitTime() {
            return network.getMaxNodeReadmitTime();
        }

        /**
         * Assign the maximum node readmit time.
         *
         * @param maxNodeReadmitTime the maximum node readmit time
         * @return {@code this}
         */
        public Client setMaxNodeReadmitTime(Duration maxNodeReadmitTime) {
            network.setMaxNodeReadmitTime(maxNodeReadmitTime);
            return this;
        }

        /**
         * Set the max amount of nodes that will be chosen per request. By default, the request will use 1/3rd the network
         * nodes per request.
         *
         * @param maxNodesPerTransaction the desired number of nodes
         * @return {@code this}
         */
        public synchronized Client setMaxNodesPerTransaction(int maxNodesPerTransaction) {
            this.network.setMaxNodesPerRequest(maxNodesPerTransaction);
            return this;
        }

        /**
         * Enable or disable automatic entity ID Checksum validation.
         *
         * @param value the desired value
         * @return {@code this}
         */
        public synchronized Client setAutoValidateChecksums(bool value) {
            autoValidateChecksums = value;
            return this;
        }

        /**
         * Is automatic entity ID Checksum validation enabled.
         *
         * @return is validation enabled
         */
        public synchronized bool isAutoValidateChecksumsEnabled() {
            return autoValidateChecksums;
        }

        /**
         * Get the ID of the operator. Useful when the client was constructed from file.
         *
         * @return {AccountId}
         */
        @Nullable
        public synchronized AccountId getOperatorAccountId() {
            if (operator == null) {
                return null;
            }

            return operator.accountId;
        }

        /**
         * Get the key of the operator. Useful when the client was constructed from file.
         *
         * @return {PublicKey}
         */
        @Nullable
        public synchronized PublicKey getOperatorPublicKey() {
            if (operator == null) {
                return null;
            }

            return operator.publicKey;
        }

        /**
         * The default maximum fee used for transactions.
         *
         * @return the max transaction fee
         */
        @Nullable
        public synchronized Hbar getDefaultMaxTransactionFee() {
            return defaultMaxTransactionFee;
        }

        /**
         * Set the maximum fee to be paid for transactions executed by this client.
         * <p>
         * Because transaction fees are always maximums, this will simply add a call to
         * {@link Transaction#setMaxTransactionFee(Hbar)} on every new transaction. The actual fee assessed for a given
         * transaction may be less than this value, but never greater.
         *
         * @param defaultMaxTransactionFee The Hbar to be set
         * @return {@code this}
         */
        public synchronized Client setDefaultMaxTransactionFee(Hbar defaultMaxTransactionFee) {
            Objects.requireNonNull(defaultMaxTransactionFee);
            if (defaultMaxTransactionFee.toTinybars() < 0) {
                throw new ArgumentException("maxTransactionFee must be non-negative");
            }

            this.defaultMaxTransactionFee = defaultMaxTransactionFee;
            return this;
        }

        /**
         * Set the maximum fee to be paid for transactions executed by this client.
         * <p>
         * Because transaction fees are always maximums, this will simply add a call to
         * {@link Transaction#setMaxTransactionFee(Hbar)} on every new transaction. The actual fee assessed for a given
         * transaction may be less than this value, but never greater.
         *
         * @param maxTransactionFee The Hbar to be set
         * @return {@code this}
         * @deprecated Use {@link #setDefaultMaxTransactionFee(Hbar)} instead.
         */
        [Obsolete]
        public synchronized Client setMaxTransactionFee(Hbar maxTransactionFee) {
            return setDefaultMaxTransactionFee(maxTransactionFee);
        }

        /**
         * Extract the maximum query payment.
         *
         * @return the default maximum query payment
         */
        public synchronized Hbar getDefaultMaxQueryPayment() {
            return defaultMaxQueryPayment;
        }

        /**
         * Set the maximum default payment allowable for queries.
         * <p>
         * When a query is executed without an explicit {@link Query#setQueryPayment(Hbar)} call, the client will first
         * request the cost of the given query from the node it will be submitted to and attach a payment for that amount
         * from the operator account on the client.
         * <p>
         * If the returned value is greater than this value, a {@link MaxQueryPaymentExceededException} will be thrown from
         * {@link Query#execute(Client)} or returned in the second callback of
         * {@link Query#executeAsync(Client, Consumer, Consumer)}.
         * <p>
         * Set to 0 to disable automatic implicit payments.
         *
         * @param defaultMaxQueryPayment The Hbar to be set
         * @return {@code this}
         */
        public synchronized Client setDefaultMaxQueryPayment(Hbar defaultMaxQueryPayment) {
            Objects.requireNonNull(defaultMaxQueryPayment);
            if (defaultMaxQueryPayment.toTinybars() < 0) {
                throw new ArgumentException("defaultMaxQueryPayment must be non-negative");
            }

            this.defaultMaxQueryPayment = defaultMaxQueryPayment;
            return this;
        }

        /**
         * @param maxQueryPayment The Hbar to be set
         * @return {@code this}
         * @deprecated Use {@link #setDefaultMaxQueryPayment(Hbar)} instead.
         */
        [Obsolete]
        public synchronized Client setMaxQueryPayment(Hbar maxQueryPayment) {
            return setDefaultMaxQueryPayment(maxQueryPayment);
        }

        /**
         * Should the transaction id be regenerated?
         *
         * @return the default regenerate transaction id
         */
        public synchronized bool getDefaultRegenerateTransactionId() {
            return defaultRegenerateTransactionId;
        }

        /**
         * Assign the default regenerate transaction id.
         *
         * @param regenerateTransactionId should there be a regenerated transaction id
         * @return {@code this}
         */
        public synchronized Client setDefaultRegenerateTransactionId(bool regenerateTransactionId) {
            this.defaultRegenerateTransactionId = regenerateTransactionId;
            return this;
        }

        /**
         * Maximum amount of time a request can run
         *
         * @return the timeout value
         */
        public synchronized Duration getRequestTimeout() {
            return requestTimeout;
        }

        /**
         * Set the maximum amount of time a request can run. Used only in async variants of methods.
         *
         * @param requestTimeout the timeout value
         * @return {@code this}
         */
        public synchronized Client setRequestTimeout(Duration requestTimeout) {
            this.requestTimeout = Objects.requireNonNull(requestTimeout);
            return this;
        }

        /**
         * Maximum amount of time closing a network can take.
         *
         * @return the timeout value
         */
        public Duration getCloseTimeout() {
            return closeTimeout;
        }

        /**
         * Set the maximum amount of time closing a network can take.
         *
         * @param closeTimeout the timeout value
         * @return {@code this}
         */
        public Client setCloseTimeout(Duration closeTimeout) {
            this.closeTimeout = Objects.requireNonNull(closeTimeout);
            network.setCloseTimeout(closeTimeout);
            mirrorNetwork.setCloseTimeout(closeTimeout);
            return this;
        }

        /**
         * Maximum amount of time a gRPC request can run
         *
         * @return the gRPC deadline value
         */
        public Duration getGrpcDeadline() {
            return grpcDeadline.get();
        }

        /**
         * Set the maximum amount of time a gRPC request can run.
         *
         * @param grpcDeadline the gRPC deadline value
         * @return {@code this}
         */
        public Client setGrpcDeadline(Duration grpcDeadline) {
            this.grpcDeadline.set(Objects.requireNonNull(grpcDeadline));
            return this;
        }

        /**
         * Extract the operator.
         *
         * @return the operator
         */
        @Nullable
        synchronized Operator getOperator() {
            return this.operator;
        }

        /**
         * Get the period for updating the Address Book
         *
         * @return the networkUpdatePeriod
         */
        @Nullable
        public synchronized Duration getNetworkUpdatePeriod() {
            return this.networkUpdatePeriod;
        }

        /**
         * Set the period for updating the Address Book
         *
         * <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
         * because it uses features introduced in API level 31 (Android 12).</p>*
         *
         * @param networkUpdatePeriod the period for updating the Address Book
         * @return {@code this}
         */
        public synchronized Client setNetworkUpdatePeriod(Duration networkUpdatePeriod) {
            cancelScheduledNetworkUpdate();
            this.networkUpdatePeriod = networkUpdatePeriod;
            scheduleNetworkUpdate(networkUpdatePeriod);
            return this;
        }

        /**
         * Trigger an immediate address book update to refresh the client's network with the latest node information.
         * This is useful when encountering INVALID_NODE_ACCOUNT_ID errors to ensure subsequent transactions
         * use the correct node account IDs.
         *
         * @return {@code this}
         */
        public synchronized Client updateNetworkFromAddressBook() {
            try {
                var fileId = FileId.getAddressBookFileIdFor(this.Shard, this.Realm);

                logger.debug("Fetching address book from file {}", fileId);

                // Execute synchronously - no async complexity
                var addressBook = new AddressBookQuery().setFileId(fileId).execute(this); // ← Synchronous!

                logger.debug("Received address book with {} nodes", addressBook.nodeAddresses.size());

                // Update the network
                this.setNetworkFromAddressBook(addressBook);

                logger.info("Address book update completed successfully");

            } catch (TimeoutException e) {
                logger.warn("Failed to fetch address book: {}", e.getMessage());
            } catch (Exception e) {
                logger.warn("Failed to update address book", e);
            }

            return this;
        }

        public Logger getLogger() {
            return this.logger;
        }

        public Client setLogger(Logger logger) {
            this.logger = logger;
            return this;
        }

        /**
         * Get the current default Realm for new Client instances.
         *
         * @return the default Realm
         */
        public long getRealm() {
            return this.Realm;
        }

        /**
         * Get the current default Shard for new Client instances.
         *
         * @return the default Shard
         */
        public long getShard() {
            return this.Shard;
        }

        /**
         * Initiates an orderly shutdown of all channels (to the Hedera network) in which preexisting transactions or
         * queries continue but more would be immediately cancelled.
         *
         * <p>After this method returns, this client can be re-used. Channels will be re-established as
         * needed.
         *
         * @ if the mirror network doesn't close in time
         */
        @Override
        public synchronized void close()  {
            close(closeTimeout);
        }

        /**
         * Initiates an orderly shutdown of all channels (to the Hedera network) in which preexisting transactions or
         * queries continue but more would be immediately cancelled.
         *
         * <p>After this method returns, this client can be re-used. Channels will be re-established as
         * needed.
         *
         * @param timeout The Duration to be set
         * @ if the mirror network doesn't close in time
         */
        public synchronized void close(Duration timeout)  {
            var closeDeadline = DateTimeOffset.now().plus(timeout);

            networkUpdatePeriod = null;
            cancelScheduledNetworkUpdate();
            cancelAllSubscriptions();

            network.beginClose();
            mirrorNetwork.beginClose();

            var networkError = network.awaitClose(closeDeadline, null);
            var mirrorNetworkError = mirrorNetwork.awaitClose(closeDeadline, networkError);

            // https://docs.oracle.com/javase/8/docs/api/java/util/concurrent/ExecutorService.html
            if (shouldShutdownExecutor) {
                try {
                    executor.shutdown();
                    if (!executor.awaitTermination(timeout.getSeconds() / 2, TimeUnit.SECONDS)) {
                        executor.shutdownNow();
                        if (!executor.awaitTermination(timeout.getSeconds() / 2, TimeUnit.SECONDS)) {
                            logger.warn("Pool did not terminate");
                        }
                    }
                } catch (InterruptedException ex) {
                    executor.shutdownNow();
                    Thread.currentThread().interrupt();
                }
            }

            if (mirrorNetworkError != null) {
                if (mirrorNetworkError instanceof TimeoutException ex) {
                    throw ex;
                } else {
                    throw new RuntimeException(mirrorNetworkError);
                }
            }
        }

        static class Operator {
            readonly AccountId accountId;
            readonly PublicKey publicKey;
            readonly UnaryOperator<byte[]> transactionSigner;

            Operator(AccountId accountId, PublicKey publicKey, UnaryOperator<byte[]> transactionSigner) {
                this.accountId = accountId;
                this.publicKey = publicKey;
                this.transactionSigner = transactionSigner;
            }
        }

        private static class Config {
            @Nullable
            private JsonElement network;

            @Nullable
            private JsonElement networkName;

            @Nullable
            private ConfigOperator operator;

            @Nullable
            private JsonElement mirrorNetwork;

            @Nullable
            private JsonElement Shard;

            @Nullable
            private JsonElement Realm;

            private static class ConfigOperator {
                @Nullable
                private string accountId;

                @Nullable
                private string privateKey;
            }

            private static Config fromString(string json) {
                return new Gson().FromJson((Reader) new StringReader(json), Config.class);
            }

            private static Config fromJson(Reader json) {
                return new Gson().FromJson(json, Config.class);
            }

            private Client toClient() , InterruptedException {
                Client client = initializeWithNetwork();
                setOperatorOn(client);
                setMirrorNetworkOn(client);
                return client;
            }

            private Client initializeWithNetwork()  {
                if (network == null) {
                    throw new Exception("Network is not set in provided json object");
                }

                Client client;
                if (network.isJsonObject()) {
                    client = clientFromNetworkJson();
                } else {
                    client = clientFromNetworkString();
                }
                return client;
            }

            private Client clientFromNetworkJson() {
                Client client;
                var networkJson = network.getAsJsonObject();
                Dictionary<string, AccountId> nodes = Client.getNetworkNodes(networkJson);
                var executor = createExecutor();
                var network = Network.forNetwork(executor, nodes);
                var mirrorNetwork = MirrorNetwork.forNetwork(executor, new ArrayList<>());
                var shardValue = Shard != null ? Shard.getAsLong() : 0;
                var realmValue = Realm != null ? Realm.getAsLong() : 0;
                client = new Client(executor, network, mirrorNetwork, null, true, null, shardValue, realmValue);
                setNetworkNameOn(client);
                return client;
            }

            private void setNetworkNameOn(Client client) {
                if (networkName != null) {
                    var networkNameString = networkName.getAsString();
                    try {
                        client.setNetworkName(NetworkName.FromString(networkNameString));
                    } catch (Exception ignored) {
                        throw new ArgumentException("networkName in config was \"" + networkNameString
                                + "\", expected either \"mainnet\", \"testnet\" or \"previewnet\"");
                    }
                }
            }

            private Client clientFromNetworkString() {
                return switch (network.getAsString()) {
                    case MAINNET -> Client.forMainnet();
                    case TESTNET -> Client.forTestnet();
                    case PREVIEWNET -> Client.forPreviewnet();
                    default -> throw new JsonParseException("Illegal argument for network.");
                };
            }

            private void setMirrorNetworkOn(Client client)  {
                if (mirrorNetwork != null) {
                    setMirrorNetwork(client);
                }
            }

            private void setMirrorNetwork(Client client)  {
                if (mirrorNetwork.isJsonArray()) {
                    setMirrorNetworksFromJsonArray(client);
                } else {
                    setMirrorNetworkFromString(client);
                }
            }

            private void setMirrorNetworkFromString(Client client) {
                string mirror = mirrorNetwork.getAsString();
                switch (mirror) {
                    case Client.MAINNET -> client.mirrorNetwork = MirrorNetwork.forMainnet(client.executor);
                    case Client.TESTNET -> client.mirrorNetwork = MirrorNetwork.forTestnet(client.executor);
                    case Client.PREVIEWNET -> client.mirrorNetwork = MirrorNetwork.forPreviewnet(client.executor);
                    default -> throw new JsonParseException("Illegal argument for mirrorNetwork.");
                }
            }

            private void setMirrorNetworksFromJsonArray(Client client)  {
                var mirrors = mirrorNetwork.getAsJsonArray();
                List<string> listMirrors = getListMirrors(mirrors);
                client.setMirrorNetwork(listMirrors);
            }

            private List<string> getListMirrors(JsonArray mirrors) {
                List<string> listMirrors = new ArrayList<>(mirrors.size());
                for (var i = 0; i < mirrors.size(); i++) {
                    listMirrors.Add(mirrors.get(i).getAsString().replace("\"", ""));
                }
                return listMirrors;
            }

            private void setOperatorOn(Client client) {
                if (operator != null) {
                    AccountId operatorAccount = AccountId.FromString(operator.accountId);
                    PrivateKey privateKey = PrivateKey.FromString(operator.privateKey);
                    client.setOperator(operatorAccount, privateKey);
                }
            }
        }
    }

}