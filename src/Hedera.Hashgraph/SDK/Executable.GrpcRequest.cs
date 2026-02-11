// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Networking;

using System;
using System.Diagnostics;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Abstract base utility class.
    /// </summary>
    /// <param name="<SdkRequestT>">the sdk request</param>
    /// <param name="<ProtoRequestT>">the proto request</param>
    /// <param name="<ResponseT>">the response</param>
    /// <param name="<O>">the O type</param>
    public abstract partial class Executable<SdkRequestT, ProtoRequestT, ResponseT, O> 
	{
		public class GrpcRequest
		{
			public GrpcRequest(Executable<SdkRequestT, ProtoRequestT, ResponseT, O> parent, Network? network, int attempt, Duration grpcDeadline)
			{
				Parent = parent;
				Network = network;
				Attempt = attempt;
				GrpcDeadline = grpcDeadline;
				Node = parent.GetNodeForExecute(attempt);
				Request = Parent.GetRequestForExecute(); // node index gets incremented here
				StartAt = Stopwatch.GetTimestamp();

				// Exponential back-off for Delayer: 250ms, 500ms, 1s, 2s, 4s, 8s, ... 8s
				Delay = (long)Math.Min(Parent.MinBackoff.ToTimeSpan().TotalMilliseconds * Math.Pow(2, attempt - 1), Parent.MaxBackoff.ToTimeSpan().TotalMilliseconds);
			}

			private readonly Executable<SdkRequestT, ProtoRequestT, ResponseT, O> Parent;
			private readonly Network? Network;
			private readonly int Attempt;
			private readonly ProtoRequestT Request;
			private readonly long StartAt;

			private ResponseT? Response;
			private double Latency;
			private ResponseStatus ResponseStatus;

			public readonly long Delay;
			public readonly Node Node;

			public Duration GrpcDeadline { get; set; }

			public virtual CallOptions CallOptions 
			{
				get 
				{
					double deadline = Math.Min(GrpcDeadline.ToTimeSpan().TotalMilliseconds, GrpcDeadline.ToTimeSpan().TotalMilliseconds);

					return CallOptions.WithDeadline(DateTime.Now.AddMilliseconds(deadline));
				}
			}
			public virtual ClientCall<ProtoRequestT, ResponseT> CreateCall()
			{
				VerboseLog(Node);

				return Node.Channel.NewCall(Parent.GetMethodDescriptor(), CallOptions);
			}

			public virtual ProtoRequestT GetRequest()
			{
				return Parent.RequestListener.Invoke(Request);
			}
			public virtual O MapResponse()
			{
				// successful response from Hedera
				return MapResponse(Response, Node.AccountId, Request);
			}
			public virtual Exception ReactToConnectionFailure()
			{
				Network?.IncreaseBackoff(Node);
				Parent.Logger.Warn("Retrying in {} ms after channel connection failure with node {} during attempt #{}", Node.GetRemainingTimeForBackoff(), Node.AccountId, Attempt);
				VerboseLog(Node);
				return new InvalidOperationException("Failed to connect to node " + Node.AccountId);
			}		
			public virtual PrecheckStatusException MapStatusException()
			{
				// request to hedera failed in a non-recoverable way
				return new PrecheckStatusException(ResponseStatus, Parent.TransactionIdInternal);
			}
			public virtual void VerboseLog(Node node)
			{
				Parent.Logger?.Trace("Node IP {0} Timestamp {1} Transaction Type {2}", node.Address?.Address ?? "NULL", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), GetType().Name);
			}
			public virtual bool ShouldRetryExceptionally(Exception e)
			{
				Latency = (double)(Stopwatch.GetTimestamp() - StartAt) / 1000000000;
				var retry = ShouldRetryExceptionally(e);
				if (retry)
				{
					Network?.IncreaseBackoff(Node);
					Parent.Logger.Warn("Retrying in {} ms after failure with node {} during attempt #{}: {}", Node.GetRemainingTimeForBackoff(), Node.AccountId, Attempt, e != null ? e.Message : "NULL");
					VerboseLog(Node);
				}

				return retry;
			}
			public virtual void HandleResponse(ResponseT responseT, ResponseStatus status, ExecutionState executionState, Client? client)
			{

				// Note: For INVALID_NODE_ACCOUNT, we don't mark the node as unhealthy here
				// because we need to do it AFTER advancing the request, to match Go SDK behavior
				if (status != ResponseStatus.InvalidNodeAccount)
				{
					Node.DecreaseBackoff();
				}

				Response = Parent.ResponseListener.Invoke(responseT);
				ResponseStatus = status;
				Parent.Logger.Trace("Received {} response in {} s from node {} during attempt #{}: {}", ResponseStatus, Latency, Node.AccountId, Attempt, Response);
				
				if (executionState == ExecutionState.ServerError && Parent.AttemptedAllNodes)
				{
					executionState = ExecutionState.Retry;
					Parent.AttemptedAllNodes = false;
				}

				switch (executionState)
				{
					case ExecutionState.Retry:
						Parent.Logger.Warn("Retrying in {} ms after failure with node {} during attempt #{}: {}", Delay, Node.AccountId, Attempt, ResponseStatus);
						VerboseLog(Node);
						break;

					case ExecutionState.ServerError:
						// Note: INVALID_NODE_ACCOUNT is handled after advanceRequest() in execute methods
						// to match Go SDK's executionStateRetryWithAnotherNode behavior
						if (status != ResponseStatus.InvalidNodeAccount)
						{
							Parent.Logger?.Warn("Problem submitting request to node {} for attempt #{}, retry with new node: {}", Node.AccountId, Attempt, ResponseStatus);
							VerboseLog(Node);
						}
						break;

					default: break;
				}
			}
		}
	}
}