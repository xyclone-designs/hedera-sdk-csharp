// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.FutureConverter;
using Com.Google.Common.Annotations;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Logger;
using Io.Grpc;
using Io.Grpc.Status;
using Io.Grpc.Stub;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Java.Util.Regex;
using Javax.Annotation;
using Org.Bouncycastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Abstract base utility class.
    /// </summary>
    /// <param name="<SdkRequestT>">the sdk request</param>
    /// <param name="<ProtoRequestT>">the proto request</param>
    /// <param name="<ResponseT>">the response</param>
    /// <param name="<O>">the O type</param>
    abstract class Executable<SdkRequestT, ProtoRequestT, ResponseT, O> where ProtoRequestT : MessageLite where ResponseT : MessageLite
    {
        protected static readonly Random random = new Random();
        static readonly Pattern RST_STREAM = Pattern.Compile(".*\\brst[^0-9a-zA-Z]stream\\b.*", Pattern.CASE_INSENSITIVE | Pattern.DOTALL);
        /// <summary>
        /// The maximum times execution will be attempted
        /// </summary>
        protected int maxAttempts = null;
        /// <summary>
        /// The maximum amount of time to wait between retries
        /// </summary>
        protected Duration maxBackoff = null;
        /// <summary>
        /// The minimum amount of time to wait between retries
        /// </summary>
        protected Duration minBackoff = null;
        /// <summary>
        /// List of account IDs for nodes with which execution will be attempted.
        /// </summary>
        protected LockableList<AccountId> nodeAccountIds = new LockableList();
        /// <summary>
        /// List of healthy and unhealthy nodes with which execution will be attempted.
        /// </summary>
        protected LockableList<Node> nodes = new LockableList();
        /// <summary>
        /// Indicates if the request has been attempted to be sent to all nodes
        /// </summary>
        protected bool attemptedAllNodes = false;
        /// <summary>
        /// The timeout for each execution attempt
        /// </summary>
        protected Duration grpcDeadline;
        protected Logger logger;
        private java.util.function.Function<ProtoRequestT, ProtoRequestT> requestListener;
        // Lambda responsible for executing synchronous gRPC requests. Pluggable for unit testing.
        Function<GrpcRequest, ResponseT> blockingUnaryCall = (grpcRequest) => ClientCalls.BlockingUnaryCall(grpcRequest.CreateCall(), grpcRequest.GetRequest());
        private java.util.function.Function<ResponseT, ResponseT> responseListener;
        Executable()
        {
            requestListener = (request) =>
            {
                if (logger.IsEnabledForLevel(LogLevel.TRACE))
                {
                    logger.Trace("Sent protobuf {}", Hex.ToHexString(request.ToByteArray()));
                }

                return request;
            };
            responseListener = (response) =>
            {
                if (logger.IsEnabledForLevel(LogLevel.TRACE))
                {
                    logger.Trace("Received protobuf {}", Hex.ToHexString(response.ToByteArray()));
                }

                return response;
            };
        }

        /// <summary>
        /// When execution is attempted, a single attempt will time out when this deadline is reached. (The SDK may
        /// subsequently retry the execution.)
        /// </summary>
        /// <returns>The timeout for each execution attempt</returns>
        public Duration GrpcDeadline()
        {
            return grpcDeadline;
        }

        /// <summary>
        /// When execution is attempted, a single attempt will timeout when this deadline is reached. (The SDK may
        /// subsequently retry the execution.)
        /// </summary>
        /// <param name="grpcDeadline">The timeout for each execution attempt</param>
        /// <returns>{@code this}</returns>
        public SdkRequestT SetGrpcDeadline(Duration grpcDeadline)
        {
            grpcDeadline = Objects.RequireNonNull(grpcDeadline);

            // noinspection unchecked
            return (SdkRequestT)this;
        }

        /// <summary>
        /// The maximum amount of time to wait between retries
        /// </summary>
        /// <returns>maxBackoff</returns>
        public Duration GetMaxBackoff()
        {
            return maxBackoff != null ? maxBackoff : Client.DEFAULT_MAX_BACKOFF;
        }

        /// <summary>
        /// The maximum amount of time to wait between retries. Every retry attempt will increase the wait time exponentially
        /// until it reaches this time.
        /// </summary>
        /// <param name="maxBackoff">The maximum amount of time to wait between retries</param>
        /// <returns>{@code this}</returns>
        public SdkRequestT SetMaxBackoff(Duration maxBackoff)
        {
            if (maxBackoff == null || maxBackoff.ToNanos() < 0)
            {
                throw new ArgumentException("maxBackoff must be a positive duration");
            }
            else if (maxBackoff.CompareTo(GetMinBackoff()) < 0)
            {
                throw new ArgumentException("maxBackoff must be greater than or equal to minBackoff");
            }

            maxBackoff = maxBackoff;

            // noinspection unchecked
            return (SdkRequestT)this;
        }

        /// <summary>
        /// The minimum amount of time to wait between retries
        /// </summary>
        /// <returns>minBackoff</returns>
        public Duration GetMinBackoff()
        {
            return minBackoff != null ? minBackoff : Client.DEFAULT_MIN_BACKOFF;
        }

        /// <summary>
        /// The minimum amount of time to wait between retries. When retrying, the delay will start at this time and increase
        /// exponentially until it reaches the maxBackoff.
        /// </summary>
        /// <param name="minBackoff">The minimum amount of time to wait between retries</param>
        /// <returns>{@code this}</returns>
        public SdkRequestT SetMinBackoff(Duration minBackoff)
        {
            if (minBackoff == null || minBackoff.ToNanos() < 0)
            {
                throw new ArgumentException("minBackoff must be a positive duration");
            }
            else if (minBackoff.CompareTo(GetMaxBackoff()) > 0)
            {
                throw new ArgumentException("minBackoff must be less than or equal to maxBackoff");
            }

            minBackoff = minBackoff;

            // noinspection unchecked
            return (SdkRequestT)this;
        }

        /// <summary>
        /// </summary>
        /// <returns>Number of errors before execution will fail.</returns>
        /// <remarks>@deprecatedUse {@link #getMaxAttempts()} instead.</remarks>
        public int GetMaxRetry()
        {
            return GetMaxAttempts();
        }

        /// <summary>
        /// </summary>
        /// <param name="count">Number of errors before execution will fail</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedUse {@link #setMaxAttempts(int)} instead.</remarks>
        public SdkRequestT SetMaxRetry(int count)
        {
            return SetMaxAttempts(count);
        }

        /// <summary>
        /// Get the maximum times execution will be attempted.
        /// </summary>
        /// <returns>Number of errors before execution will fail.</returns>
        public int GetMaxAttempts()
        {
            return maxAttempts != null ? maxAttempts : Client.DEFAULT_MAX_ATTEMPTS;
        }

        /// <summary>
        /// Set the maximum times execution will be attempted.
        /// </summary>
        /// <param name="maxAttempts">Execution will fail after this many errors.</param>
        /// <returns>{@code this}</returns>
        public SdkRequestT SetMaxAttempts(int maxAttempts)
        {
            if (maxAttempts <= 0)
            {
                throw new ArgumentException("maxAttempts must be greater than zero");
            }

            maxAttempts = maxAttempts;

            // noinspection unchecked
            return (SdkRequestT)this;
        }

        /// <summary>
        /// Get the list of account IDs for nodes with which execution will be attempted.
        /// </summary>
        /// <returns>the list of account IDs</returns>
        public IList<AccountId> GetNodeAccountIds()
        {
            if (!nodeAccountIds.IsEmpty())
            {
                return new List(nodeAccountIds.GetList());
            }

            return null;
        }

        /// <summary>
        /// Set the account IDs of the nodes that this transaction will be submitted to.
        /// <p>
        /// Providing an explicit node account ID interferes with client-side load balancing of the network. By default, the
        /// SDK will pre-generate a transaction for 1/3 of the nodes on the network. If a node is down, busy, or otherwise
        /// reports a fatal error, the SDK will try again with a different node.
        /// </summary>
        /// <param name="nodeAccountIds">The list of node AccountIds to be set</param>
        /// <returns>{@code this}</returns>
        public virtual SdkRequestT SetNodeAccountIds(IList<AccountId> nodeAccountIds)
        {
            nodeAccountIds.SetList(nodeAccountIds).SetLocked(true);

            // noinspection unchecked
            return (SdkRequestT)this;
        }

        /// <summary>
        /// Set a callback that will be called right before the request is sent. As input, the callback will receive the
        /// protobuf of the request, and the callback should return the request protobuf.  This means the callback has an
        /// opportunity to read, copy, or modify the request that will be sent.
        /// </summary>
        /// <param name="requestListener">The callback to use</param>
        /// <returns>{@code this}</returns>
        public SdkRequestT SetRequestListener(UnaryOperator<ProtoRequestT> requestListener)
        {
            requestListener = Objects.RequireNonNull(requestListener);
            return (SdkRequestT)this;
        }

        /// <summary>
        /// Set a callback that will be called right before the response is returned. As input, the callback will receive the
        /// protobuf of the response, and the callback should return the response protobuf.  This means the callback has an
        /// opportunity to read, copy, or modify the response that will be read.
        /// </summary>
        /// <param name="responseListener">The callback to use</param>
        /// <returns>{@code this}</returns>
        public SdkRequestT SetResponseListener(UnaryOperator<ResponseT> responseListener)
        {
            responseListener = Objects.RequireNonNull(responseListener);
            return (SdkRequestT)this;
        }

        /// <summary>
        /// Set the logger
        /// </summary>
        /// <param name="logger">the new logger</param>
        /// <returns>{@code this}</returns>
        public virtual SdkRequestT SetLogger(Logger logger)
        {
            logger = logger;
            return (SdkRequestT)this;
        }

        virtual void CheckNodeAccountIds()
        {
            if (nodeAccountIds.IsEmpty())
            {
                throw new InvalidOperationException("Request node account IDs were not set before executing");
            }
        }

        abstract void OnExecute(Client client);
        abstract CompletableFuture<Void> OnExecuteAsync(Client client);
        virtual void MergeFromClient(Client client)
        {
            if (maxAttempts == null)
            {
                maxAttempts = client.GetMaxAttempts();
            }

            if (maxBackoff == null)
            {
                maxBackoff = client.GetMaxBackoff();
            }

            if (minBackoff == null)
            {
                minBackoff = client.GetMinBackoff();
            }

            if (grpcDeadline == null)
            {
                grpcDeadline = client.GetGrpcDeadline();
            }
        }

        private void Delay(long delay)
        {
            if (delay <= 0)
            {
                return;
            }

            try
            {
                if (delay > 0)
                {
                    if (logger.IsEnabledForLevel(LogLevel.DEBUG))
                    {
                        logger.Debug("Sleeping for: " + delay + " | Thread name: " + Thread.CurrentThread().GetName());
                    }

                    Thread.Sleep(delay);
                }
            }
            catch (InterruptedException e)
            {
                throw new Exception(e);
            }
        }

        /// <summary>
        /// Updates the client's network from the address book if a mirror network is configured.
        /// Logs any errors at TRACE level without throwing exceptions.
        /// </summary>
        /// <param name="client">The client whose network should be updated</param>
        private void UpdateNetworkFromAddressBook(Client client)
        {
            try
            {
                if (client.GetMirrorNetwork() != null && !client.GetMirrorNetwork().IsEmpty())
                {
                    client.UpdateNetworkFromAddressBook();
                }
            }
            catch (Exception updateError)
            {
                if (logger.IsEnabledForLevel(LogLevel.TRACE))
                {
                    logger.Trace("failed to update client address book after INVALID_NODE_ACCOUNT_ID: {}", updateError.GetMessage());
                }
            }
        }

        /// <summary>
        /// Execute this transaction or query
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>Result of execution</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public virtual O Execute(Client client)
        {
            return Execute(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Execute this transaction or query with a timeout
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>Result of execution</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        public virtual O Execute(Client client, Duration timeout)
        {
            Throwable lastException = null;
            if (IsBatchedAndNotBatchTransaction())
            {
                throw new ArgumentException("Cannot execute batchified transaction outside of BatchTransaction");
            }


            // If the logger on the request is not set, use the logger in client
            // (if set, otherwise do not use logger)
            if (logger == null)
            {
                logger = client.GetLogger();
            }

            MergeFromClient(client);
            OnExecute(client);
            CheckNodeAccountIds();
            SetNodesFromNodeAccountIds(client);
            var timeoutTime = Instant.Now().Plus(timeout);
            for (int attempt = 1;; attempt++)
            {
                if (attempt > maxAttempts)
                {
                    throw new MaxAttemptsExceededException(lastException);
                }

                Duration currentTimeout = Duration.Between(Instant.Now(), timeoutTime);
                if (currentTimeout.IsNegative() || currentTimeout.IsZero())
                {
                    throw new TimeoutException();
                }

                GrpcRequest grpcRequest = new GrpcRequest(client.network, attempt, currentTimeout);
                Node node = grpcRequest.GetNode();
                ResponseT response = null;

                // If we get an unhealthy node here, we've cycled through all the "good" nodes that have failed
                // and have no choice but to try a bad one.
                if (!node.IsHealthy())
                {
                    Delay(node.GetRemainingTimeForBackoff());
                }

                if (node.ChannelFailedToConnect(timeoutTime))
                {
                    logger.Trace("Failed to connect channel for node {} for request #{}", node.GetAccountId(), attempt);
                    lastException = grpcRequest.ReactToConnectionFailure();
                    AdvanceRequest(); // Advance to next node before retrying
                    continue;
                }

                currentTimeout = Duration.Between(Instant.Now(), timeoutTime);
                grpcRequest.SetGrpcDeadline(currentTimeout);
                try
                {
                    response = blockingUnaryCall.Apply(grpcRequest);
                    LogTransaction(GetTransactionIdInternal(), client, node, false, attempt, response, null);
                }
                catch (Throwable e)
                {
                    if (e is StatusRuntimeException)
                    {
                        StatusRuntimeException statusRuntimeException = (StatusRuntimeException)e;
                        if (statusRuntimeException.GetStatus().GetCode().Equals(Code.DEADLINE_EXCEEDED))
                        {
                            throw new TimeoutException();
                        }
                    }

                    lastException = e;
                    LogTransaction(GetTransactionIdInternal(), client, node, false, attempt, null, e);
                }

                if (response == null)
                {
                    if (grpcRequest.ShouldRetryExceptionally(lastException))
                    {
                        AdvanceRequest(); // Advance to next node before retrying
                        continue;
                    }
                    else
                    {
                        throw new Exception(lastException);
                    }
                }

                var status = MapResponseStatus(response);
                var executionState = GetExecutionState(status, response);
                grpcRequest.HandleResponse(response, status, executionState, client);
                switch (executionState)
                {
                    case RETRY:

                        // Response is not ready yet from server, need to wait.
                        // Handle INVALID_NODE_ACCOUNT: mark node as unusable and update network
                        if (status == Status.INVALID_NODE_ACCOUNT)
                        {
                            if (logger.IsEnabledForLevel(LogLevel.TRACE))
                            {
                                logger.Trace("Received INVALID_NODE_ACCOUNT; updating address book and marking node {} as unhealthy, attempt #{}", node.GetAccountId(), attempt);
                            }


                            // Schedule async address book update
                            UpdateNetworkFromAddressBook(client);

                            // Mark this node as unhealthy
                            client.network.IncreaseBackoff(node);
                        }

                        lastException = grpcRequest.MapStatusException();
                        if (attempt < maxAttempts)
                        {
                            currentTimeout = Duration.Between(Instant.Now(), timeoutTime);
                            Delay(Math.Min(currentTimeout.ToMillis(), grpcRequest.GetDelay()));
                        }

                        continue;
                    case SERVER_ERROR:
                        lastException = grpcRequest.MapStatusException();
                        AdvanceRequest(); // Advance to next node before retrying
                        continue;
                    case REQUEST_ERROR:
                        throw grpcRequest.MapStatusException();
                    case SUCCESS:
                    default:
                        return grpcRequest.MapResponse();
                        break;
                }
            }
        }

        protected virtual bool IsBatchedAndNotBatchTransaction()
        {
            return false;
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
        /// because it uses features introduced in API level 31 (Android 12).</p>*
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>Future result of execution</returns>
        public virtual CompletableFuture<O> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
        /// because it uses features introduced in API level 31 (Android 12).</p>*
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <returns>Future result of execution</returns>
        public virtual CompletableFuture<O> ExecuteAsync(Client client, Duration timeout)
        {
            var retval = new CompletableFuture<O>().OrTimeout(timeout.ToMillis(), TimeUnit.MILLISECONDS);
            MergeFromClient(client);
            OnExecuteAsync(client).ThenRun(() =>
            {
                CheckNodeAccountIds();
                SetNodesFromNodeAccountIds(client);
                ExecuteAsyncInternal(client, 1, null, retval, timeout);
            }).Exceptionally((error) =>
            {
                retval.CompleteExceptionally(error);
                return null;
            });
            return retval;
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="callback">a BiConsumer which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, BiConsumer<O, Throwable> callback)
        {
            ConsumerHelper.BiConsumer(ExecuteAsync(client), callback);
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a BiConsumer which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Duration timeout, BiConsumer<O, Throwable> callback)
        {
            ConsumerHelper.BiConsumer(ExecuteAsync(client, timeout), callback);
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="onSuccess">a Consumer which consumes the result on success.</param>
        /// <param name="onFailure">a Consumer which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Consumer<O> onSuccess, Consumer<Throwable> onFailure)
        {
            ConsumerHelper.TwoConsumers(ExecuteAsync(client), onSuccess, onFailure);
        }

        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Consumer which consumes the result on success.</param>
        /// <param name="onFailure">a Consumer which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Duration timeout, Consumer<O> onSuccess, Consumer<Throwable> onFailure)
        {
            ConsumerHelper.TwoConsumers(ExecuteAsync(client, timeout), onSuccess, onFailure);
        }

        /// <summary>
        /// Logs the transaction's parameters
        /// </summary>
        /// <param name="transactionId">the transaction's id</param>
        /// <param name="client">the client that executed the transaction</param>
        /// <param name="node">the node the transaction was sent to</param>
        /// <param name="isAsync">whether the transaction was executed asynchronously</param>
        /// <param name="attempt">the attempt number</param>
        /// <param name="response">the transaction response if the transaction was successful</param>
        /// <param name="error">the error if the transaction was not successful</param>
        protected virtual void LogTransaction(TransactionId transactionId, Client client, Node node, bool isAsync, int attempt, ResponseT response, Throwable error)
        {
            if (!logger.IsEnabledForLevel(LogLevel.TRACE))
            {
                return;
            }

            logger.Trace("Execute{} Transaction ID: {}, submit to {}, node: {}, attempt: {}", isAsync ? "Async" : "", transactionId, client.network, node.GetAccountId(), attempt);
            if (response != null)
            {
                logger.Trace(" - Response: {}", response);
            }

            if (error != null)
            {
                logger.Trace(" - Error: {}", error.GetMessage());
            }
        }

        virtual void SetNodesFromNodeAccountIds(Client client)
        {
            nodes.Clear();

            // When a single node is explicitly set we get all of its proxies so in case of
            // failure the system can retry with different proxy on each attempt
            if (nodeAccountIds.Count == 1)
            {
                var nodeProxies = client.network.GetNodeProxies(nodeAccountIds[0]);
                if (nodeProxies == null || nodeProxies.IsEmpty())
                {
                    throw new InvalidOperationException("Account ID did not map to valid node in the client's network");
                }

                nodes.AddAll(nodeProxies).Shuffle();
                return;
            }


            // When multiple nodes are available the system retries with different node on each attempt
            // instead of different proxy of the same node
            foreach (var accountId in nodeAccountIds)
            {
                var nodeProxies = client.network.GetNodeProxies(accountId);
                if (nodeProxies == null || nodeProxies.IsEmpty())
                {
                    logger.Warn("Attempting to fetch node {} proxy which is not included in the Client's network. Please review your Client config.", accountId.ToString());
                    continue;
                }

                var node = nodeProxies[random.NextInt(nodeProxies.Count)];
                nodes.Add(Objects.RequireNonNull(node));
            }

            if (nodes.IsEmpty())
            {
                throw new InvalidOperationException("All node account IDs did not map to valid nodes in the client's network");
            }
        }

        /// <summary>
        /// Return the next node for execution. Will select the first node that is deemed healthy. If we cannot find such a
        /// node and have tried n nodes (n being the size of the node list), we will select the node with the smallest
        /// remaining delay. All delays MUST be executed in calling layer as this method will be called for sync + async
        /// scenarios.
        /// </summary>
        virtual Node GetNodeForExecute(int attempt)
        {
            Node node = null;
            Node candidate = null;
            long smallestDelay = Long.MAX_VALUE;
            for (int _i = 0; _i < nodes.Count; _i++)
            {

                // NOTE: _i is NOT the index into nodes, it is just keeping track of how many times we've iterated.
                // In the event of ServerErrors, this method depends on the nodes list to have advanced to
                // the next node.
                node = nodes.GetCurrent();
                if (!node.IsHealthy())
                {

                    // Keep track of the node with the smallest delay seen thus far. If we go through the entire list
                    // (meaning all nodes are unhealthy) then we will select the node with the smallest delay.
                    long backoff = node.GetRemainingTimeForBackoff();
                    if (backoff < smallestDelay)
                    {
                        candidate = node;
                        smallestDelay = backoff;
                    }

                    node = null;
                    AdvanceRequest();
                }
                else
                {
                    break; // got a good node, use it
                }
            }

            if (node == null)
            {
                node = candidate;

                // If we've tried all nodes, index will be +1 too far. Index increment happens outside
                // this method so try to be consistent with happy path.
                nodeAccountIds.SetIndex(Math.Max(0, nodeAccountIds.GetIndex()));
            }


            // node won't be null at this point because execute() validates before this method is called.
            // Add null check here to work around sonar NPE detection.
            if (node != null && logger != null)
            {
                logger.Trace("Using node {} for request #{}: {}", node.GetAccountId(), attempt, this);
            }

            return node;
        }

        private ProtoRequestT GetRequestForExecute()
        {
            var request = MakeRequest();
            return request;
        }

        private void ExecuteAsyncInternal(Client client, int attempt, Throwable lastException, CompletableFuture<O> returnFuture, Duration timeout)
        {

            // If the logger on the request is not set, use the logger in client
            // (if set, otherwise do not use logger)
            if (logger == null && client.GetLogger() != null)
            {
                logger = client.GetLogger();
            }

            if (returnFuture.IsCancelled() || returnFuture.IsCompletedExceptionally() || returnFuture.IsDone())
            {
                return;
            }

            if (attempt > maxAttempts)
            {
                returnFuture.CompleteExceptionally(new CompletionException(new MaxAttemptsExceededException(lastException)));
                return;
            }

            var timeoutTime = Instant.Now().Plus(timeout);
            GrpcRequest grpcRequest = new GrpcRequest(client.network, attempt, Duration.Between(Instant.Now(), timeoutTime));
            Supplier<CompletableFuture<Void>> afterUnhealthyDelay = () =>
            {
                return grpcRequest.GetNode().IsHealthy() ? CompletableFuture.CompletedFuture((Void)null) : Delayer.DelayFor(grpcRequest.GetNode().GetRemainingTimeForBackoff(), client.executor);
            };
            afterUnhealthyDelay.Get().ThenRun(() =>
            {
                grpcRequest.GetNode().ChannelFailedToConnectAsync().ThenAccept((connectionFailed) =>
                {
                    if (connectionFailed)
                    {
                        var connectionException = grpcRequest.ReactToConnectionFailure();
                        AdvanceRequest(); // Advance to next node before retrying
                        ExecuteAsyncInternal(client, attempt + 1, connectionException, returnFuture, Duration.Between(Instant.Now(), timeoutTime));
                        return;
                    }

                    ToCompletableFuture(ClientCalls.FutureUnaryCall(grpcRequest.CreateCall(), grpcRequest.GetRequest())).Handle((response, error) =>
                    {
                        LogTransaction(GetTransactionIdInternal(), client, grpcRequest.GetNode(), true, attempt, response, error);
                        if (grpcRequest.ShouldRetryExceptionally(error))
                        {

                            // the transaction had a network failure reaching Hedera
                            AdvanceRequest(); // Advance to next node before retrying
                            ExecuteAsyncInternal(client, attempt + 1, error, returnFuture, Duration.Between(Instant.Now(), timeoutTime));
                            return null;
                        }

                        if (error != null)
                        {

                            // not a network failure, some other weirdness going on; just fail fast
                            returnFuture.CompleteExceptionally(new CompletionException(error));
                            return null;
                        }

                        var status = MapResponseStatus(response);
                        var executionState = GetExecutionState(status, response);
                        grpcRequest.HandleResponse(response, status, executionState, client);
                        switch (executionState)
                        {
                            case RETRY:

                                // Response is not ready yet from server, need to wait.
                                // Handle INVALID_NODE_ACCOUNT: mark node as unusable and update network
                                if (status == Status.INVALID_NODE_ACCOUNT)
                                {
                                    if (logger.IsEnabledForLevel(LogLevel.TRACE))
                                    {
                                        logger.Trace("Received INVALID_NODE_ACCOUNT; updating address book and marking node {} as unhealthy, attempt #{}", grpcRequest.GetNode().GetAccountId(), attempt);
                                    }


                                    // Schedule async address book update
                                    UpdateNetworkFromAddressBook(client);

                                    // Mark this node as unhealthy
                                    client.network.IncreaseBackoff(grpcRequest.GetNode());
                                }

                                Delayer.DelayFor((attempt < maxAttempts) ? grpcRequest.GetDelay() : 0, client.executor).ThenRun(() => ExecuteAsyncInternal(client, attempt + 1, grpcRequest.MapStatusException(), returnFuture, Duration.Between(Instant.Now(), timeoutTime)));
                                break;
                            case SERVER_ERROR:
                                AdvanceRequest(); // Advance to next node before retrying
                                ExecuteAsyncInternal(client, attempt + 1, grpcRequest.MapStatusException(), returnFuture, Duration.Between(Instant.Now(), timeoutTime));
                                break;
                            case REQUEST_ERROR:
                                returnFuture.CompleteExceptionally(new CompletionException(grpcRequest.MapStatusException()));
                                break;
                            case SUCCESS:
                            default:
                                returnFuture.Complete(grpcRequest.MapResponse());
                                break;
                        }

                        return null;
                    }).Exceptionally((error) =>
                    {
                        returnFuture.CompleteExceptionally(error);
                        return null;
                    });
                }).Exceptionally((error) =>
                {
                    returnFuture.CompleteExceptionally(error);
                    return null;
                });
            });
        }

        abstract ProtoRequestT MakeRequest();
        virtual GrpcRequest GetGrpcRequest(int attempt)
        {
            return new GrpcRequest(null, attempt, grpcDeadline);
        }

        virtual void AdvanceRequest()
        {
            if (nodeAccountIds.GetIndex() + 1 == nodes.Count - 1)
            {
                attemptedAllNodes = true;
            }

            nodes.Advance();
            if (nodeAccountIds.Count > 1)
            {
                nodeAccountIds.Advance();
            }
        }

        /// <summary>
        /// Called after receiving the query response from Hedera. The derived class should map into its output type.
        /// </summary>
        abstract O MapResponse(ResponseT response, AccountId nodeId, ProtoRequestT request);
        abstract Status MapResponseStatus(ResponseT response);
        /// <summary>
        /// Called to direct the invocation of the query to the appropriate gRPC service.
        /// </summary>
        abstract MethodDescriptor<ProtoRequestT, ResponseT> GetMethodDescriptor();
        abstract TransactionId GetTransactionIdInternal();
        virtual bool ShouldRetryExceptionally(Throwable error)
        {
            if (error is StatusRuntimeException)
            {
                var status = statusException.GetStatus().GetCode();
                var description = statusException.GetStatus().GetDescription();
                return (status == Code.UNAVAILABLE) || (status == Code.RESOURCE_EXHAUSTED) || (status == Code.INTERNAL && description != null && RST_STREAM.Matcher(description).Matches());
            }

            return false;
        }

        /// <summary>
        /// Default implementation, may be overridden in subclasses (especially for query case). Called just after receiving
        /// the query response from Hedera. By default it triggers a retry when the pre-check status is {@code BUSY}.
        /// </summary>
        virtual ExecutionState GetExecutionState(Status status, ResponseT response)
        {
            switch (status)
            {
                case PLATFORM_TRANSACTION_NOT_CREATED:
                case PLATFORM_NOT_ACTIVE:
                    return ExecutionState.SERVER_ERROR;
                case BUSY:
                case INVALID_NODE_ACCOUNT:
                    return ExecutionState.RETRY; // INVALID_NODE_ACCOUNT retries with special handling for node account update
                case OK:
                    return ExecutionState.SUCCESS;
                default:
                    return ExecutionState.REQUEST_ERROR; // user error
                    break;
            }
        }

        class GrpcRequest
        {
            private readonly Network network;
            private readonly Node node;
            private readonly int attempt;
            // private final ClientCall<ProtoRequestT, ResponseT> call;
            private readonly ProtoRequestT request;
            private readonly long startAt;
            private readonly long delay;
            private Duration grpcDeadline;
            private ResponseT response;
            private double latency;
            private Status responseStatus;
            GrpcRequest(Network network, int attempt, Duration grpcDeadline)
            {
                network = network;
                attempt = attempt;
                grpcDeadline = grpcDeadline;
                node = GetNodeForExecute(attempt);
                request = GetRequestForExecute(); // node index gets incremented here
                startAt = System.NanoTime();

                // Exponential back-off for Delayer: 250ms, 500ms, 1s, 2s, 4s, 8s, ... 8s
                delay = (long)Math.Min(Objects.RequireNonNull(minBackoff).ToMillis() * Math.Pow(2, attempt - 1), Objects.RequireNonNull(maxBackoff).ToMillis());
            }

            public virtual CallOptions GetCallOptions()
            {
                long deadline = Math.Min(grpcDeadline.ToMillis(), grpcDeadline.ToMillis());
                return CallOptions.DEFAULT.WithDeadlineAfter(deadline, TimeUnit.MILLISECONDS);
            }

            public virtual void SetGrpcDeadline(Duration grpcDeadline)
            {
                grpcDeadline = grpcDeadline;
            }

            public virtual Node GetNode()
            {
                return node;
            }

            public virtual ClientCall<ProtoRequestT, ResponseT> CreateCall()
            {
                VerboseLog(node);
                return node.GetChannel().NewCall(GetMethodDescriptor(), GetCallOptions());
            }

            public virtual ProtoRequestT GetRequest()
            {
                return requestListener.Apply(request);
            }

            public virtual long GetDelay()
            {
                return delay;
            }

            virtual Throwable ReactToConnectionFailure()
            {
                Objects.RequireNonNull(network).IncreaseBackoff(node);
                logger.Warn("Retrying in {} ms after channel connection failure with node {} during attempt #{}", node.GetRemainingTimeForBackoff(), node.GetAccountId(), attempt);
                VerboseLog(node);
                return new InvalidOperationException("Failed to connect to node " + node.GetAccountId());
            }

            virtual bool ShouldRetryExceptionally(Throwable e)
            {
                latency = (double)(System.NanoTime() - startAt) / 1000000000;
                var retry = ShouldRetryExceptionally(e);
                if (retry)
                {
                    Objects.RequireNonNull(network).IncreaseBackoff(node);
                    logger.Warn("Retrying in {} ms after failure with node {} during attempt #{}: {}", node.GetRemainingTimeForBackoff(), node.GetAccountId(), attempt, e != null ? e.GetMessage() : "NULL");
                    VerboseLog(node);
                }

                return retry;
            }

            virtual PrecheckStatusException MapStatusException()
            {

                // request to hedera failed in a non-recoverable way
                return new PrecheckStatusException(responseStatus, GetTransactionIdInternal());
            }

            virtual O MapResponse()
            {

                // successful response from Hedera
                return MapResponse(response, node.GetAccountId(), request);
            }

            virtual void HandleResponse(ResponseT response, Status status, ExecutionState executionState, Client client)
            {

                // Note: For INVALID_NODE_ACCOUNT, we don't mark the node as unhealthy here
                // because we need to do it AFTER advancing the request, to match Go SDK behavior
                if (status != Status.INVALID_NODE_ACCOUNT)
                {
                    node.DecreaseBackoff();
                }

                response = responseListener.Apply(response);
                responseStatus = status;
                logger.Trace("Received {} response in {} s from node {} during attempt #{}: {}", responseStatus, latency, node.GetAccountId(), attempt, response);
                if (executionState == ExecutionState.SERVER_ERROR && attemptedAllNodes)
                {
                    executionState = ExecutionState.RETRY;
                    attemptedAllNodes = false;
                }

                switch (executionState)
                {
                    case RETRY:
                    {
                        logger.Warn("Retrying in {} ms after failure with node {} during attempt #{}: {}", delay, node.GetAccountId(), attempt, responseStatus);
                        VerboseLog(node);
                    }

                    case SERVER_ERROR:
                    {

                        // Note: INVALID_NODE_ACCOUNT is handled after advanceRequest() in execute methods
                        // to match Go SDK's executionStateRetryWithAnotherNode behavior
                        if (status != Status.INVALID_NODE_ACCOUNT)
                        {
                            logger.Warn("Problem submitting request to node {} for attempt #{}, retry with new node: {}", node.GetAccountId(), attempt, responseStatus);
                            VerboseLog(node);
                        }
                    }

                    default:
                    {
                    }

                        break;
                }
            }

            virtual void VerboseLog(Node node)
            {
                string ipAddress;
                if (node.address == null)
                {
                    ipAddress = "NULL";
                }
                else if (node.address.GetAddress() == null)
                {
                    ipAddress = "NULL";
                }
                else
                {
                    ipAddress = node.address.GetAddress();
                }

                logger.Trace("Node IP {} Timestamp {} Transaction Type {}", ipAddress, System.CurrentTimeMillis(), GetType().GetSimpleName());
            }
        }
    }
}