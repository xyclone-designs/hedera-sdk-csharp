// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Grpc.Core;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Transactions;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/// <include file="Executable.cs.xml" path='docs/member[@name="T:Executable"]/*' />
	public abstract partial class Executable
	{
		internal static readonly Regex RST_STREAM = new(".*\\brst[^0-9a-zA-Z]stream\\b.*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
	}

    public abstract partial class Executable<TSdkRequest, TProtoRequest, TProtoResponse, TTransactionResponse> : Executable where TProtoRequest : class, IMessage where TProtoResponse : class, IMessage
	{
        protected static readonly Random random = new ();
		protected Func<GrpcRequest, TProtoResponse> BlockingUnaryCall 
		{
			get => grpcRequest =>
			{
				var call = grpcRequest.CreateCall();
				var request = grpcRequest.GetRequest();

				return call.BlockingUnaryCall(GetMethod(), null, grpcRequest.CallOptions, request);
			};
		}

		/// <include file="Executable.cs.xml" path='docs/member[@name="T:Executable_2"]/*' />
		protected ListGuarded<Node> Nodes
		{
			get => [.. field ??= []];
			set
			{
				field = value;
			}
		}

		/// <include file="Executable.cs.xml" path='docs/member[@name="P:Executable.AttemptedAllNodes"]/*' />
		public bool AttemptedAllNodes { get; protected set; }
		/// <include file="Executable.cs.xml" path='docs/member[@name="P:Executable.GrpcDeadline"]/*' />
		public TimeSpan GrpcDeadline { get; set; }
		/// <include file="Executable.cs.xml" path='docs/member[@name="T:Executable_3"]/*' />
		public TimeSpan MaxBackoff
		{
			get => field;
			set
			{
				if (field.Nanoseconds < 0)
				{
					throw new ArgumentException("maxBackoff must be a positive duration");
				}
				else if (field.CompareTo(MinBackoff) < 0)
				{
					throw new ArgumentException("maxBackoff must be greater than or equal to minBackoff");
				}

				field = value;
			}

		} = Client.DEFAULT_MAX_BACKOFF;
		/// <include file="Executable.cs.xml" path='docs/member[@name="T:Executable_4"]/*' />
		public TimeSpan MinBackoff
		{
			get => field;
			set
			{
				if (value.Nanoseconds < 0)
				{
					throw new ArgumentException("minBackoff must be a positive duration");
				}
				else if (value.CompareTo(MaxBackoff) > 0)
				{
					throw new ArgumentException("minBackoff must be less than or equal to maxBackoff");
				}

				field = value;
			}

		} = Client.DEFAULT_MIN_BACKOFF;
		/// <include file="Executable.cs.xml" path='docs/member[@name="T:Executable_5"]/*' />
		public int MaxAttempts
        {
            get;
            set
            {
                if (value <= 0)
					throw new ArgumentException("MaxAttempts must be greater than zero");

				field = value;
            }
        } = Client.DEFAULT_MAX_ATTEMPTS;
        public int MaxRetry
        {
            get => MaxAttempts;
            set => MaxAttempts = value;
        }
		/// <include file="Executable.cs.xml" path='docs/member[@name="T:Executable_6"]/*' />
		public ListGuarded<AccountId> NodeAccountIds
        {
            get => field ??= [];
            set
            {
                field = value;
            }
        }
        /// <include file="Executable.cs.xml" path='docs/member[@name="T:Executable_7"]/*' />
        public Func<TProtoRequest, TProtoRequest> RequestListener
		{
			set; get => field ??= (request) =>
			{
				Logger?.Trace("Sent protobuf {}", Hex.ToHexString(request.ToByteArray()));

				return request;
			};
		}
        /// <include file="Executable.cs.xml" path='docs/member[@name="T:Executable_8"]/*' />
        public Func<TProtoResponse, TProtoResponse> ResponseListener
        {
            set; get => field ??= (response) =>
			{
				Logger?.Trace("Received protobuf {}", Hex.ToHexString(response.ToByteArray()));

				return response;
			};
		}

        /// <include file="Executable.cs.xml" path='docs/member[@name="P:Executable.Logger"]/*' />
        public virtual Logger? Logger { get; set; }
		
		/// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.GetMethod"]/*' />
		public abstract Method<TProtoRequest, TProtoResponse> GetMethod();
		public abstract MethodDescriptor GetMethodDescriptor();
		public abstract TransactionId TransactionIdInternal { get; }
		public abstract void OnExecute(Client client);
		public abstract Task OnExecuteAsync(Client client);
		public abstract TProtoRequest MakeRequest();
		public abstract ResponseStatus MapResponseStatus(Proto.Response response);
		/// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.MapResponse(TProtoResponse,AccountId,TProtoRequest)"]/*' />
		public abstract TTransactionResponse MapResponse(TProtoResponse response, AccountId nodeId, TProtoRequest request);

		protected virtual void CheckNodeAccountIds()
		{
			if (NodeAccountIds.Count == 0)
				throw new InvalidOperationException("Request node account IDs were not set before executing");
		}
		protected virtual bool IsBatchedAndNotBatchTransaction()
		{
			return false;
		}		

		public virtual void AdvanceRequest()
		{
			if (NodeAccountIds.Index + 1 == Nodes.Count - 1)
				AttemptedAllNodes = true;

			Nodes.Advance();

			if (NodeAccountIds.Count > 1)
				NodeAccountIds.Advance();
		}
		public virtual GrpcRequest GetGrpcRequest(int attempt)
		{
			return new GrpcRequest(this, null, attempt, GrpcDeadline);
		}
		/// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.GetNodeForExecute(System.Int32)"]/*' />
		public virtual Node GetNodeForExecute(int attempt)
		{
			Node? node = null;
			Node? candidate = null;
			long smallestDelay = long.MaxValue;
			for (int _i = 0; _i < Nodes.Count; _i++)
			{

				// NOTE: _i is NOT the index into nodes, it is just keeping track of how many times we've iterated.
				// In the event of ServerErrors, this method depends on the nodes list to have advanced to
				// the next node.
				node = Nodes.Current;

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
				NodeAccountIds.Index = Math.Max(0, NodeAccountIds.Index);
			}

			// node won't be null at this point because execute() validates before this method is called.
			// Add null check here to work around sonar NPE detection.
			if (node != null)
				Logger?.Trace("Using node {} for request #{}: {}", node.AccountId, attempt, this);

			return node!;
		}
		/// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.GetExecutionState(ResponseStatus,TProtoResponse)"]/*' />
		public virtual ExecutionState GetExecutionState(ResponseStatus status, TProtoResponse response)
		{
			return status switch
			{
				ResponseStatus.PlatformTransactionNotCreated or
				ResponseStatus.PlatformNotActive => ExecutionState.ServerError,

				ResponseStatus.Busy or
				ResponseStatus.InvalidNodeAccount => ExecutionState.Retry, // INVALID_NODE_ACCOUNT retries with special handling for node account update

				ResponseStatus.Ok => ExecutionState.Success,

				_ => ExecutionState.RequestError, // user error
			};
		}
		public virtual void MergeFromClient(Client client)
        {
            MaxAttempts = client.MaxAttempts;
            MaxBackoff = client.MaxBackoff;
            MinBackoff = client.MinBackoff;
            GrpcDeadline = client.GrpcDeadline;
        }
		public virtual void SetNodesFromNodeAccountIds(Client client)
		{
			Nodes.Clear();

			// When a single node is explicitly set we get all of its proxies so in case of
			// failure the system can retry with different proxy on each attempt
			if (NodeAccountIds.Count == 1)
			{
				var nodeProxies = client.Network_.GetNodeProxies(NodeAccountIds[0]);
				if (nodeProxies == null || nodeProxies.Count == 0)
				{
					throw new InvalidOperationException("Account ID did not map to valid node in the client's network");
				}

				Nodes.AddRange(nodeProxies);
				Nodes.Shuffle();
				return;
			}


			// When multiple nodes are available the system retries with different node on each attempt
			// instead of different proxy of the same node
			foreach (var accountId in NodeAccountIds)
			{
				var nodeProxies = client.Network_.GetNodeProxies(accountId);
				if (nodeProxies == null || nodeProxies.Count == 0)
				{
					Logger?.Warn("Attempting to fetch node {} proxy which is not included in the Client's network. Please review your Client config.", accountId.ToString());
					continue;
				}

				 Node node = nodeProxies[random.Next(nodeProxies.Count)];

				Nodes.Add(node);
			}

			if (Nodes.Count == 0)
				throw new InvalidOperationException("All node account IDs did not map to valid nodes in the client's network");
		}
		public virtual bool ShouldRetryExceptionally(Exception error)
		{
			if (error is RpcException rpcexception)
			{
				var status = rpcexception.StatusCode;
				var description = rpcexception.Status.Detail;
				
				return 
					(status == StatusCode.Unavailable) || 
					(status == StatusCode.ResourceExhausted) || 
					(status == StatusCode.Internal && description != null && RST_STREAM.Count(description) > 0);
			}

			return false;
		}
		
        /// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.Execute(Client)"]/*' />
        public virtual TTransactionResponse Execute(Client client)
        {
            return Execute(client, client.RequestTimeout);
        }
		/// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.Execute(Client,System.TimeSpan)"]/*' />
		public virtual TTransactionResponse Execute(Client client, TimeSpan timeout)
		{
			Exception? lastException = null;

			if (IsBatchedAndNotBatchTransaction())
			{
				throw new ArgumentException("Cannot execute batchified transaction outside of BatchTransaction");
			}

			// Use client logger if not set
			//Logger ??= client.Logger_;

			MergeFromClient(client);
			OnExecute(client);
			CheckNodeAccountIds();
			SetNodesFromNodeAccountIds(client);

			DateTimeOffset timeoutTime = DateTimeOffset.UtcNow + timeout;

			for (int attempt = 1; ; attempt++)
			{
				if (attempt > MaxAttempts)
					throw new MaxAttemptsExceededException(lastException);

				TimeSpan remainingTimeout = timeoutTime - DateTimeOffset.UtcNow;

				if (remainingTimeout <= TimeSpan.Zero)
					throw new TimeoutException();

				GrpcRequest grpcRequest = new(this, client.Network_, attempt, remainingTimeout);

				TProtoResponse? response = default;

				// If node is unhealthy, wait backoff
				if (!grpcRequest.Node.IsHealthy())
				{
					Delay(grpcRequest.Node.GetRemainingTimeForBackoff());
				}

				if (grpcRequest.Node.ChannelFailedToConnect(timeoutTime))
				{
					lastException = grpcRequest.ReactToConnectionFailure();
					AdvanceRequest(); // move to next node
					continue;
				}

				grpcRequest.GrpcDeadline = remainingTimeout;

				try
				{
					response = BlockingUnaryCall.Invoke(grpcRequest);
					LogTransaction(TransactionIdInternal, client, grpcRequest.Node, false, attempt, response, null);
				}
				catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.DeadlineExceeded)
				{
					throw new TimeoutException();
				}
				catch (Exception e)
				{
					lastException = e;
					LogTransaction(TransactionIdInternal, client, grpcRequest.Node, false, attempt, default, e);
				}

				if (response == null)
				{
					if (grpcRequest.ShouldRetryExceptionally(lastException))
					{
						AdvanceRequest();
						continue;
					}
					else
					{
						throw lastException ?? new Exception("Unknown error executing request");
					}
				}

				var status = MapResponseStatus(response as Proto.Response);
				var executionState = GetExecutionState(status, response);

				grpcRequest.HandleResponse(response, status, executionState, client);

				switch (executionState)
				{
					case ExecutionState.Retry:
						if (status == ResponseStatus.InvalidNodeAccount)
						{
							Logger?.Trace("Received INVALID_NODE_ACCOUNT; updating address book and marking node {0} as unhealthy, attempt #{1}", grpcRequest.Node.AccountId, attempt);

							// Async address book update
							UpdateNetworkFromAddressBook(client);

							// Mark node as unhealthy
							client.Network_.IncreaseBackoff(grpcRequest.Node);
						}

						lastException = grpcRequest.MapStatusException();

						if (attempt < MaxAttempts)
						{
							remainingTimeout = timeoutTime - DateTimeOffset.UtcNow;
							Delay((int)Math.Min(remainingTimeout.TotalMilliseconds, grpcRequest.Delay.TotalMilliseconds));
						}

						continue;

					case ExecutionState.ServerError:
						lastException = grpcRequest.MapStatusException();
						AdvanceRequest();
						continue;

					case ExecutionState.RequestError:
						throw grpcRequest.MapStatusException();

					case ExecutionState.Success:
					default:
						return grpcRequest.MapResponse();
				}
			}
		}
        /// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.ExecuteAsync(Client)"]/*' />
        public virtual Task<TTransactionResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.RequestTimeout);
        }
        /// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.ExecuteAsync(Client,System.TimeSpan)"]/*' />
        public virtual async Task<TTransactionResponse> ExecuteAsync(Client client, TimeSpan timeout)
        {
			TaskCompletionSource<TTransactionResponse> retval = new ();

			retval.SetResult(default);

			MergeFromClient(client);

			await OnExecuteAsync(client);

			CheckNodeAccountIds();
			SetNodesFromNodeAccountIds(client);

			await ExecuteAsyncInternal(client, 1, null, retval, timeout);

			return await retval.Task;
		}
        /// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.ExecuteAsync(Client,System.Action{TTransactionResponse,System.Exception})"]/*' />
        public virtual async void ExecuteAsync(Client client, Action<TTransactionResponse?, Exception?> callback)
        {
			Utils.ActionHelper.Action(ExecuteAsync(client), callback);
		}
        /// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.ExecuteAsync(Client,System.TimeSpan,System.Action{TTransactionResponse,System.Exception})"]/*' />
        public virtual async void ExecuteAsync(Client client, TimeSpan timeout, Action<TTransactionResponse?, Exception?> callback)
        {
			Utils.ActionHelper.Action(ExecuteAsync(client, timeout), callback);
		}
        /// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.ExecuteAsync(Client,System.Action{TTransactionResponse},System.Action{System.Exception})"]/*' />
        public virtual async void ExecuteAsync(Client client, Action<TTransactionResponse> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(ExecuteAsync(client), onSuccess, onFailure);
		}
        /// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.ExecuteAsync(Client,System.TimeSpan,System.Action{TTransactionResponse},System.Action{System.Exception})"]/*' />
        public virtual async void ExecuteAsync(Client client, TimeSpan timeout, Action<TTransactionResponse> onSuccess, Action<Exception> onFailure)
        {
			Utils.ActionHelper.TwoActions(ExecuteAsync(client, timeout), onSuccess, onFailure);
		}

        /// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.LogTransaction(TransactionId,Client,Node,System.Boolean,System.Int32,TProtoResponse,System.Exception)"]/*' />
        protected virtual void LogTransaction(TransactionId transactionId, Client client, Node node, bool isAsync, int attempt, TProtoResponse? response, Exception? error)
        {
            Logger?.Trace("Execute{} Transaction ID: {}, submit to {}, node: {}, attempt: {}", isAsync ? "Async" : "", transactionId, client.Network_, node.AccountId, attempt);
            
            if (response != null)
				Logger?.Trace(" - Response: {}", response);

			if (error != null)
				Logger?.Trace(" - Error: {}", error.Message);
		}

		private void Delay(double delay)
		{
			if (delay <= 0)
			{
				return;
			}

			try
			{
				if (delay > 0)
				{
					Logger?.Debug("Sleeping for: " + delay + " | Thread name: " + Thread.CurrentThread.Name);

					Thread.Sleep((int)delay);
				}
			}
			catch (ThreadInterruptedException e)
			{
				throw new Exception(string.Empty, e);
			}
		}
		private TProtoRequest GetRequestForExecute()
		{
			var request = MakeRequest();
			return request;
		}
		private async Task ExecuteAsyncInternal(Client client, int attempt, Exception? lastException, TaskCompletionSource<TTransactionResponse> returnFuture, TimeSpan timeout)
		{
			// Use client logger if not set
			//Logger ??= client.Logger_;

			if (returnFuture.Task.IsCompleted || returnFuture.Task.IsCanceled)
				return;

			if (attempt > MaxAttempts)
			{
				returnFuture.TrySetException(new MaxAttemptsExceededException(lastException));
				return;
			}

			DateTimeOffset timeoutTime = DateTimeOffset.UtcNow + timeout;
			TimeSpan remainingTimeout = timeoutTime - DateTimeOffset.UtcNow;

			var grpcRequest = new GrpcRequest(this, client.Network_, attempt, remainingTimeout);

			try
			{
				// Wait if node is unhealthy
				if (!grpcRequest.Node.IsHealthy())
				{
					await new Delayer(client.Executor).DelayAsync(TimeSpan.FromMilliseconds(grpcRequest.Node.GetRemainingTimeForBackoff()));
				}

				// Check if node channel failed to connect
				if (grpcRequest.Node.ChannelFailedToConnect(timeoutTime))
				{
					var connectionException = grpcRequest.ReactToConnectionFailure();
					AdvanceRequest();
					await ExecuteAsyncInternal(client, attempt + 1, connectionException, returnFuture, timeoutTime - DateTimeOffset.UtcNow);
					return;
				}

				// Execute the gRPC call
				TProtoResponse response;
				try
				{
					response = await grpcRequest
						.CreateCall()
						.AsyncUnaryCall(GetMethod(), null, grpcRequest.CallOptions, GetRequestForExecute())
						.ResponseAsync;
					
					LogTransaction(TransactionIdInternal, client, grpcRequest.Node, true, attempt, response, null);
				}
				catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.DeadlineExceeded)
				{
					returnFuture.TrySetException(new TimeoutException());
					return;
				}
				catch (Exception e)
				{
					lastException = e;
					
					LogTransaction(TransactionIdInternal, client, grpcRequest.Node, true, attempt, default, e);
					
					if (grpcRequest.ShouldRetryExceptionally(lastException))
					{
						AdvanceRequest();
						await ExecuteAsyncInternal(client, attempt + 1, lastException, returnFuture, timeoutTime - DateTimeOffset.UtcNow);
						return;
					}
					returnFuture.TrySetException(e);
					return;
				}

				// Process response status
				var status = MapResponseStatus(response as Proto.Response);
				var executionState = GetExecutionState(status, response);
				grpcRequest.HandleResponse(response, status, executionState, client);

				switch (executionState)
				{
					case ExecutionState.Retry:
						if (status == ResponseStatus.InvalidNodeAccount)
						{
							Logger?.Trace("Received INVALID_NODE_ACCOUNT; updating address book and marking node {0} as unhealthy, attempt #{1}", grpcRequest.Node.AccountId, attempt);
							UpdateNetworkFromAddressBook(client);
							client.Network_.IncreaseBackoff(grpcRequest.Node);
						}

						await new Delayer(client.Executor).DelayAsync(attempt < MaxAttempts ? grpcRequest.Delay : TimeSpan.Zero, () => { });
						await ExecuteAsyncInternal(client, attempt + 1, grpcRequest.MapStatusException(), returnFuture, timeoutTime - DateTimeOffset.UtcNow);
						break;

					case ExecutionState.ServerError:
						AdvanceRequest();
						await ExecuteAsyncInternal(client, attempt + 1, grpcRequest.MapStatusException(), returnFuture, timeoutTime - DateTimeOffset.UtcNow);
						break;

					case ExecutionState.RequestError:
						returnFuture.TrySetException(grpcRequest.MapStatusException());
						break;

					case ExecutionState.Success:
					default:
						returnFuture.TrySetResult(grpcRequest.MapResponse());
						break;
				}
			}
			catch (Exception ex)
			{
				returnFuture.TrySetException(ex);
			}
		}

		/// <include file="Executable.cs.xml" path='docs/member[@name="M:Executable.UpdateNetworkFromAddressBook(Client)"]/*' />
		private void UpdateNetworkFromAddressBook(Client client)
		{
			try
			{
				if (client.MirrorNetwork_?.Network.Count > 0)
					client.UpdateNetworkFromAddressBook();
			}
			catch (Exception updateError)
			{
				Logger?.Trace("failed to update client address book after INVALID_NODE_ACCOUNT_ID: {}", updateError.Message);
			}
		}	
	}
}