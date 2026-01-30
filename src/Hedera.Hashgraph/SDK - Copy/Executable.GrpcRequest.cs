// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Annotations;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.FutureConverter;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions.Account;
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
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Grpc.Core.Metadata;
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
    internal abstract partial class Executable<SdkRequestT, ProtoRequestT, ResponseT, O> 
	{
		private class GrpcRequest
		{
			private readonly Network network;
			private readonly int attempt;
			// private final ClientCall<ProtoRequestT, ResponseT> call;
			private readonly ProtoRequestT request;
			private readonly long startAt;
			
			private ResponseT response;
			private double latency;
			private Status responseStatus;

			public GrpcRequest(Network network, int attempt, Duration grpcDeadline)
			{
				network = network;
				attempt = attempt;
				grpcDeadline = grpcDeadline;
				Node = GetNodeForExecute(attempt);
				request = GetRequestForExecute(); // node index gets incremented here
				startAt = System.NanoTime();

				// Exponential back-off for Delayer: 250ms, 500ms, 1s, 2s, 4s, 8s, ... 8s
				Delay = (long)Math.Min(ArgumentNullException.ThrowIfNull(minBackoff).ToMillis() * Math.Pow(2, attempt - 1), ArgumentNullException.ThrowIfNull(maxBackoff).ToMillis());
			}

			private readonly Node Node { get; }
			private readonly long Delay { get; }
			private Duration GrpcDeadline { set; }
			public virtual CallOptions CallOptions 
			{
				get 
				{
					long deadline = Math.Min(grpcDeadline.ToMillis(), grpcDeadline.ToMillis());
					return CallOptions.DEFAULT.WithDeadlineAfter(deadline, TimeUnit.MILLISECONDS);
				}
			}
			public virtual ClientCall<ProtoRequestT, ResponseT> CreateCall()
			{
				VerboseLog(Node);

				return Node.GetChannel().NewCall(GetMethodDescriptor(), GetCallOptions());
			}

			public virtual ProtoRequestT GetRequest()
			{
				return requestListener.Apply(request);
			}



			public virtual Exception ReactToConnectionFailure()
			{
				ArgumentNullException.ThrowIfNull(network).IncreaseBackoff(Node);
				Loger.Warn("Retrying in {} ms after channel connection failure with node {} during attempt #{}", Node.GetRemainingTimeForBackoff(), Node.AccountId, attempt);
				VerboseLog(Node);
				return new InvalidOperationException("Failed to connect to node " + Node.AccountId);
			}

			public virtual bool ShouldRetryExceptionally(Exception e)
			{
				latency = (double)(System.NanoTime() - startAt) / 1000000000;
				var retry = ShouldRetryExceptionally(e);
				if (retry)
				{
					ArgumentNullException.ThrowIfNull(network).IncreaseBackoff(Node);
					logger.Warn("Retrying in {} ms after failure with node {} during attempt #{}: {}", Node.GetRemainingTimeForBackoff(), Node.AccountId, attempt, e != null ? e.GetMessage() : "NULL");
					VerboseLog(Node);
				}

				return retry;
			}

			public virtual PrecheckStatusException MapStatusException()
			{
				// request to hedera failed in a non-recoverable way
				return new PrecheckStatusException(responseStatus, GetTransactionIdInternal());
			}

			public virtual O MapResponse()
			{
				// successful response from Hedera
				return MapResponse(response, Node.AccountId, request);
			}

			virtual void HandleResponse(Proto.ResponseT response, Status status, ExecutionState executionState, Client client)
			{

				// Note: For INVALID_NODE_ACCOUNT, we don't mark the node as unhealthy here
				// because we need to do it AFTER advancing the request, to match Go SDK behavior
				if (status != Status.INVALID_NODE_ACCOUNT)
				{
					Node.DecreaseBackoff();
				}

				response = responseListener.Apply(response);
				responseStatus = status;
				logger.Trace("Received {} response in {} s from node {} during attempt #{}: {}", responseStatus, latency, Node.AccountId, attempt, response);
				if (executionState == ExecutionState.SERVER_ERROR && attemptedAllNodes)
				{
					executionState = ExecutionState.RETRY;
					attemptedAllNodes = false;
				}

				switch (executionState)
				{
					case ExecutionState.Retry:
						{
							logger.Warn("Retrying in {} ms after failure with node {} during attempt #{}: {}", Delay, Node.AccountId, attempt, responseStatus);
							VerboseLog(Node);
						}

					case ExecutionState.ServerError:
						{

							// Note: INVALID_NODE_ACCOUNT is handled after advanceRequest() in execute methods
							// to match Go SDK's executionStateRetryWithAnotherNode behavior
							if (status != Status.INVALID_NODE_ACCOUNT)
							{
								logger.Warn("Problem submitting request to node {} for attempt #{}, retry with new node: {}", Node.AccountId, attempt, responseStatus);
								VerboseLog(Node);
							}
						}

					default:
						{
						}

						break;
				}
			}

			public virtual void VerboseLog(Node node)
			{
				logger.Trace("Node IP {0} Timestamp {1} Transaction Type {2}", node.Address?.Address ?? "NULL", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), GetType().Name);
			}
		}
	}
}