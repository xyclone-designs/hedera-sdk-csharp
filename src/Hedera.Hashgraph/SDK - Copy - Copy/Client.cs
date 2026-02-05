// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Managed client for use on the Hedera Hashgraph network.
    /// </summary>
    public sealed partial class Client : IDisposable
    {
        public static readonly int DEFAULT_MAX_ATTEMPTS = 10;
        public static readonly Duration DEFAULT_MAX_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromSeconds(8));
        public static readonly Duration DEFAULT_MIN_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromMilliseconds(250));
        public static readonly Duration DEFAULT_MAX_NODE_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromHours(1));
        public static readonly Duration DEFAULT_MIN_NODE_BACKOFF = Duration.FromTimeSpan(TimeSpan.FromSeconds(8));
        public static readonly Duration DEFAULT_CLOSE_TIMEOUT = Duration.FromTimeSpan(TimeSpan.FromSeconds(30));
        public static readonly Duration DEFAULT_REQUEST_TIMEOUT = Duration.FromTimeSpan(TimeSpan.FromMinutes(2));
        public static readonly Duration DEFAULT_GRPC_DEADLINE = Duration.FromTimeSpan(TimeSpan.FromSeconds(10));
        public static readonly Duration DEFAULT_NETWORK_UPDATE_PERIOD = Duration.FromTimeSpan(TimeSpan.FromHours(24));
        // Initial delay of 10 seconds before we update the network for the first time,
        // so that this doesn't happen in unit tests.
        static readonly Duration NETWORK_UPDATE_INITIAL_DELAY = Duration.FromTimeSpan(TimeSpan.FromSeconds(10));
		private static readonly Hbar DEFAULT_MAX_QUERY_PAYMENT = new (1);
        private const string MAINNET = "mainnet";
        private const string TESTNET = "testnet";
        private const string PREVIEWNET = "previewnet";
        readonly ExecutorService executor;
        private readonly AtomicReference<Duration> grpcDeadline = new AtomicReference(DEFAULT_GRPC_DEADLINE);
        private readonly HashSet<SubscriptionHandle> subscriptions = ConcurrentDictionary.NewKeySet();
        Hbar? defaultMaxTransactionFee = null;
        Hbar defaultMaxQueryPayment = DEFAULT_MAX_QUERY_PAYMENT;
        protected Network network;
        MirrorNetwork mirrorNetwork;
        private int maxAttempts = DEFAULT_MAX_ATTEMPTS;
        private volatile Duration maxBackoff = DEFAULT_MAX_BACKOFF;
        private volatile Duration minBackoff = DEFAULT_MIN_BACKOFF;
        private bool autoValidateChecksums = false;
        private bool defaultRegenerateTransactionId = true;
        private readonly bool shouldShutdownExecutor;

        // If networkUpdatePeriod is null, any network updates in progress will not complete
        private Duration networkUpdatePeriod;
        private Task networkUpdateFuture;
        private Logger logger = new (LogLevel.Silent);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="executor">the executor</param>
        /// <param name="network">the network</param>
        /// <param name="mirrorNetwork">the mirror network</param>
        /// <param name="shouldShutdownExecutor"></param>
        internal Client(ExecutorService executor, Network network, MirrorNetwork mirrorNetwork, Duration networkUpdateInitialDelay, bool shouldShutdownExecutor, Duration networkUpdatePeriod, long shard, long realm)
        {
            executor = executor;
            network = network;
            mirrorNetwork = mirrorNetwork;
            shouldShutdownExecutor = shouldShutdownExecutor;
            networkUpdatePeriod = networkUpdatePeriod;
            shard = shard;
            realm = realm;
            ScheduleNetworkUpdate(networkUpdateInitialDelay);
        }

        /// <summary>
        /// Extract the executor.
        /// </summary>
        /// <returns>the executor service</returns>
        private static ExecutorService CreateExecutor()
        {
            var threadFactory = new ThreadFactoryBuilder().SetNameFormat("hedera-sdk-%d").SetDaemon(true).Build();
            int nThreads = Runtime.GetRuntime().AvailableProcessors();
            
            return new ThreadPoolExecutor(nThreads, nThreads, 0, TimeUnit.MILLISECONDS, new LinkedBlockingQueue(), threadFactory, new CallerRunsPolicy());
        }
		private static Dictionary<string, AccountId> GetNetworkNodes(JsonObject networks)
		{
			Dictionary<string, AccountId> nodes = new(networks.Count);

			foreach (KeyValuePair<string, JsonNode?> entry in networks.AsEnumerable())
				if (entry.Value is not null)
					nodes.Add(entry.Value.ToString().Replace("\"", ""), AccountId.FromString(entry.Key.Replace("\"", "")));

			return nodes;
		}

		/// <summary>
		/// Set up the client for the selected network.
		/// </summary>
		/// <param name="name">the selected network</param>
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
		/// same Hedera network. Failure to do so will result in undefined behavior.
		/// The client will load balance all requests to Hedera using a simple round-robin scheme to
		/// chose nodes to send transactions to. For one transaction, at most 1/3 of the nodes will be tried.
		/// </summary>
		/// <param name="networkMap">the map of node IDs to node addresses that make up the network.</param>
		/// <param name="executor">runs the grpc requests asynchronously.</param>
		/// <returns>{@link Client}</returns>
		public static Client ForNetwork(Dictionary<string, AccountId> networkMap, ExecutorService executor)
        {
            var network = Network.ForNetwork(executor, networkMap);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
            return new Client(executor, network, mirrorNetwork, null, false, null, 0, 0);
        }
        /// <summary>
        /// Construct a client given a set of nodes.
        /// 
        /// <p>It is the responsibility of the caller to ensure that all nodes in the map are part of the
        /// same Hedera network. Failure to do so will result in undefined behavior.
        /// 
        /// <p>The client will load balance all requests to Hedera using a simple round-robin scheme to
        /// chose nodes to send transactions to. For one transaction, at most 1/3 of the nodes will be tried.
        /// </summary>
        /// <param name="networkMap">the map of node IDs to node addresses that make up the network.</param>
        /// <returns>{@link Client}</returns>
        public static Client ForNetwork(Dictionary<string, AccountId> networkMap)
        {
            ExeutorService executor = CreateExecutor();
            var isValidNetwork = true;
            long shard = 0;
            long realm = 0;
            foreach (AccountId accountId in networkMap.Values)
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

            var network = Network.ForNetwork(executor, networkMap);
            var mirrorNetwork = MirrorNetwork.ForNetwork(executor, new ());
            return new Client(executor, network, mirrorNetwork, null, true, null, shard, realm);
        }
        /// <summary>
        /// Set up the client from selected mirror network.
        /// Using default `0` values for realm and shard for retrieving addressBookFileId
        /// </summary>
        /// <param name="mirrorNetworkList"></param>
        /// <returns></returns>
        public static Client ForMirrorNetwork(IList<string> mirrorNetworkList)
        {
            return ForMirrorNetwork(mirrorNetworkList, 0, 0);
        }
        /// <summary>
        /// Set up the client from selected mirror network and given realm and shard
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
            var addressBook = new AddressBookQuery().SetFileId(FileId.GetAddressBookFileIdFor(shard, realm)).Execute(client);
            client.SetNetworkFromAddressBook(addressBook);
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
        /// Configure a client based off the given JSON reader.
        /// </summary>
        /// <param name="json">The Reader containing the client configuration</param>
        /// <returns>{@link Client}</returns>
        /// <exception cref="Exception">if the config is incorrect</exception>
        public static Client FromConfig(Reader json)
        {
            return Config.FromJson(json).ToClient();
        }
		/// <summary>
		/// Configure a client based on a JSON file.
		/// </summary>
		/// <param name="file">The file containing the client configuration</param>
		/// <returns>{@link Client}</returns>
		/// <exception cref="IOException">if IO operations fail</exception>
		public static Client FromConfigFile(File file)
		{
			return FromConfig(Files.NewBufferedReader(file.ToPath(), StandardCharsets.UTF_8));
		}
		/// <summary>
		/// Configure a client based on a JSON file at the given path.
		/// </summary>
		/// <param name="fileName">The string containing the file path</param>
		/// <returns>{@link Client}</returns>
		/// <exception cref="IOException">if IO operations fail</exception>
		public static Client FromConfigFile(string fileName)
		{
			return FromConfigFile(new File(fileName));
		}

		public Logger Logger { get; set; }

		/// <summary>
		/// Current LedgerId of the network; corresponds to ledger ID in entity ID checksum calculations.
		/// </summary>
		public LedgerId LedgerId
		{
			get
			{
				lock (this)
				{
					return network.LedgerId;
				}
			}
			set
			{
				lock (this)
				{
					network.SetLedgerId(value);
				}
			}
		}
		/// <summary>
		/// Current name of the network; corresponds to ledger ID in entity ID checksum calculations.
		/// </summary>
		/// <remarks>@deprecated Use <see cref="LedgerId"/> instead.</remarks>
		public NetworkName NetworkName
		{
			get
			{
				lock (this)
				{
					var ledgerId = network.LedgerId;

					return ledgerId.ToNetworkName();
				}
			}
			set
			{
				lock (this)
				{
					network.LedgerId = LedgerId.FromNetworkName(value);
				}
			}
		}
		/// <summary>
		/// Replace all nodes in this Client with the nodes in the Address Book
		/// and update the address book if necessary.
		/// </summary>
		public NodeAddressBook NetworkFromAddressBook
		{
			set
			{
				lock (this)
				{
					network.SetNetwork(Network.AddressBookToNetwork(value.NodeAddresses));
					network.SetAddressBook(value);
				}
			}
		}

		/// <summary>
		/// Extract the mirror network node list.
		/// </summary>
		/// <returns>the list of mirror nodes</returns>
		public IList<string> MirrorNetwork
		{
			get
			{
				lock (this)
				{
					return mirrorNetwork.GetNetwork();
				}
			}
			set
			{
				lock (this)
				{
					try
					{
						mirrorNetwork.SetNetwork(value);
					}
					catch (TimeoutException e)
					{
						throw new Exception(string.Empty, e);
					}
				}
			}
		}
		/// <summary>
		/// Build the REST base URL for the next healthy mirror node.
		/// Returns a string like `https://host[:port]/api/v1`.
		/// If the selected mirror node is a local host (localhost/127.0.0.1) returns `http://localhost:{5551|8545}/api/v1`.
		/// </summary>
		public string MirrorRestBaseUrl
		{
			get
			{
				try
				{
					return mirrorNetwork.GetRestBaseUrl();
				}
				catch (ThreadInterruptedException e)
				{
					Thread.CurrentThread.Interrupt();
					throw new InvalidOperationException("Interrupted while retrieving mirror base URL", e);
				}
			}
		}
		/// <summary>
		/// Set if transport security should be used to connect to mirror nodes.
		/// <br/>
		/// If transport security is enabled all connections to mirror nodes will use TLS.
		/// </summary>
		/// <remarks>
		/// @deprecated Mirror nodes can only be accessed using TLS.
		/// </remarks>
		public bool MirrorTransportSecurity
		{
			// No-op setter preserved for API compatibility
			set { /* intentionally ignored */ }
		}
		/// <summary>
		/// Is tls enabled for mirror nodes.
		/// </summary>
		public bool MirrorTransportSecurityEnabled
		{
			get => mirrorNetwork.IsTransportSecurity;
		}
		/// <summary>
		/// Is tls enabled for consensus nodes.
		/// </summary>
		public bool TransportSecurity
		{
			get => network.TransportSecurity;
			set => network.TransportSecurity = value;
		}
		/// <summary>
		/// Is certificate verification enabled.
		/// </summary>
		public bool VerifyCertificates
		{
			get => network.VerifyCertificates;
			set => network.VerifyCertificates = value;
		}

		/// <summary>
		/// Get the current default realm for new Client instances.
		/// </summary>
		/// <returns>the default realm</returns>
		public long Realm { get; private set; }
		/// <summary>
		/// Get the current default shard for new Client instances.
		/// </summary>
		/// <returns>the default shard</returns>
		public long Shard { get; private set; }

		/// <summary>
		/// Max number of attempts a request executed with this client will do.
		/// </summary>
		public int MaxAttempts
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					if (value <= 0)
					{
						throw new ArgumentException("MaxAttempts must be greater than zero");
					}

					field = value;
				}
			}
		}
		/// <summary>
		/// The minimum amount of time to wait between retries.
		/// </summary>
		public Duration MinBackoff
		{
			get => minBackoff;
			set
			{
				ArgumentNullException.ThrowIfNull(value);

				if (value.ToNanos() < 0)
				{
					throw new ArgumentException("MinBackoff must be a positive duration");
				}

				if (maxBackoff != null && value.CompareTo(maxBackoff) > 0)
				{
					throw new ArgumentException("MinBackoff must be less than or equal to MaxBackoff");
				}

				minBackoff = value;
			}
		}
		/// <summary>
		/// The maximum amount of time to wait between retries.
		/// </summary>
		public Duration MaxBackoff
		{
			get => maxBackoff;
			set
			{
				ArgumentNullException.ThrowIfNull(value);

				if (value.ToNanos() < 0)
				{
					throw new ArgumentException("MaxBackoff must be a positive duration");
				}

				if (minBackoff != null && value.CompareTo(minBackoff) < 0)
				{
					throw new ArgumentException("MaxBackoff must be greater than or equal to MinBackoff");
				}

				maxBackoff = value;
			}
		}

		/// <summary>
		/// Max number of times any node in the network can receive a bad gRPC status before being removed.
		/// </summary>
		public int MaxNodeAttempts
		{
			get
			{
				lock (this)
				{
					return network.GetMaxNodeAttempts();
				}
			}
			set
			{
				lock (this) network.MaxNodeAttempts = value;
			}
		}
		/// <summary>
		/// The minimum backoff time for any node in the network.
		/// </summary>
		public Duration NodeMinBackoff
		{
			get
			{
				lock (this)
				{
					return network.GetMinNodeBackoff();
				}
			}
			set
			{
				lock (this) network.MinNodeBackoff = value;
			}
		}
		/// <summary>
		/// The maximum backoff time for any node in the network.
		/// </summary>
		public Duration NodeMaxBackoff
		{
			get
			{
				lock (this)
				{
					return network.GetMaxNodeBackoff();
				}
			}
			set
			{
				lock (this) network.MaxNodeBackoff = value;
			}
		}
		/// <summary>
		/// Extract the minimum node readmit time.
		/// </summary>
		public Duration MinNodeReadmitTime
		{
			get => network.MinNodeReadmitTime;
			set => network.MinNodeReadmitTime = value;
		}
		/// <summary>
		/// Extract the maximum node readmit time.
		/// </summary>
		public Duration MaxNodeReadmitTime
		{
			get => network.MaxNodeReadmitTime;
			set => network.MaxNodeReadmitTime = value;
		}

		/// <summary>
		/// Enable or disable automatic entity ID checksum validation.
		/// </summary>
		public bool AutoValidateChecksums
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					field = value;
				}
			}
		}
		/// <summary>
		/// Should the transaction ID be regenerated by default.
		/// </summary>
		public bool DefaultRegenerateTransactionId
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					field = value;
				}
			}
		}

		/// <summary>
		/// Get the ID of the oper8r.
		/// </summary>
		public AccountId OperatorAccountId
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					return oper8r?.accountId;
				}
			}
		}
		/// <summary>
		/// Get the public key of the oper8r.
		/// </summary>
		public PublicKey OperatorPublicKey
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					return Oper8r.PublicKey = value;
				}
			}
		}
		/// <summary>
		/// Extract the oper8r.
		/// </summary>
		public Operator Oper8r
		{
			private set;
			get
			{
				lock (this)
				{
					return field;
				}
			}
		}

		/// <summary>
		/// The default maximum transaction fee.
		/// </summary>
		public Hbar DefaultMaxTransactionFee
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					ArgumentNullException.ThrowIfNull(value);

					if (value.ToTinybars() < 0)
					{
						throw new ArgumentException("MaxTransactionFee must be non-negative");
					}

					field = value;
				}
			}
		}
		/// <summary>
		/// The default maximum query payment.
		/// </summary>
		public Hbar DefaultMaxQueryPayment
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					ArgumentNullException.ThrowIfNull(value);

					if (value.ToTinybars() < 0)
					{
						throw new ArgumentException("DefaultMaxQueryPayment must be non-negative");
					}

					field = value;
				}
			}
		}

		/// <summary>
		/// Maximum amount of time a request can run.
		/// </summary>
		public Duration RequestTimeout
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }
		}
		/// <summary>
		/// Maximum amount of time closing a network can take.
		/// </summary>
		public Duration CloseTimeout
		{
			get => field;
			set
			{
				field = value;
				network.CloseTimeout = value;
				mirrorNetwork.CloseTimeout = value;
			}
		}
		/// <summary>
		/// Maximum amount of time a gRPC request can run.
		/// </summary>
		public Duration GrpcDeadline
		{
			get => grpcDeadline.Get();
			set => grpcDeadline.Set(ArgumentNullException.ThrowIfNull(value));
		}
		/// <summary>
		/// The period for updating the Address Book
		/// </summary>
		public Duration NetworkUpdatePeriod
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					CancelScheduledNetworkUpdate();
					field = value;
					ScheduleNetworkUpdate(networkUpdatePeriod);
				}
			}
		}

		private void ScheduleNetworkUpdate(Duration delay)
		{
			lock (this)
			{
				if (delay == null)
				{
					networkUpdateFuture = null;
					return;
				}

				networkUpdateFuture = Delayer.DelayFor(delay.ToMillis(), executor);
				networkUpdateFuture.ThenRun(() =>
				{

					// Checking networkUpdatePeriod != null must be synchronized, so I've put it in a synchronized method.
					RequireNetworkUpdatePeriodNotNull(() =>
					{
						var fileId = FileId.GetAddressBookFileIdFor(shard, realm);
						new AddressBookQuery().SetFileId(fileId).ExecuteAsync(this).ThenCompose((addressBook) => RequireNetworkUpdatePeriodNotNull(() =>
						{
							try
							{
								SetNetworkFromAddressBook(addressBook);
							}
							catch (Exception error)
							{
								return Task.FailedFuture(error);
							}

							return Task.FromResult(null);
						})).Exceptionally((error) =>
						{
							logger.Warn("Failed to update address book via mirror node query ", error);
							return null;
						});
						ScheduleNetworkUpdate(networkUpdatePeriod);
						return null;
					});
				});
			}
		}
		private void CancelAllSubscriptions()
		{
			foreach (var subscription in subscriptions)
				subscription.Unsubscribe(); 
		}
		private void CancelScheduledNetworkUpdate()
		{
			networkUpdateFuture?.Cancel(true);
		}
		private void TrackSubscription(SubscriptionHandle subscriptionHandle)
		{
			subscriptions.Add(subscriptionHandle);
		}
		private void UntrackSubscription(SubscriptionHandle subscriptionHandle)
		{
			subscriptions.Remove(subscriptionHandle);
		}
		private CompletionStage<T> RequireNetworkUpdatePeriodNotNull(Supplier<CompletionStage<T>> task)
		{
			lock (this)
			{
				return networkUpdatePeriod != null ? task.Get() : Task.FromResult(null);
			}
		}

		/// <summary>
		/// Send a ping to the given node.
		/// </summary>
		/// <param name="nodeAccountId">Account ID of the node to ping</param>
		/// <exception cref="TimeoutException">when the transaction times out</exception>
		/// <exception cref="PrecheckStatusException">when the precheck fails</exception>
		public void Ping(AccountId nodeAccountId)
        {
            Ping(nodeAccountId, RequestTimeout);
        }
        /// <summary>
        /// Send a ping to the given node.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public void Ping(AccountId nodeAccountId, Duration timeout)
        {
            new AccountBalanceQuery
			{
				AccountId = nodeAccountId,
				NodeAccountIds = [nodeAccountId],

			}.Execute(this, timeout);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public Task PingAsync(AccountId nodeAccountId)
        {
            return PingAsync(nodeAccountId, RequestTimeout);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public async Task PingAsync(AccountId nodeAccountId, Duration timeout)
        {
			await new AccountBalanceQuery()
			{
				NodeAccountIds = [nodeAccountId],

			}.ExecuteAsync(this, timeout);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAsync(AccountId nodeAccountId, Action<Exception> callback)
        {
            ActionHelper.Action(PingAsync(nodeAccountId), callback);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAsync(AccountId nodeAccountId, Duration timeout, Action<Exception> callback)
        {
            ActionHelper.Action(PingAsync(nodeAccountId, timeout), callback);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void PingAsync(AccountId nodeAccountId, Action onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(PingAsync(nodeAccountId), onSuccess, onFailure);
        }
        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void PingAsync(AccountId nodeAccountId, Duration timeout, Action onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(PingAsync(nodeAccountId, timeout), onSuccess, onFailure);
        }
        /// <summary>
        /// Sends pings to all nodes in the client's network. Combines well with setMaxAttempts(1) to remove all dead nodes
        /// from the network.
        /// </summary>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public void PingAll()
        {
            lock (this)
            {
                PingAll(RequestTimeout);
            }
        }
        /// <summary>
        /// Sends pings to all nodes in the client's network. Combines well with setMaxAttempts(1) to remove all dead nodes
        /// from the network.
        /// </summary>
        /// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public void PingAll(Duration timeoutPerPing)
        {
            lock (this)
            {
                foreach (var nodeAccountId in network.GetNetwork().Values)
                {
                    Ping(nodeAccountId, timeoutPerPing);
                }
            }
        }
        /// <summary>
        /// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the network.
        /// </summary>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public Task PingAllAsync()
        {
            lock (this)
            {
                return PingAllAsync(RequestTimeout);
            }
        }
        /// <summary>
        /// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the network.
        /// </summary>
        /// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public Task PingAllAsync(Duration timeoutPerPing)
        {
            lock (this)
            {
                var _network = network.GetNetwork();

                var list = new List<Task>(_network.Count);
                foreach (var nodeAccountId in _network.Values)
                {
                    list.Add(PingAsync(nodeAccountId, timeoutPerPing));
                }

                return Task.WhenAll(list);
            }
        }
        /// <summary>
        /// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the network.
        /// </summary>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAllAsync(Action<Exception> callback)
        {
            ActionHelper.Action(PingAllAsync(), callback);
        }
		/// <summary>
		/// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
		/// all dead nodes from the network.
		/// </summary>
		/// <param name="onSuccess">a Action which consumes the result on success.</param>
		/// <param name="onFailure">a Action which consumes the error on failure.</param>
		public void PingAllAsync(Action onSuccess, Action<Exception> onFailure)
		{
			ActionHelper.TwoActions(PingAllAsync(), onSuccess, onFailure);
		}
		/// <summary>
		/// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
		/// all dead nodes from the network.
		/// </summary>
		/// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
		/// <param name="callback">a Action which handles the result or error.</param>
		public void PingAllAsync(Duration timeoutPerPing, Action<Exception> callback)
        {
            ActionHelper.Action(PingAllAsync(timeoutPerPing), callback);
        }
        /// <summary>
        /// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the network.
        /// </summary>
        /// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void PingAllAsync(Duration timeoutPerPing, Action onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(PingAllAsync(timeoutPerPing), onSuccess, onFailure);
        }

        /// <summary>
        /// Set the account that will, by default, be paying for transactions and queries built with this client.
        /// <p>
        /// The oper8r account ID is used to generate the default transaction ID for all transactions executed with this
        /// client.
        /// <p>
        /// The oper8r private key is used to sign all transactions executed by this client.
        /// </summary>
        /// <param name="accountId">The AccountId of the oper8r</param>
        /// <param name="privateKey">The PrivateKey of the oper8r</param>
        /// <returns>{@code this}</returns>
        public Client SetOperator(AccountId accountId, PrivateKey privateKey)
        {
            lock (this)
            {
                return SetOperatorWith(accountId, privateKey.GetPublicKey(), privateKey.Sign());
            }
        }
        /// <summary>
        /// Sets the account that will, by default, by paying for transactions and queries built with this client.
        /// <p>
        /// The oper8r account ID is used to generate a default transaction ID for all transactions executed with this
        /// client.
        /// <p>
        /// The `transactionSigner` is invoked to sign all transactions executed by this client.
        /// </summary>
        /// <param name="accountId">The AccountId of the oper8r</param>
        /// <param name="publicKey">The PrivateKey of the oper8r</param>
        /// <param name="transactionSigner">The signer for the oper8r</param>
        /// <returns>{@code this}</returns>
        public Client SetOperatorWith(AccountId accountId, PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
        {
            lock (this)
            {
				try
				{
					accountId.ValidateChecksum(this);
				}
				catch (BadEntityIdException exc)
				{
					throw new ArgumentException("Tried to set the client oper8r account ID to an account ID with an invalid checksum: " + exc.Message);
				}


				Oper8r = new Operator(accountId, publicKey, transactionSigner);
                return this;
            }
        }

		/// <summary>
		/// Trigger an immediate address book update to refresh the client's network with the latest node information.
		/// This is useful when encountering INVALID_NODE_ACCOUNT_ID errors to ensure subsequent transactions
		/// use the correct node account IDs.
		/// </summary>
		/// <returns>{@code this}</returns>
		public Client UpdateNetworkFromAddressBook()
		{
			lock (this)
			{
				try
				{
					var fileId = FileId.GetAddressBookFileIdFor(Shard, Realm);
					logger.Debug("Fetching address book from file {}", fileId);

					// Execute synchronously - no async complexity
					var addressBook = new AddressBookQuery().SetFileId(fileId).Execute(this); // ← Synchronous!
					logger.Debug("Received address book with {} nodes", addressBook.NodeAddresses.Count);

					// Update the network
					NetworkFromAddressBook = addressBook;

					logger.Info("Address book update completed successfully");
				}
				catch (TimeoutException e)
				{
					logger.Warn("Failed to fetch address book: {}", e.Message);
				}
				catch (Exception e)
				{
					logger.Warn("Failed to update address book", e);
				}

				return this;
			}
		}

		/// <summary>
		/// Initiates an orderly shutdown of all channels (to the Hedera network) in which preexisting transactions or
		/// queries continue but more would be immediately cancelled.
		/// 
		/// <p>After this method returns, this client can be re-used. Channels will be re-established as
		/// needed.
		/// </summary>
		/// <exception cref="TimeoutException">if the mirror network doesn't close in time</exception>
		public void Dispose()
        {
            lock (this)
            {
                Dispose(CloseTimeout);
            }
        }
        /// <summary>
        /// Initiates an orderly shutdown of all channels (to the Hedera network) in which preexisting transactions or
        /// queries continue but more would be immediately cancelled.
        /// 
        /// <p>After this method returns, this client can be re-used. Channels will be re-established as
        /// needed.
        /// </summary>
        /// <param name="timeout">The Duration to be set</param>
        /// <exception cref="TimeoutException">if the mirror network doesn't close in time</exception>
        public void Dispose(Duration timeout)
        {
            lock (this)
            {
                var closeDeadline = Timestamp.Now().Plus(timeout);
                
                networkUpdatePeriod = null;
                CancelScheduledNetworkUpdate();
                CancelAllSubscriptions();
                network.BeginClose();
                mirrorNetwork.BeginClose();

                var networkError = network.AwaitClose(closeDeadline, null);
                var mirrorNetworkError = mirrorNetwork.AwaitClose(closeDeadline, networkError);

                // https://docs.oracle.com/javase/8/docs/api/java/util/concurrent/ExecutorService.html
                if (shouldShutdownExecutor)
                {
                    try
                    {
                        executor.Shutdown();
                        if (!executor.AwaitTermination(timeout.Seconds / 2, TimeUnit.SECONDS))
                        {
                            executor.ShutdownNow();
                            if (!executor.AwaitTermination(timeout.Seconds / 2, TimeUnit.SECONDS))
                            {
                                logger.Warn("Pool did not terminate");
                            }
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        executor.ShutdownNow();
                        
                        Thread.CurrentThread.Interrupt();
                    }
                }

                if (mirrorNetworkError != null)
                {
                    if (mirrorNetworkError is TimeoutException)
						throw mirrorNetworkError;
					else
						throw new Exception(mirrorNetworkError.Message);
				}
            }
        }
    }
}