// SPDX-License-Identifier: Apache-2.0
using Io.Grpc;
using Io.Grpc.Inprocess;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
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
    /// Internal utility class.
    /// </summary>
    /// <param name="<N>">the n type</param>
    /// <param name="<KeyT>">the key t type</param>
    abstract class BaseNode<N, KeyT> where N : BaseNode<N, KeyT>
    {
        private static readonly int GET_STATE_INTERVAL_MILLIS = 50;
        private static readonly int GET_STATE_TIMEOUT_MILLIS = 10000;
        private static readonly int GET_STATE_MAX_ATTEMPTS = GET_STATE_TIMEOUT_MILLIS / GET_STATE_INTERVAL_MILLIS;
        private bool hasConnected = false;
        protected readonly ExecutorService executor;
        /// <summary>
        /// Address of this node
        /// </summary>
        protected readonly BaseNodeAddress address;
        /// <summary>
        /// Timestamp of when this node will be considered healthy again
        /// </summary>
        protected Instant readmitTime;
        /// <summary>
        /// The current backoff duration. Uses exponential backoff so think 1s, 2s, 4s, 8s, etc until maxBackoff is hit
        /// </summary>
        protected Duration currentBackoff;
        /// <summary>
        /// Minimum backoff used by node when receiving a bad gRPC status
        /// </summary>
        protected Duration minBackoff;
        /// <summary>
        /// Maximum backoff used by node when receiving a bad gRPC status
        /// </summary>
        protected Duration maxBackoff;
        /// <summary>
        /// Number of times this node has received a bad gRPC status
        /// </summary>
        protected long badGrpcStatusCount;
        protected ManagedChannel channel = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">the node address</param>
        /// <param name="executor">the client</param>
        protected BaseNode(BaseNodeAddress address, ExecutorService executor)
        {
            executor = executor;
            address = address;
            currentBackoff = Client.DEFAULT_MIN_NODE_BACKOFF;
            minBackoff = Client.DEFAULT_MIN_NODE_BACKOFF;
            maxBackoff = Client.DEFAULT_MAX_NODE_BACKOFF;
            readmitTime = Instant.EPOCH;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node">the node object</param>
        /// <param name="address">the address to assign</param>
        protected BaseNode(N node, BaseNodeAddress address)
        {
            address = address;
            executor = node.executor;
            minBackoff = node.minBackoff;
            maxBackoff = node.maxBackoff;
            readmitTime = node.readmitTime;
            currentBackoff = node.currentBackoff;
            badGrpcStatusCount = node.badGrpcStatusCount;
        }

        /// <summary>
        /// Return the local host ip address
        /// </summary>
        /// <returns>                         the authority address</returns>
        protected virtual string GetAuthority()
        {
            return "127.0.0.1";
        }

        /// <summary>
        /// Extract the key list
        /// </summary>
        /// <returns>                         the key list</returns>
        abstract KeyT GetKey();
        /// <summary>
        /// Get the address of this node
        /// </summary>
        /// <returns>                         the address for the node</returns>
        virtual BaseNodeAddress GetAddress()
        {
            return address;
        }

        /// <summary>
        /// Get the minimum backoff time
        /// </summary>
        /// <returns>                         the minimum backoff time</returns>
        virtual Duration GetMinBackoff()
        {
            lock (this)
            {
                return minBackoff;
            }
        }

        /// <summary>
        /// Set the minimum backoff tim
        /// </summary>
        /// <param name="minBackoff">the minimum backoff time</param>
        /// <returns>{@code this}</returns>
        virtual N SetMinBackoff(Duration minBackoff)
        {
            lock (this)
            {
                if (currentBackoff == minBackoff)
                {
                    currentBackoff = minBackoff;
                }

                minBackoff = minBackoff;

                // noinspection unchecked
                return (N)this;
            }
        }

        /// <summary>
        /// Get the maximum backoff time
        /// </summary>
        /// <returns>                         the maximum backoff time</returns>
        virtual Duration GetMaxBackoff()
        {
            return maxBackoff;
        }

        /// <summary>
        /// Set the maximum backoff time
        /// </summary>
        /// <param name="maxBackoff">the max backoff time</param>
        /// <returns>{@code this}</returns>
        virtual N SetMaxBackoff(Duration maxBackoff)
        {
            maxBackoff = maxBackoff;

            // noinspection unchecked
            return (N)this;
        }

        /// <summary>
        /// Get the number of times this node has received a bad gRPC status
        /// </summary>
        /// <returns>                         the count of bad grpc status</returns>
        virtual long GetBadGrpcStatusCount()
        {
            return badGrpcStatusCount;
        }

        /// <summary>
        /// Extract the unhealthy backoff time remaining.
        /// </summary>
        /// <returns>                         the unhealthy backoff time remaining</returns>
        virtual long UnhealthyBackoffRemaining()
        {
            return Math.Max(0, readmitTime.ToEpochMilli() - System.CurrentTimeMillis());
        }

        /// <summary>
        /// Determines if this is node is healthy.
        /// Healthy means the node has either not received any bad gRPC statuses, or if it has received bad gRPC status then
        /// the node backed off for a period of time.
        /// </summary>
        /// <returns>                         is the node healthy</returns>
        virtual bool IsHealthy()
        {
            return readmitTime.ToEpochMilli() < Instant.Now().ToEpochMilli();
        }

        /// <summary>
        /// Used when a node has received a bad gRPC status
        /// </summary>
        virtual void IncreaseBackoff()
        {
            lock (this)
            {
                badGrpcStatusCount++;
                readmitTime = Instant.Now().Plus(currentBackoff);
                currentBackoff = currentBackoff.MultipliedBy(2);
                currentBackoff = currentBackoff.CompareTo(maxBackoff) < 0 ? currentBackoff : maxBackoff;
            }
        }

        /// <summary>
        /// Used when a node has not received a bad gRPC status.
        /// This means on each request that doesn't get a bad gRPC status the current backoff will be lowered. The point of
        /// this is to allow a node which has been performing poorly (receiving several bad gRPC status) to become used again
        /// once it stops receiving bad gRPC statuses.
        /// </summary>
        virtual void DecreaseBackoff()
        {
            lock (this)
            {
                currentBackoff = currentBackoff.DividedBy(2);
                currentBackoff = currentBackoff.CompareTo(minBackoff) > 0 ? currentBackoff : minBackoff;
            }
        }

        /// <summary>
        /// Get the amount of time the node has to wait until it's healthy again
        /// </summary>
        /// <returns>                         remaining back off time</returns>
        virtual long GetRemainingTimeForBackoff()
        {
            return readmitTime.ToEpochMilli() - System.CurrentTimeMillis();
        }

        /// <summary>
        /// Create TLS credentials when transport security is enabled
        /// </summary>
        /// <returns>                         the channel credentials</returns>
        virtual ChannelCredentials GetChannelCredentials()
        {
            return TlsChannelCredentials.Create();
        }

        /// <summary>
        /// Get the gRPC channel for this node
        /// </summary>
        /// <returns>                         the channel</returns>
        virtual ManagedChannel GetChannel()
        {
            lock (this)
            {
                if (channel != null)
                {
                    return channel;
                }

                ManagedChannelBuilder<TWildcardTodo> channelBuilder;
                if (address.IsInProcess())
                {
                    channelBuilder = InProcessChannelBuilder.ForName(Objects.RequireNonNull(address.GetName()));
                }
                else if (address.IsTransportSecurity())
                {
                    channelBuilder = Grpc.NewChannelBuilder(address.ToString(), GetChannelCredentials());
                    string authority = GetAuthority();
                    if (authority != null)
                    {
                        channelBuilder = channelBuilder.OverrideAuthority(authority);
                    }
                }
                else
                {
                    channelBuilder = ManagedChannelBuilder.ForTarget(address.ToString()).UsePlaintext();
                }

                channel = channelBuilder.KeepAliveTimeout(10, TimeUnit.SECONDS).KeepAliveWithoutCalls(true).Intercept(new MetadataInterceptor()).EnableRetry().Executor(executor).Build();
                return channel;
            }
        }

        /// <summary>
        /// Did we fail to connect?
        /// </summary>
        /// <returns>                         did we fail to connect</returns>
        virtual bool ChannelFailedToConnect()
        {
            return ChannelFailedToConnect(Instant.MAX);
        }

        virtual bool ChannelFailedToConnect(Instant timeoutTime)
        {
            if (hasConnected)
            {
                return false;
            }

            hasConnected = (GetChannel().GetState(true) == ConnectivityState.READY);
            try
            {
                for (int i = 0; i < GET_STATE_MAX_ATTEMPTS && !hasConnected; i++)
                {
                    Duration currentTimeout = Duration.Between(Instant.Now(), timeoutTime);
                    if (currentTimeout.IsNegative() || currentTimeout.IsZero())
                    {
                        return false;
                    }

                    TimeUnit.MILLISECONDS.Sleep(GET_STATE_INTERVAL_MILLIS);
                    hasConnected = (GetChannel().GetState(true) == ConnectivityState.READY);
                }
            }
            catch (InterruptedException e)
            {
                throw new Exception(e);
            }

            return !hasConnected;
        }

        private CompletableFuture<bool> ChannelFailedToConnectAsync(int i, ConnectivityState state)
        {
            hasConnected = (state == ConnectivityState.READY);
            if (i >= GET_STATE_MAX_ATTEMPTS || hasConnected)
            {
                return CompletableFuture.CompletedFuture(!hasConnected);
            }

            return Delayer.DelayFor(GET_STATE_INTERVAL_MILLIS, executor).ThenCompose((ignored) =>
            {
                return ChannelFailedToConnectAsync(i + 1, GetChannel().GetState(true));
            });
        }

        /// <summary>
        /// Asynchronously determine if the channel failed to connect.
        /// </summary>
        /// <returns>                         did we fail to connect</returns>
        virtual CompletableFuture<bool> ChannelFailedToConnectAsync()
        {
            if (hasConnected)
            {
                return CompletableFuture.CompletedFuture(false);
            }

            return ChannelFailedToConnectAsync(0, GetChannel().GetState(true));
        }

        /// <summary>
        /// Close the current nodes channel
        /// </summary>
        /// <param name="timeout">the timeout value</param>
        /// <exception cref="InterruptedException">thrown when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
        virtual void Dispose(Duration timeout)
        {
            lock (this)
            {
                if (channel != null)
                {
                    channel.Shutdown();
                    channel.AwaitTermination(timeout.GetSeconds(), TimeUnit.SECONDS);
                    channel = null;
                }
            }
        }

        /// <summary>
        /// Metadata interceptor for the client.
        /// This interceptor adds the user agent header to the request.
        /// </summary>
        class MetadataInterceptor : ClientInterceptor
        {
            private readonly Metadata metadata;
            public MetadataInterceptor()
            {
                metadata = new Metadata();
                Metadata.Key<String> authKey = Metadata.Key.Of("x-user-agent", Metadata.ASCII_STRING_MARSHALLER);
                metadata.Put(authKey, GetUserAgent());
            }

            public virtual ClientCall<ReqT, RespT> InterceptCall<ReqT, RespT>(MethodDescriptor<ReqT, RespT> method, CallOptions callOptions, Channel next)
            {
                ClientCall<ReqT, RespT> call = next.NewCall(method, callOptions);
                return new AnonymousSimpleForwardingClientCall(call);
            }

            private sealed class AnonymousSimpleForwardingClientCall : SimpleForwardingClientCall
            {
                public AnonymousSimpleForwardingClientCall(MetadataInterceptor parent)
                {
                    parent = parent;
                }

                private readonly MetadataInterceptor parent;
                public void Start(Listener<RespT> responseListener, Metadata headers)
                {
                    headers.Merge(metadata);
                    base.Start(responseListener, headers);
                }
            }

            /// <summary>
            /// Extract the user agent. This information is used to gather usage metrics.
            /// If the version is not available, the user agent will be set to "hiero-sdk-java/DEV".
            /// </summary>
            private string GetUserAgent()
            {
                var thePackage = GetType().GetPackage();
                var implementationVersion = thePackage != null ? thePackage.GetImplementationVersion() : null;
                return "hiero-sdk-java/" + ((implementationVersion != null) ? (implementationVersion) : "DEV");
            }
        }
    }
}