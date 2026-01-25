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
using Google.Protobuf.WellKnownTypes;
using System.Text.RegularExpressions;
using Hedera.Hashgraph.SDK.Transactions.Account;
using System.Threading.Tasks;
using System.Threading;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Transactions;

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
        static readonly Regex RST_STREAM = new (".*\\brst[^0-9a-zA-Z]stream\\b.*", RegexOptions.IgnoreCase | RegexOptions.DOTALL);
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
        private Func<ProtoRequestT, ProtoRequestT> requestListener;
        // Lambda responsible for executing synchronous gRPC requests. Pluggable for unit testing.
        Function<GrpcRequest, ResponseT> blockingUnaryCall = (grpcRequest) => ClientCalls.BlockingUnaryCall(grpcRequest.CreateCall(), grpcRequest.GetRequest());
        private Func<ResponseT, ResponseT> responseListener;
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
        /// When execution is attempted, a single attempt will timeout when this deadline is reached. (The SDK may
        /// subsequently retry the execution.)
        /// </summary>
        public Duration GrpcDeadline { get; set; }

        /// <summary>
        /// The maximum amount of time to wait between retries. Every retry attempt will increase the wait time exponentially
        /// until it reaches this time.
        /// </summary>
        public Duration MaxBackoff
        {
            get => field ?? Client.DEFAULT_MAX_BACKOFF;
			set
			{
				if (field == null || field.Nanos < 0)
				{
					throw new ArgumentException("maxBackoff must be a positive duration");
				}
				else if (field.CompareTo(MinBackoff) < 0)
				{
					throw new ArgumentException("maxBackoff must be greater than or equal to minBackoff");
				}

				field = value;
			}
		}
        
        /// <summary>
        /// The minimum amount of time to wait between retries. When retrying, the delay will start at this time and increase
        /// exponentially until it reaches the maxBackoff.
        /// </summary>
        public Duration MinBackoff
        {
            get => field ?? Client.DEFAULT_MIN_BACKOFF;
			set 
            {
				if (value == null || value.Nanos < 0)
				{
					throw new ArgumentException("minBackoff must be a positive duration");
				}
				else if (value.CompareTo(MaxBackoff) > 0)
				{
					throw new ArgumentException("minBackoff must be less than or equal to maxBackoff");
				}

				field = value;
			}
        }

        public int MaxRetry
        {
            get => MaxAttempts;
            set => MaxAttempts = value;
		}

        public int MaxAttempts
        {
            get;
            set
            {
                if (maxAttempts <= 0)
                {
                    throw new ArgumentException("maxAttempts must be greater than zero");
                }

                field = value;
            }
        } = Client.DEFAULT_MAX_ATTEMPTS;

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
        /// Set a callback that will be called right before the request is sent. As input, the callback will receive the
        /// protobuf of the request, and the callback should return the request protobuf.  This means the callback has an
        /// opportunity to read, copy, or modify the request that will be sent.
        /// </summary>
        /// <param name="requestListener">The callback to use</param>
        /// <returns>{@code this}</returns>
        public SdkRequestT SetRequestListener(Func<ProtoRequestT, ProtoRequestT> requestListener)
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
        public SdkRequestT SetResponseListener(Func<ResponseT, ResponseT> responseListener)
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

		

		public abstract void OnExecute(Client client);
		public abstract Task OnExecuteAsync(Client client);
		public abstract ProtoRequestT MakeRequest();
		public abstract Status MapResponseStatus(ResponseT response);
		/// <summary>
		/// Called after receiving the query response from Hedera. The derived class should map into its output type.
		/// </summary>
		public abstract O MapResponse(ResponseT response, AccountId nodeId, ProtoRequestT request);

		

        public virtual void MergeFromClient(Client client)
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
		protected virtual void CheckNodeAccountIds()
		{
			if (nodeAccountIds.IsEmpty())
			{
				throw new InvalidOperationException("Request node account IDs were not set before executing");
			}
		}
		protected virtual bool IsBatchedAndNotBatchTransaction()
		{
			return false;
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
                        logger.Debug("Sleeping for: " + delay + " | Thread name: " + Thread.CurrentThread.Name);
                    }

                    Thread.Sleep((int)delay);
                }
            }
            catch (ThreadInterruptedException e)
            {
                throw new Exception(string.Empty, e);
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
                if (client.GetMirrorNetwork() != null && client.GetMirrorNetwork().Count > 0)
                {
                    client.UpdateNetworkFromAddressBook();
                }
            }
            catch (Exception updateError)
            {
                if (logger.IsEnabledForLevel(LogLevel.TRACE))
                {
                    logger.Trace("failed to update client address book after INVALID_NODE_ACCOUNT_ID: {}", updateError.Message);
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
            Exception lastException = null;
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
                catch (Exception e)
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
        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 31
        /// because it uses features introduced in API level 31 (Android 12).</p>*
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <returns>Future result of execution</returns>
        public virtual Task<O> ExecuteAsync(Client client)
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
        public virtual Task<O> ExecuteAsync(Client client, Duration timeout)
        {
            var retval = new Task<O>().OrTimeout(timeout.ToMillis(), TimeUnit.MILLISECONDS);
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
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Action<O, Exception> callback)
        {
            ActionHelper.Action(ExecuteAsync(client), callback);
        }
        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Duration timeout, Action<O, Exception> callback)
        {
            ActionHelper.Action(ExecuteAsync(client, timeout), callback);
        }
        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Action<O> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(ExecuteAsync(client), onSuccess, onFailure);
        }
        /// <summary>
        /// Execute this transaction or query asynchronously.
        /// </summary>
        /// <param name="client">The client with which this will be executed.</param>
        /// <param name="timeout">The timeout after which the execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Duration timeout, Action<O> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(ExecuteAsync(client, timeout), onSuccess, onFailure);
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
        protected virtual void LogTransaction(TransactionId transactionId, Client client, Node node, bool isAsync, int attempt, ResponseT response, Exception error)
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
                nodeAccountIds.SetIndex(Math.Max(0, nodeAccountIds.Index()));
            }


            // node won't be null at this point because execute() validates before this method is called.
            // Add null check here to work around sonar NPE detection.
            if (node != null && logger != null)
            {
                logger.Trace("Using node {} for request #{}: {}", node.AccountId, attempt, this);
            }

            return node;
        }

        private ProtoRequestT GetRequestForExecute()
        {
            var request = MakeRequest();
            return request;
        }

        private void ExecuteAsyncInternal(Client client, int attempt, Exception lastException, Task<O> returnFuture, Duration timeout)
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
            Supplier<Task> afterUnhealthyDelay = () =>
            {
                return grpcRequest.GetNode().IsHealthy() ? Task.FromResult((Void)null) : Delayer.DelayFor(grpcRequest.GetNode().GetRemainingTimeForBackoff(), client.executor);
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

                    ToTask(ClientCalls.FutureUnaryCall(grpcRequest.CreateCall(), grpcRequest.GetRequest())).Handle((response, error) =>
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

        
		virtual GrpcRequest GetGrpcRequest(int attempt)
        {
            return new GrpcRequest(null, attempt, grpcDeadline);
        }

        virtual void AdvanceRequest()
        {
            if (nodeAccountIds.Index() + 1 == nodes.Count - 1)
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
		/// Called to direct the invocation of the query to the appropriate gRPC service.
		/// </summary>
		public abstract MethodDescriptor<ProtoRequestT, ResponseT> GetMethodDescriptor();
        public abstract TransactionId GetTransactionIdInternal();
        virtual bool ShouldRetryExceptionally(Exception error)
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
    }
}