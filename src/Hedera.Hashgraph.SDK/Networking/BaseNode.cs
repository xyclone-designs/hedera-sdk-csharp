// SPDX-License-Identifier: Apache-2.0
using Grpc.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <include file="BaseNode.cs.xml" path='docs/member[@name="T:BaseNode"]/*' />
	public abstract partial class BaseNode<N, KeyT>: IDisposable where N : BaseNode<N, KeyT>
    {
        private static readonly int GET_STATE_INTERVAL_MILLIS = 50;
        private static readonly int GET_STATE_TIMEOUT_MILLIS = 10000;
        private static readonly int GET_STATE_MAX_ATTEMPTS = GET_STATE_TIMEOUT_MILLIS / GET_STATE_INTERVAL_MILLIS;

		private Channel? channel = null;
		private bool hasConnected = false;
        
		protected ExecutorService Executor;

		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.#ctor(N,BaseNodeAddress)"]/*' />
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
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.#ctor(BaseNodeAddress,ExecutorService)"]/*' />
		internal BaseNode(BaseNodeAddress address, ExecutorService executor)
        {
            Executor = executor;
            Address = address;
			ReadmitTime = DateTime.UnixEpoch;
			CurrentBackoff = Client.DEFAULT_MIN_NODE_BACKOFF;
            MinBackoff = Client.DEFAULT_MIN_NODE_BACKOFF;
            MaxBackoff = Client.DEFAULT_MAX_NODE_BACKOFF;
        }


        /// <include file="BaseNode.cs.xml" path='docs/member[@name="P:BaseNode.Key"]/*' />
        public abstract KeyT Key { get; }

		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.lock(this)"]/*' />
		public virtual string? Authority
		{
			get => "127.0.0.1";
		}
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.lock(this)_2"]/*' />
		public virtual Channel Channel
		{
			get
			{
				lock (this)
				{
					if (channel != null)
						return channel;

					if (Address.IsInProcess)
					{
						// InProcessChannelBuilder has no direct Grpc.Core equivalent.
						// Assuming external logic handles this.
						throw new NotSupportedException("InProcess channels not supported in Grpc.Core.");
					}

					channel = Address.IsTransportSecurity
						? new Channel(Address.ToString(), GetChannelCredentials())
						: new Channel(Address.ToString(), ChannelCredentials.Insecure);

					return channel;
				}
			}
		}
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="P:BaseNode.ReadmitTime"]/*' />
		public DateTimeOffset ReadmitTime { get; set; }

		/// <include file="BaseNode.cs.xml" path='docs/member[@name="P:BaseNode.CurrentBackoff"]/*' />
		public TimeSpan CurrentBackoff { get; set; }
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.lock(this)_3"]/*' />
		public virtual TimeSpan MinBackoff 
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
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="P:BaseNode.MaxBackoff"]/*' />
		public virtual TimeSpan MaxBackoff { get; set; }
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="P:BaseNode.Address"]/*' />
		public virtual BaseNodeAddress Address { get; protected set; }
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="P:BaseNode.BadGrpcStatusCount"]/*' />
		public virtual long BadGrpcStatusCount { get; protected set; }
		
		private async Task<bool> ChannelFailedToConnectAsync(int i, ChannelState state)
		{
			hasConnected = (state == ChannelState.Ready);

			if (i >= GET_STATE_MAX_ATTEMPTS || hasConnected)
			{
				return await Task.FromResult(!hasConnected);
			}

			return await await new Delayer(Executor)
				.DelayAsync(TimeSpan.FromMilliseconds(GET_STATE_INTERVAL_MILLIS), () => ChannelFailedToConnectAsync(i + 1, Channel.State));
		}

		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.ChannelFailedToConnect"]/*' />
		public virtual bool ChannelFailedToConnect()
		{
			return ChannelFailedToConnect(DateTime.MaxValue);
		}
		public virtual bool ChannelFailedToConnect(DateTimeOffset timeoutTime)
		{
			if (hasConnected)
			{
				return false;
			}

			hasConnected = Channel.State == ChannelState.Ready;

			try
			{
				for (int i = 0; i < GET_STATE_MAX_ATTEMPTS && !hasConnected; i++)
				{
					TimeSpan remaining = timeoutTime - DateTime.UtcNow;

					if (remaining <= TimeSpan.Zero) return false;

					Thread.Sleep(GET_STATE_INTERVAL_MILLIS);

					hasConnected = Channel.State == ChannelState.Ready;
				}
			}
			catch (ThreadInterruptedException e)
			{
				throw new Exception(string.Empty, e);
			}

			return !hasConnected;
		}
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.ChannelFailedToConnectAsync"]/*' />
		public virtual Task<bool> ChannelFailedToConnectAsync()
		{
			if (hasConnected)
			{
				return Task.FromResult(false);
			}

			return ChannelFailedToConnectAsync(0, Channel.State);
		}
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.IsHealthy"]/*' />
		public virtual bool IsHealthy()
        {
            return ReadmitTime.ToUnixTimeMilliseconds() < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        /// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.IncreaseBackoff"]/*' />
        public virtual void IncreaseBackoff()
        {
            lock (this)
            {
				BadGrpcStatusCount++;
				ReadmitTime = DateTime.UtcNow + CurrentBackoff;

				CurrentBackoff *= 2;

				if (CurrentBackoff > MaxBackoff)
					CurrentBackoff = MaxBackoff;
			}
        }
        /// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.DecreaseBackoff"]/*' />
        public virtual void DecreaseBackoff()
        {
            lock (this)
            {
				CurrentBackoff /= 2;

				if (CurrentBackoff < MinBackoff)
					CurrentBackoff = MinBackoff;
            }
        }
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.GetRemainingTimeForBackoff"]/*' />
		public virtual long GetRemainingTimeForBackoff()
        {
            return ReadmitTime.ToUnixTimeMilliseconds() - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        /// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.GetChannelCredentials"]/*' />
        public virtual ChannelCredentials GetChannelCredentials()
        {
            return new SslCredentials();
        }
		/// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.UnhealthyBackoffRemaining"]/*' />
		public virtual long UnhealthyBackoffRemaining()
		{
			return Math.Max(0, ReadmitTime.ToUnixTimeMilliseconds() - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
		}
        /// <include file="BaseNode.cs.xml" path='docs/member[@name="M:BaseNode.Dispose(System.TimeSpan)"]/*' />
        public virtual void Dispose(TimeSpan timeout)
        {
            lock (this)
            {
				channel?.ShutdownAsync().Wait(timeout);
				channel = null;
			}
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
		public void ChannelReset() { channel = null; }
    }
}