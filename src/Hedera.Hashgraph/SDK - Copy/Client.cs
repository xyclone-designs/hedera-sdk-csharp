// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Annotations;
using Com.Google.Common.Util.Concurrent;
using Com.Google.Gson;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Logger;
using Hedera.Hashgraph.SDK.Transactions.Account;
using Java.Io;
using Java.Nio.Charset;
using Java.Nio.File;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent.Atomic;
using Java.Util.Function;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Managed client for use on the Hedera Hashgraph network.
    /// </summary>
    public sealed class Client : IDisposable
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
        Network network;
        MirrorNetwork mirrorNetwork;
        private Operator oper8r;
        private Duration requestTimeout = DEFAULT_REQUEST_TIMEOUT;
        private Duration closeTimeout = DEFAULT_CLOSE_TIMEOUT;
        private int maxAttempts = DEFAULT_MAX_ATTEMPTS;
        private volatile Duration maxBackoff = DEFAULT_MAX_BACKOFF;
        private volatile Duration minBackoff = DEFAULT_MIN_BACKOFF;
        private bool autoValidateChecksums = false;
        private bool defaultRegenerateTransactionId = true;
        private readonly bool shouldShutdownExecutor;
        private readonly long shard;
        private readonly long realm;
        // If networkUpdatePeriod is null, any network updates in progress will not complete
        private Duration networkUpdatePeriod;
        private Task networkUpdateFuture;
        private Logger logger = new Logger(LogLevel.SILENT);
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="executor">the executor</param>
        /// <param name="network">the network</param>
        /// <param name="mirrorNetwork">the mirror network</param>
        /// <param name="shouldShutdownExecutor"></param>
        Client(ExecutorService executor, Network network, MirrorNetwork mirrorNetwork, Duration networkUpdateInitialDelay, bool shouldShutdownExecutor, Duration networkUpdatePeriod, long shard, long realm)
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
        static ExecutorService CreateExecutor()
        {
            var threadFactory = new ThreadFactoryBuilder().SetNameFormat("hedera-sdk-%d").SetDaemon(true).Build();
            int nThreads = Runtime.GetRuntime().AvailableProcessors();
            return new ThreadPoolExecutor(nThreads, nThreads, 0, TimeUnit.MILLISECONDS, new LinkedBlockingQueue(), threadFactory, new CallerRunsPolicy());
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

                _ => throw new ArgumentException("Name must be one-of `mainnet`, `testnet`, or `previewnet`")};
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
        /// <param name="executor">runs the grpc requests asynchronously.</param>
        /// <returns>{@link Client}</returns>
        public static Client ForPreviewnet(ExecutorService executor)
        {
            var network = Network.ForPreviewnet(executor);
            var mirrorNetwork = MirrorNetwork.ForPreviewnet(executor);
            return new Client(executor, network, mirrorNetwork, NETWORK_UPDATE_INITIAL_DELAY, false, DEFAULT_NETWORK_UPDATE_PERIOD, 0, 0);
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

        private static Dictionary<string, AccountId> GetNetworkNodes(JsonObject networks)
        {
            Dictionary<string, AccountId> nodes = new (networks.Count);
            foreach (Map.Entry<String, JsonElement> entry in networks.EntrySet())
            {
                nodes.Put(entry.GetValue().ToString().Replace("\"", ""), AccountId.FromString(entry.GetKey().Replace("\"", "")));
            }

            return nodes;
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
        /// Extract the mirror network node list.
        /// </summary>
        /// <returns>the list of mirror nodes</returns>
        public IList<string> GetMirrorNetwork()
        {
            lock (this)
            {
                return mirrorNetwork.GetNetwork();
            }
        }

        /// <summary>
        /// Build the REST base URL for the next healthy mirror node.
        /// Returns a string like `https://host[:port]/api/v1`.
        /// If the selected mirror node is a local host (localhost/127.0.0.1) returns `http://localhost:{5551|8545}/api/v1`.
        /// </summary>
        public string GetMirrorRestBaseUrl()
        {
            try
            {
                return mirrorNetwork.GetRestBaseUrl();
            }
            catch (InterruptedException e)
            {
                Thread.CurrentThread().Interrupt();
                throw new InvalidOperationException("Interrupted while retrieving mirror base URL", e);
            }
        }

        /// <summary>
        /// Set the mirror network nodes.
        /// </summary>
        /// <param name="network">list of network nodes</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="InterruptedException">when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
        public Client SetMirrorNetwork(IList<string> network)
        {
            lock (this)
            {
                try
                {
                    mirrorNetwork.SetNetwork(network);
                }
                catch (TimeoutException e)
                {
                    throw new Exception(string.Empty, e);
                }

                return this;
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

        private CompletionStage<TWildcardTodo> RequireNetworkUpdatePeriodNotNull(Supplier<CompletionStage<TWildcardTodo>> task)
        {
            lock (this)
            {
                return networkUpdatePeriod != null ? task.Get() : Task.FromResult(null);
            }
        }

        private void CancelScheduledNetworkUpdate()
        {
            if (networkUpdateFuture != null)
            {
                networkUpdateFuture.Cancel(true);
            }
        }

        private void CancelAllSubscriptions()
        {
            subscriptions.ForEach(SubscriptionHandle.Unsubscribe());
        }

        void TrackSubscription(SubscriptionHandle subscriptionHandle)
        {
            subscriptions.Add(subscriptionHandle);
        }

        void UntrackSubscription(SubscriptionHandle subscriptionHandle)
        {
            subscriptions.Remove(subscriptionHandle);
        }

        /// <summary>
        /// Replace all nodes in this Client with the nodes in the Address Book
        /// and update the address book if necessary.
        /// </summary>
        /// <param name="addressBook">A list of nodes and their metadata</param>
        /// <returns>{@code this}</returns>
        public Client SetNetworkFromAddressBook(NodeAddressBook addressBook)
        {
            lock (this)
            {
                network.SetNetwork(Network.AddressBookToNetwork(addressBook.NodeAddresses));
                network.SetAddressBook(addressBook);
                return this;
            }
        }

        /// <summary>
        /// Extract the network.
        /// </summary>
        /// <returns>the client's network</returns>
        public Dictionary<string, AccountId> GetNetwork()
        {
            lock (this)
            {
                return network.GetNetwork();
            }
        }

        /// <summary>
        /// Replace all nodes in this Client with a new set of nodes (e.g. for an Address Book update).
        /// </summary>
        /// <param name="network">a map of node account ID to node URL.</param>
        /// <returns>{@code this} for fluent API usage.</returns>
        /// <exception cref="TimeoutException">when shutting down nodes</exception>
        /// <exception cref="InterruptedException">when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
        public Client SetNetwork(Dictionary<string, AccountId> network)
        {
            lock (this)
            {
                network.SetNetwork(network);
                return this;
            }
        }

        /// <summary>
        /// Set if transport security should be used to connect to mirror nodes.
        /// <br>
        /// If transport security is enabled all connections to mirror nodes will use TLS.
        /// </summary>
        /// <param name="transportSecurity">- enable or disable transport security for mirror nodes</param>
        /// <returns>{@code this} for fluent API usage.</returns>
        /// <remarks>@deprecatedMirror nodes can only be accessed using TLS</remarks>
        public Client SetMirrorTransportSecurity(bool transportSecurity)
        {
            return this;
        }

        /// <summary>
        /// Is tls enabled for consensus nodes.
        /// </summary>
        /// <returns>is tls enabled</returns>
        public bool IsTransportSecurity()
        {
            return network.IsTransportSecurity();
        }

        /// <summary>
        /// Set if transport security should be used to connect to consensus nodes.
        /// <br>
        /// If transport security is enabled all connections to consensus nodes will use TLS, and the server's certificate
        /// hash will be compared to the hash stored in the {@link NodeAddressBook} for the given network.
        /// <br>
        /// *Note*: If transport security is enabled, but {@link Client#isVerifyCertificates()} is disabled then server
        /// certificates will not be verified.
        /// </summary>
        /// <param name="transportSecurity">enable or disable transport security for consensus nodes</param>
        /// <returns>{@code this} for fluent API usage.</returns>
        /// <exception cref="InterruptedException">when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
        public Client SetTransportSecurity(bool transportSecurity)
        {
            network.SetTransportSecurity(transportSecurity);
            return this;
        }

        /// <summary>
        /// Is tls enabled for mirror nodes.
        /// </summary>
        /// <returns>is tls enabled</returns>
        public bool MirrorIsTransportSecurity()
        {
            return mirrorNetwork.IsTransportSecurity();
        }

        /// <summary>
        /// Is certificate verification enabled.
        /// </summary>
        /// <returns>is certificate verification enabled</returns>
        public bool IsVerifyCertificates()
        {
            return network.IsVerifyCertificates();
        }

        /// <summary>
        /// Set if server certificates should be verified against an existing address book
        /// </summary>
        /// <param name="verifyCertificates">- enable or disable certificate verification</param>
        /// <returns>{@code this}</returns>
        public Client SetVerifyCertificates(bool verifyCertificates)
        {
            network.SetVerifyCertificates(verifyCertificates);
            return this;
        }

        /// <summary>
        /// Send a ping to the given node.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public Void Ping(AccountId nodeAccountId)
        {
            return Ping(nodeAccountId, GetRequestTimeout());
        }

        /// <summary>
        /// Send a ping to the given node.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public Void Ping(AccountId nodeAccountId, Duration timeout)
        {
            new AccountBalanceQuery().SetAccountId(nodeAccountId).SetNodeAccountIds(Collections.SingletonList(nodeAccountId)).Execute(this, timeout);
            return null;
        }

        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public Task PingAsync(AccountId nodeAccountId)
        {
            return PingAsync(nodeAccountId, GetRequestTimeout());
        }

        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>an empty future that throws exception if there was an error</returns>
        public Task PingAsync(AccountId nodeAccountId, Duration timeout)
        {
            var result = new Task();
            new AccountBalanceQuery().SetAccountId(nodeAccountId).SetNodeAccountIds(Collections.SingletonList(nodeAccountId)).ExecuteAsync(this, timeout).WhenComplete((balance, error) =>
            {
                if (error == null)
                {
                    result.Complete(null);
                }
                else
                {
                    result.CompleteExceptionally(error);
                }
            });
            return result;
        }

        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAsync(AccountId nodeAccountId, Action<Void, Exception> callback)
        {
            ActionHelper.Action(PingAsync(nodeAccountId), callback);
        }

        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAsync(AccountId nodeAccountId, Duration timeout, Action<Void, Exception> callback)
        {
            ActionHelper.Action(PingAsync(nodeAccountId, timeout), callback);
        }

        /// <summary>
        /// Send a ping to the given node asynchronously.
        /// </summary>
        /// <param name="nodeAccountId">Account ID of the node to ping</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void PingAsync(AccountId nodeAccountId, Action<Void> onSuccess, Action<Exception> onFailure)
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
        public void PingAsync(AccountId nodeAccountId, Duration timeout, Action<Void> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(PingAsync(nodeAccountId, timeout), onSuccess, onFailure);
        }

        /// <summary>
        /// Sends pings to all nodes in the client's network. Combines well with setMaxAttempts(1) to remove all dead nodes
        /// from the network.
        /// </summary>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public Void PingAll()
        {
            lock (this)
            {
                return PingAll(GetRequestTimeout());
            }
        }

        /// <summary>
        /// Sends pings to all nodes in the client's network. Combines well with setMaxAttempts(1) to remove all dead nodes
        /// from the network.
        /// </summary>
        /// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public Void PingAll(Duration timeoutPerPing)
        {
            lock (this)
            {
                foreach (var nodeAccountId in network.GetNetwork().Values())
                {
                    Ping(nodeAccountId, timeoutPerPing);
                }

                return null;
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
                return PingAllAsync(GetRequestTimeout());
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
                var network = network.GetNetwork();
                var list = new List<Task>(network.Count);
                foreach (var nodeAccountId in network.Values())
                {
                    list.Add(PingAsync(nodeAccountId, timeoutPerPing));
                }

                return Task.AllOf(list.ToArray(new Task<TWildcardTodo>[0])).ThenApply((v) => null);
            }
        }

        /// <summary>
        /// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the network.
        /// </summary>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAllAsync(Action<Void, Exception> callback)
        {
            ActionHelper.Action(PingAllAsync(), callback);
        }

        /// <summary>
        /// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the network.
        /// </summary>
        /// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public void PingAllAsync(Duration timeoutPerPing, Action<Void, Exception> callback)
        {
            ActionHelper.Action(PingAllAsync(timeoutPerPing), callback);
        }

        /// <summary>
        /// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the network.
        /// </summary>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void PingAllAsync(Action<Void> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(PingAllAsync(), onSuccess, onFailure);
        }

        /// <summary>
        /// Sends pings to all nodes in the client's network asynchronously. Combines well with setMaxAttempts(1) to remove
        /// all dead nodes from the network.
        /// </summary>
        /// <param name="timeoutPerPing">The timeout after which each execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public void PingAllAsync(Duration timeoutPerPing, Action<Void> onSuccess, Action<Exception> onFailure)
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
                if (GetNetworkName() != null)
                {
                    try
                    {
                        accountId.ValidateChecksum(this);
                    }
                    catch (BadEntityIdException exc)
                    {
                        throw new ArgumentException("Tried to set the client oper8r account ID to an account ID with an invalid checksum: " + exc.GetMessage());
                    }
                }

                oper8r = new Operator(accountId, publicKey, transactionSigner);
                return this;
            }
        }

        /// <summary>
        /// Current name of the network; corresponds to ledger ID in entity ID checksum calculations.
        /// </summary>
        /// <returns>the network name</returns>
        /// <remarks>@deprecateduse {@link #getLedgerId()} instead</remarks>
        public NetworkName GetNetworkName()
        {
            lock (this)
            {
                var ledgerId = network.GetLedgerId();
                return ledgerId == null ? null : ledgerId.ToNetworkName();
            }
        }

        /// <summary>
        /// Set the network name to a particular value. Useful when constructing a network which is a subset of an existing
        /// known network.
        /// </summary>
        /// <param name="networkName">the desired network</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecateduse {@link #setLedgerId(LedgerId)} instead</remarks>
        public Client SetNetworkName(NetworkName networkName)
        {
            lock (this)
            {
                network.SetLedgerId(networkName == null ? null : LedgerId.FromNetworkName(networkName));
                return this;
            }
        }

        /// <summary>
        /// Current LedgerId of the network; corresponds to ledger ID in entity ID checksum calculations.
        /// </summary>
        /// <returns>the ledger id</returns>
        public LedgerId GetLedgerId()
        {
            lock (this)
            {
                return network.GetLedgerId();
            }
        }

        /// <summary>
        /// Set the LedgerId to a particular value. Useful when constructing a network which is a subset of an existing known
        /// network.
        /// </summary>
        /// <param name="ledgerId">the desired ledger id</param>
        /// <returns>{@code this}</returns>
        public Client SetLedgerId(LedgerId ledgerId)
        {
            lock (this)
            {
                network.SetLedgerId(ledgerId);
                return this;
            }
        }

        /// <summary>
        /// Max number of attempts a request executed with this client will do.
        /// </summary>
        /// <returns>the maximus attempts</returns>
        public int GetMaxAttempts()
        {
            lock (this)
            {
                return maxAttempts;
            }
        }

        /// <summary>
        /// Set the max number of attempts a request executed with this client will do.
        /// </summary>
        /// <param name="maxAttempts">the desired max attempts</param>
        /// <returns>{@code this}</returns>
        public Client SetMaxAttempts(int maxAttempts)
        {
            lock (this)
            {
                if (maxAttempts <= 0)
                {
                    throw new ArgumentException("maxAttempts must be greater than zero");
                }

                maxAttempts = maxAttempts;
                return this;
            }
        }

        /// <summary>
        /// The maximum amount of time to wait between retries
        /// </summary>
        /// <returns>maxBackoff</returns>
        public Duration GetMaxBackoff()
        {
            return maxBackoff;
        }

        /// <summary>
        /// The maximum amount of time to wait between retries. Every retry attempt will increase the wait time exponentially
        /// until it reaches this time.
        /// </summary>
        /// <param name="maxBackoff">The maximum amount of time to wait between retries</param>
        /// <returns>{@code this}</returns>
        public Client SetMaxBackoff(Duration maxBackoff)
        {
            if (maxBackoff == null || maxBackoff.ToNanos() < 0)
            {
                throw new ArgumentException("maxBackoff must be a positive duration");
            }
            else if (maxBackoff.CompareTo(minBackoff) < 0)
            {
                throw new ArgumentException("maxBackoff must be greater than or equal to minBackoff");
            }

            maxBackoff = maxBackoff;
            return this;
        }

        /// <summary>
        /// The minimum amount of time to wait between retries
        /// </summary>
        /// <returns>minBackoff</returns>
        public Duration GetMinBackoff()
        {
            return minBackoff;
        }

        /// <summary>
        /// The minimum amount of time to wait between retries. When retrying, the delay will start at this time and increase
        /// exponentially until it reaches the maxBackoff.
        /// </summary>
        /// <param name="minBackoff">The minimum amount of time to wait between retries</param>
        /// <returns>{@code this}</returns>
        public Client SetMinBackoff(Duration minBackoff)
        {
            if (minBackoff == null || minBackoff.ToNanos() < 0)
            {
                throw new ArgumentException("minBackoff must be a positive duration");
            }
            else if (minBackoff.CompareTo(maxBackoff) > 0)
            {
                throw new ArgumentException("minBackoff must be less than or equal to maxBackoff");
            }

            minBackoff = minBackoff;
            return this;
        }

        /// <summary>
        /// Max number of times any node in the network can receive a bad gRPC status before being removed from the network.
        /// </summary>
        /// <returns>the maximum node attempts</returns>
        public int GetMaxNodeAttempts()
        {
            lock (this)
            {
                return network.GetMaxNodeAttempts();
            }
        }

        /// <summary>
        /// Set the max number of times any node in the network can receive a bad gRPC status before being removed from the
        /// network.
        /// </summary>
        /// <param name="maxNodeAttempts">the desired minimum attempts</param>
        /// <returns>{@code this}</returns>
        public Client SetMaxNodeAttempts(int maxNodeAttempts)
        {
            lock (this)
            {
                network.SetMaxNodeAttempts(maxNodeAttempts);
                return this;
            }
        }

        /// <summary>
        /// The minimum backoff time for any node in the network.
        /// </summary>
        /// <returns>the wait time</returns>
        /// <remarks>@deprecated- Use {@link Client#getNodeMaxBackoff()} instead</remarks>
        public Duration GetNodeWaitTime()
        {
            lock (this)
            {
                return GetNodeMinBackoff();
            }
        }

        /// <summary>
        /// Set the minimum backoff time for any node in the network.
        /// </summary>
        /// <param name="nodeWaitTime">the wait time</param>
        /// <returns>the updated client</returns>
        /// <remarks>@deprecated- Use {@link Client#setNodeMinBackoff(Duration)} ()} instead</remarks>
        public Client SetNodeWaitTime(Duration nodeWaitTime)
        {
            lock (this)
            {
                return SetNodeMinBackoff(nodeWaitTime);
            }
        }

        /// <summary>
        /// The minimum backoff time for any node in the network.
        /// </summary>
        /// <returns>the minimum backoff time</returns>
        public Duration GetNodeMinBackoff()
        {
            lock (this)
            {
                return network.GetMinNodeBackoff();
            }
        }

        /// <summary>
        /// Set the minimum backoff time for any node in the network.
        /// </summary>
        /// <param name="minBackoff">the desired minimum backoff time</param>
        /// <returns>{@code this}</returns>
        public Client SetNodeMinBackoff(Duration minBackoff)
        {
            lock (this)
            {
                network.SetMinNodeBackoff(minBackoff);
                return this;
            }
        }

        /// <summary>
        /// The maximum backoff time for any node in the network.
        /// </summary>
        /// <returns>the maximum node backoff time</returns>
        public Duration GetNodeMaxBackoff()
        {
            lock (this)
            {
                return network.GetMaxNodeBackoff();
            }
        }

        /// <summary>
        /// Set the maximum backoff time for any node in the network.
        /// </summary>
        /// <param name="maxBackoff">the desired max backoff time</param>
        /// <returns>{@code this}</returns>
        public Client SetNodeMaxBackoff(Duration maxBackoff)
        {
            lock (this)
            {
                network.SetMaxNodeBackoff(maxBackoff);
                return this;
            }
        }

        /// <summary>
        /// Extract the minimum node readmit time.
        /// </summary>
        /// <returns>the minimum node readmit time</returns>
        public Duration GetMinNodeReadmitTime()
        {
            return network.GetMinNodeReadmitTime();
        }

        /// <summary>
        /// Assign the minimum node readmit time.
        /// </summary>
        /// <param name="minNodeReadmitTime">the requested duration</param>
        /// <returns>{@code this}</returns>
        public Client SetMinNodeReadmitTime(Duration minNodeReadmitTime)
        {
            network.SetMinNodeReadmitTime(minNodeReadmitTime);
            return this;
        }

        /// <summary>
        /// Extract the node readmit time.
        /// </summary>
        /// <returns>the maximum node readmit time</returns>
        public Duration GetMaxNodeReadmitTime()
        {
            return network.GetMaxNodeReadmitTime();
        }

        /// <summary>
        /// Assign the maximum node readmit time.
        /// </summary>
        /// <param name="maxNodeReadmitTime">the maximum node readmit time</param>
        /// <returns>{@code this}</returns>
        public Client SetMaxNodeReadmitTime(Duration maxNodeReadmitTime)
        {
            network.SetMaxNodeReadmitTime(maxNodeReadmitTime);
            return this;
        }

        /// <summary>
        /// Set the max amount of nodes that will be chosen per request. By default, the request will use 1/3rd the network
        /// nodes per request.
        /// </summary>
        /// <param name="maxNodesPerTransaction">the desired number of nodes</param>
        /// <returns>{@code this}</returns>
        public Client SetMaxNodesPerTransaction(int maxNodesPerTransaction)
        {
            lock (this)
            {
                network.SetMaxNodesPerRequest(maxNodesPerTransaction);
                return this;
            }
        }

        /// <summary>
        /// Enable or disable automatic entity ID checksum validation.
        /// </summary>
        /// <param name="value">the desired value</param>
        /// <returns>{@code this}</returns>
        public Client SetAutoValidateChecksums(bool value)
        {
            lock (this)
            {
                autoValidateChecksums = value;
                return this;
            }
        }

        /// <summary>
        /// Is automatic entity ID checksum validation enabled.
        /// </summary>
        /// <returns>is validation enabled</returns>
        public bool IsAutoValidateChecksumsEnabled()
        {
            lock (this)
            {
                return autoValidateChecksums;
            }
        }

        /// <summary>
        /// Get the ID of the oper8r. Useful when the client was constructed from file.
        /// </summary>
        /// <returns>{AccountId}</returns>
        public AccountId GetOperatorAccountId()
        {
            lock (this)
            {
                if (oper8r == null)
                {
                    return null;
                }

                return oper8r.accountId;
            }
        }

        /// <summary>
        /// Get the key of the oper8r. Useful when the client was constructed from file.
        /// </summary>
        /// <returns>{PublicKey}</returns>
        public PublicKey GetOperatorPublicKey()
        {
            lock (this)
            {
                if (oper8r == null)
                {
                    return null;
                }

                return oper8r.publicKey;
            }
        }

        /// <summary>
        /// The default maximum fee used for transactions.
        /// </summary>
        /// <returns>the max transaction fee</returns>
        public Hbar GetDefaultMaxTransactionFee()
        {
            lock (this)
            {
                return defaultMaxTransactionFee;
            }
        }

        /// <summary>
        /// Set the maximum fee to be paid for transactions executed by this client.
        /// <p>
        /// Because transaction fees are always maximums, this will simply add a call to
        /// {@link Transaction#setMaxTransactionFee(Hbar)} on every new transaction. The actual fee assessed for a given
        /// transaction may be less than this value, but never greater.
        /// </summary>
        /// <param name="defaultMaxTransactionFee">The Hbar to be set</param>
        /// <returns>{@code this}</returns>
        public Client SetDefaultMaxTransactionFee(Hbar defaultMaxTransactionFee)
        {
            lock (this)
            {
                Objects.RequireNonNull(defaultMaxTransactionFee);
                if (defaultMaxTransactionFee.ToTinybars() < 0)
                {
                    throw new ArgumentException("maxTransactionFee must be non-negative");
                }

                defaultMaxTransactionFee = defaultMaxTransactionFee;
                return this;
            }
        }

        /// <summary>
        /// Set the maximum fee to be paid for transactions executed by this client.
        /// <p>
        /// Because transaction fees are always maximums, this will simply add a call to
        /// {@link Transaction#setMaxTransactionFee(Hbar)} on every new transaction. The actual fee assessed for a given
        /// transaction may be less than this value, but never greater.
        /// </summary>
        /// <param name="maxTransactionFee">The Hbar to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedUse {@link #setDefaultMaxTransactionFee(Hbar)} instead.</remarks>
        public Client SetMaxTransactionFee(Hbar maxTransactionFee)
        {
            lock (this)
            {
                return SetDefaultMaxTransactionFee(maxTransactionFee);
            }
        }

        /// <summary>
        /// Extract the maximum query payment.
        /// </summary>
        /// <returns>the default maximum query payment</returns>
        public Hbar GetDefaultMaxQueryPayment()
        {
            lock (this)
            {
                return defaultMaxQueryPayment;
            }
        }

        /// <summary>
        /// Set the maximum default payment allowable for queries.
        /// <p>
        /// When a query is executed without an explicit {@link Query#setQueryPayment(Hbar)} call, the client will first
        /// request the cost of the given query from the node it will be submitted to and attach a payment for that amount
        /// from the oper8r account on the client.
        /// <p>
        /// If the returned value is greater than this value, a {@link MaxQueryPaymentExceededException} will be thrown from
        /// {@link Query#execute(Client)} or returned in the second callback of
        /// {@link Query#executeAsync(Client, Action, Action)}.
        /// <p>
        /// Set to 0 to disable automatic implicit payments.
        /// </summary>
        /// <param name="defaultMaxQueryPayment">The Hbar to be set</param>
        /// <returns>{@code this}</returns>
        public Client SetDefaultMaxQueryPayment(Hbar defaultMaxQueryPayment)
        {
            lock (this)
            {
                Objects.RequireNonNull(defaultMaxQueryPayment);
                if (defaultMaxQueryPayment.ToTinybars() < 0)
                {
                    throw new ArgumentException("defaultMaxQueryPayment must be non-negative");
                }

                defaultMaxQueryPayment = defaultMaxQueryPayment;
                return this;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="maxQueryPayment">The Hbar to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedUse {@link #setDefaultMaxQueryPayment(Hbar)} instead.</remarks>
        public Client SetMaxQueryPayment(Hbar maxQueryPayment)
        {
            lock (this)
            {
                return SetDefaultMaxQueryPayment(maxQueryPayment);
            }
        }

        /// <summary>
        /// Should the transaction id be regenerated?
        /// </summary>
        /// <returns>the default regenerate transaction id</returns>
        public bool GetDefaultRegenerateTransactionId()
        {
            lock (this)
            {
                return defaultRegenerateTransactionId;
            }
        }

        /// <summary>
        /// Assign the default regenerate transaction id.
        /// </summary>
        /// <param name="regenerateTransactionId">should there be a regenerated transaction id</param>
        /// <returns>{@code this}</returns>
        public Client SetDefaultRegenerateTransactionId(bool regenerateTransactionId)
        {
            lock (this)
            {
                defaultRegenerateTransactionId = regenerateTransactionId;
                return this;
            }
        }

        /// <summary>
        /// Maximum amount of time a request can run
        /// </summary>
        /// <returns>the timeout value</returns>
        public Duration GetRequestTimeout()
        {
            lock (this)
            {
                return requestTimeout;
            }
        }

        /// <summary>
        /// Set the maximum amount of time a request can run. Used only in async variants of methods.
        /// </summary>
        /// <param name="requestTimeout">the timeout value</param>
        /// <returns>{@code this}</returns>
        public Client SetRequestTimeout(Duration requestTimeout)
        {
            lock (this)
            {
                requestTimeout = Objects.RequireNonNull(requestTimeout);
                return this;
            }
        }

        /// <summary>
        /// Maximum amount of time closing a network can take.
        /// </summary>
        /// <returns>the timeout value</returns>
        public Duration GetCloseTimeout()
        {
            return closeTimeout;
        }

        /// <summary>
        /// Set the maximum amount of time closing a network can take.
        /// </summary>
        /// <param name="closeTimeout">the timeout value</param>
        /// <returns>{@code this}</returns>
        public Client SetCloseTimeout(Duration closeTimeout)
        {
            closeTimeout = Objects.RequireNonNull(closeTimeout);
            network.SetCloseTimeout(closeTimeout);
            mirrorNetwork.SetCloseTimeout(closeTimeout);
            return this;
        }

        /// <summary>
        /// Maximum amount of time a gRPC request can run
        /// </summary>
        /// <returns>the gRPC deadline value</returns>
        public Duration GetGrpcDeadline()
        {
            return grpcDeadline.Get();
        }

        /// <summary>
        /// Set the maximum amount of time a gRPC request can run.
        /// </summary>
        /// <param name="grpcDeadline">the gRPC deadline value</param>
        /// <returns>{@code this}</returns>
        public Client SetGrpcDeadline(Duration grpcDeadline)
        {
            grpcDeadline.Set(Objects.RequireNonNull(grpcDeadline));
            return this;
        }

        /// <summary>
        /// Extract the oper8r.
        /// </summary>
        /// <returns>the oper8r</returns>
        Operator GetOperator()
        {
            lock (this)
            {
                return oper8r;
            }
        }

        /// <summary>
        /// Get the period for updating the Address Book
        /// </summary>
        /// <returns>the networkUpdatePeriod</returns>
        public Duration GetNetworkUpdatePeriod()
        {
            lock (this)
            {
                return networkUpdatePeriod;
            }
        }

        /// <summary>
        /// Set the period for updating the Address Book
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
        /// because it uses features introduced in API level 31 (Android 12).</p>*
        /// </summary>
        /// <param name="networkUpdatePeriod">the period for updating the Address Book</param>
        /// <returns>{@code this}</returns>
        public Client SetNetworkUpdatePeriod(Duration networkUpdatePeriod)
        {
            lock (this)
            {
                CancelScheduledNetworkUpdate();
                networkUpdatePeriod = networkUpdatePeriod;
                ScheduleNetworkUpdate(networkUpdatePeriod);
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
                    var fileId = FileId.GetAddressBookFileIdFor(shard, realm);
                    logger.Debug("Fetching address book from file {}", fileId);

                    // Execute synchronously - no async complexity
                    var addressBook = new AddressBookQuery().SetFileId(fileId).Execute(this); // ← Synchronous!
                    logger.Debug("Received address book with {} nodes", addressBook.nodeAddresses.Count);

                    // Update the network
                    SetNetworkFromAddressBook(addressBook);
                    logger.Info("Address book update completed successfully");
                }
                catch (TimeoutException e)
                {
                    logger.Warn("Failed to fetch address book: {}", e.GetMessage());
                }
                catch (Exception e)
                {
                    logger.Warn("Failed to update address book", e);
                }

                return this;
            }
        }

        public Logger GetLogger()
        {
            return logger;
        }

        public Client SetLogger(Logger logger)
        {
            logger = logger;
            return this;
        }

        /// <summary>
        /// Get the current default realm for new Client instances.
        /// </summary>
        /// <returns>the default realm</returns>
        public long GetRealm()
        {
            return realm;
        }

        /// <summary>
        /// Get the current default shard for new Client instances.
        /// </summary>
        /// <returns>the default shard</returns>
        public long GetShard()
        {
            return shard;
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
                Dispose(closeTimeout);
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
                        if (!executor.AwaitTermination(timeout.GetSeconds() / 2, TimeUnit.SECONDS))
                        {
                            executor.ShutdownNow();
                            if (!executor.AwaitTermination(timeout.GetSeconds() / 2, TimeUnit.SECONDS))
                            {
                                logger.Warn("Pool did not terminate");
                            }
                        }
                    }
                    catch (InterruptedException ex)
                    {
                        executor.ShutdownNow();
                        Thread.CurrentThread().Interrupt();
                    }
                }

                if (mirrorNetworkError != null)
                {
                    if (mirrorNetworkError is TimeoutException)
                    {
                        throw ex;
                    }
                    else
                    {
                        throw new Exception(mirrorNetworkError);
                    }
                }
            }
        }
    }
}