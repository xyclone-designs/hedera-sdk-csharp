// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <summary>
	/// Internal utility class.
	/// </summary>
	/// <param name="<N>">the n type</param>
	/// <param name="<KeyT>">the key t type</param>
	public abstract partial class BaseNode<N, KeyT> where N : BaseNode<N, KeyT>
    {
        private static readonly int GET_STATE_INTERVAL_MILLIS = 50;
        private static readonly int GET_STATE_TIMEOUT_MILLIS = 10000;
        private static readonly int GET_STATE_MAX_ATTEMPTS = GET_STATE_TIMEOUT_MILLIS / GET_STATE_INTERVAL_MILLIS;

        private bool hasConnected = false;
        protected readonly ExecutorService executor;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="node">the node object</param>
		/// <param name="address">the address to assign</param>
		internal BaseNode(N node, BaseNodeAddress address)
		{
			Address = address;
			Executor = node.Executor;
			MinBackoff = node.MinBackoff;
			MaxBackoff = node.MaxBackoff;
			ReadmitTime = node.ReadmitTime;
			CurrentBackoff = node.CurrentBackoff;
			BadGrpcStatusCount = node.BadGrpcStatusCount;
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="address">the node address</param>
		/// <param name="executor">the client</param>
		internal BaseNode(BaseNodeAddress address, ExecutorService executor)
        {
            Executor = executor;
            Address = address;
			ReadmitTime = Instant.EPOCH;
			CurrentBackoff = Client.DEFAULT_MIN_NODE_BACKOFF;
            MinBackoff = Client.DEFAULT_MIN_NODE_BACKOFF;
            MaxBackoff = Client.DEFAULT_MAX_NODE_BACKOFF;
        }


        /// <summary>
        /// Extract the key list
        /// </summary>
        /// <returns>                         the key list</returns>
        public abstract KeyT Key { get; }

		/// <summary>
		/// Return the local host ip address
		/// </summary>
		/// <returns>                         the authority address</returns>
		public virtual string? Authority
		{
			get => "127.0.0.1";
		}
		/// <summary>
		/// Get the gRPC channel for this node
		/// </summary>
		/// <returns>                         the channel</returns>
		public virtual MergedChannel Channel
		{
			get
			{
				lock (this)
				{
					if (field != null)
						return field;

					ManagedChannelBuilder<T> channelBuilder;

					if (Address.IsInProcess)
						channelBuilder = InProcessChannelBuilder.ForName(ArgumentNullException.ThrowIfNull(Address.Name));
					else if (Address.IsTransportSecurity)
					{
						channelBuilder = Grpc.NewChannelBuilder(Address.ToString(), GetChannelCredentials());
						
						if (Authority != null)
							channelBuilder = channelBuilder.OverrideAuthority(Authority);
					}
					else channelBuilder = ManagedChannelBuilder.ForTarget(Address.ToString()).UsePlaintext();

					field = channelBuilder
						.KeepAliveTimeout(10, TimeUnit.SECONDS)
						.KeepAliveWithoutCalls(true)
						.Intercept(new MetadataInterceptor())
						.EnableRetry()
						.Executor(executor).Build();
					
					return field;
				}
			}
		}
		/// <summary>
		/// Timestamp of when this node will be considered healthy again
		/// </summary>
		public Instant ReadmitTime { get; set; }

		/// <summary>
		/// The current backoff duration. Uses exponential backoff so think 1s, 2s, 4s, 8s, etc until maxBackoff is hit
		/// </summary>
		public Duration CurrentBackoff { get; set; }
		/// <summary>
		/// Minimum backoff used by node when receiving a bad gRPC status
		/// </summary>
		public virtual Duration MinBackoff 
        {
            get
            {
				lock (this)
				{
					return field;
				}
			}
            set
            {
				lock (this)
				{
					if (CurrentBackoff == value)
						CurrentBackoff = field;

					field = value;
				}
			}
        }
		/// <summary>
		/// Get the maximum backoff time
		/// </summary>
		/// <returns>                         the maximum backoff time</returns>
		public virtual Duration MaxBackoff { get; set; }
		/// <summary>
		/// Address of this node
		/// </summary>
		public virtual BaseNodeAddress Address { get; protected set; }
		/// <summary>
		/// Get the number of times this node has received a bad gRPC status
		/// </summary>
		public virtual long BadGrpcStatusCount { get; protected set; }
		

		private Task<bool> ChannelFailedToConnectAsync(int i, ConnectivityState state)
		{
			hasConnected = (state == ConnectivityState.Ready);

			if (i >= GET_STATE_MAX_ATTEMPTS || hasConnected)
			{
				return Task.FromResult(!hasConnected);
			}

			return Delayer.DelayFor(GET_STATE_INTERVAL_MILLIS, executor).ThenCompose((ignored) =>
			{
				return ChannelFailedToConnectAsync(i + 1, GetChannel().GetState(true));
			});
		}

		/// <summary>
		/// Did we fail to connect?
		/// </summary>
		/// <returns>                         did we fail to connect</returns>
		public virtual bool ChannelFailedToConnect()
		{
			return ChannelFailedToConnect(Instant.MAX);
		}
		public virtual bool ChannelFailedToConnect(Instant timeoutTime)
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

					hasConnected = Channel.GetState(true) == ConnectivityState.READY;
				}
			}
			catch (ThreadInterruptedException e)
			{
				throw new Exception(string.Empty, e);
			}

			return !hasConnected;
		}
		/// <summary>
		/// Asynchronously determine if the channel failed to connect.
		/// </summary>
		/// <returns>                         did we fail to connect</returns>
		public virtual Task<bool> ChannelFailedToConnectAsync()
		{
			if (hasConnected)
			{
				return Task.FromResult(false);
			}

			return ChannelFailedToConnectAsync(0, Channel.GetState(true));
		}
		/// <summary>
		/// Determines if this is node is healthy.
		/// Healthy means the node has either not received any bad gRPC statuses, or if it has received bad gRPC status then
		/// the node backed off for a period of time.
		/// </summary>
		/// <returns>                         is the node healthy</returns>
		public virtual bool IsHealthy()
        {
            return ReadmitTime.ToEpochMilli() < Instant.Now().ToEpochMilli();
        }
        /// <summary>
        /// Used when a node has received a bad gRPC status
        /// </summary>
        public virtual void IncreaseBackoff()
        {
            lock (this)
            {
                BadGrpcStatusCount++;
                ReadmitTime = Instant.Now().Plus(CurrentBackoff);
                CurrentBackoff = CurrentBackoff.MultipliedBy(2);
                CurrentBackoff = CurrentBackoff.CompareTo(MaxBackoff) < 0 ? CurrentBackoff : MaxBackoff;
            }
        }
        /// <summary>
        /// Used when a node has not received a bad gRPC status.
        /// This means on each request that doesn't get a bad gRPC status the current backoff will be lowered. The point of
        /// this is to allow a node which has been performing poorly (receiving several bad gRPC status) to become used again
        /// once it stops receiving bad gRPC statuses.
        /// </summary>
        public virtual void DecreaseBackoff()
        {
            lock (this)
            {
                CurrentBackoff = CurrentBackoff.DividedBy(2);
                CurrentBackoff = CurrentBackoff.CompareTo(MinBackoff) > 0 ? CurrentBackoff : MinBackoff;
            }
        }
		/// <summary>
		/// Get the amount of time the node has to wait until it's healthy again
		/// </summary>
		/// <returns>                         remaining back off time</returns>
		public virtual long GetRemainingTimeForBackoff()
        {
            return ReadmitTime.ToEpochMilli() - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        /// <summary>
        /// Create TLS credentials when transport security is enabled
        /// </summary>
        /// <returns>                         the channel credentials</returns>
        public virtual ChannelCredentials GetChannelCredentials()
        {
            return TlsChannelCredentials.Create();
        }
		/// <summary>
		/// Extract the unhealthy backoff time remaining.
		/// </summary>
		/// <returns>                         the unhealthy backoff time remaining</returns>
		public virtual long UnhealthyBackoffRemaining()
		{
			return Math.Max(0, ReadmitTime.ToEpochMilli() - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
		}
        /// <summary>
        /// Close the current nodes channel
        /// </summary>
        /// <param name="timeout">the timeout value</param>
        /// <exception cref="InterruptedException">thrown when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
        public virtual void Dispose(Duration timeout)
        {
            lock (this)
            {
                if (Channel != null)
                {
                    Channel.Shutdown();
                    Channel.AwaitTermination(timeout.Seconds, TimeUnit.SECONDS);
                    Channel = null;
                }
            }
        }
    }
}