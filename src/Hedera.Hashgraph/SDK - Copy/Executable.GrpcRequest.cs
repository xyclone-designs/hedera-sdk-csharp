// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Annotations;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.FutureConverter;
using Hedera.Hashgraph.SDK.Logger;
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
			public GrpcRequest(Network network, int attempt, Duration grpcDeadline)
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

			virtual Exception ReactToConnectionFailure()
			{
				Objects.RequireNonNull(network).IncreaseBackoff(node);
				logger.Warn("Retrying in {} ms after channel connection failure with node {} during attempt #{}", node.GetRemainingTimeForBackoff(), node.GetAccountId(), attempt);
				VerboseLog(node);
				return new InvalidOperationException("Failed to connect to node " + node.GetAccountId());
			}

			virtual bool ShouldRetryExceptionally(Exception e)
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