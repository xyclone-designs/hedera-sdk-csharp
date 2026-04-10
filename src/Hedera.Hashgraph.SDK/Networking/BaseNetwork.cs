// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="T:BaseNetwork"]/*' />
	public abstract class BaseNetwork<BaseNetworkT, KeyT, BaseNodeT>
        where BaseNetworkT : BaseNetwork<BaseNetworkT, KeyT, BaseNodeT> 
        where BaseNodeT : BaseNode<BaseNodeT, KeyT>
        where KeyT : notnull
	{
		internal BaseNetwork(ExecutorService executor)
		{
			Executor = executor;
		}
		
		private void RemoveNodeFromNetwork(BaseNodeT node)
		{
			var nodesForKey = Network[node.Key];
			nodesForKey.Remove(node);

			if (nodesForKey.Count == 0)
			{
				Network.Remove(node.Key);
			}
		}
		private bool AddressIsInNodeList(string addressString, IList<BaseNodeT> nodes)
		{
			var address = BaseNodeAddress.FromString(addressString);

			foreach (var node in nodes)
				if (node.Address.Equals(address))
					return true;

			return false;
		}
		private bool NodeIsInGivenNetwork(BaseNodeT node, Dictionary<string, KeyT> network)
		{
			foreach (KeyValuePair<string, KeyT> entry in network)
			{
				if (node.Key.Equals(entry.Value) && node.Address.Equals(BaseNodeAddress.FromString(entry.Key)))
				{
					return true;
				}
			}

			return false;
		}

		private bool HasShutDownNow = false;
		protected static readonly int DEFAULT_MAX_NODE_ATTEMPTS = -1;
        protected static readonly Random random = new ();
        protected readonly ExecutorService Executor;
		protected abstract BaseNodeT CreateNodeFromNetworkEntry(KeyValuePair<string, KeyT> entry);

		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.RemoveDeadNodes"]/*' />
		protected virtual void RemoveDeadNodes()
		{
			if (MaxNodeAttempts > 0)
			{
				for (int i = Nodes.Count - 1; i >= 0; i--)
				{
					var node = Nodes[i];

					if (Nodes[i].BadGrpcStatusCount >= MaxNodeAttempts)
					{
						node.Dispose(CloseTimeout);
						RemoveNodeFromNetwork(node);
						Nodes.RemoveAt(i);
					}
				}
			}
		}
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.GetNodesToRemove(System.Collections.Generic.Dictionary{System.String,KeyT})"]/*' />
		protected virtual IList<int> GetNodesToRemove(Dictionary<string, KeyT> network)
		{
			var _nodes = new List<int>(Nodes.Count);

			for (int i = Nodes.Count - 1; i >= 0; i--)
			{
				var node = Nodes[i];
				if (!NodeIsInGivenNetwork(node, network))
				{
					_nodes.Add(i);
				}
			}

			return _nodes;
		}
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.GetNumberOfMostHealthyNodes(System.Int32)"]/*' />
		protected virtual IList<BaseNodeT> GetNumberOfMostHealthyNodes(int count)
		{
			lock (this)
			{
				ReadmitNodes();
				RemoveDeadNodes();
				var returnNodes = new Dictionary<KeyT, BaseNodeT>(count);
				for (var i = 0; i < count; i++)
				{
					var node = RandomNode;

					returnNodes.TryAdd(node.Key, node);
				}

				var returnList = new List<BaseNodeT>();

				returnList.AddRange(returnNodes.Values);

				return returnList;
			}
		}

		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="P:BaseNetwork.Nodes"]/*' />
		public IList<BaseNodeT> Nodes { get; protected set; } = [];
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="P:BaseNetwork.HealthyNodes"]/*' />
		public IList<BaseNodeT> HealthyNodes { get; protected set; } = [];        
        /// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.AddSeconds(Client.DEFAULT_MIN_NODE_BACKOFF.)"]/*' />
        public DateTimeOffset EarliestReadmitTime { get; protected set; } = DateTimeOffset.UtcNow.AddSeconds(Client.DEFAULT_MIN_NODE_BACKOFF.TotalSeconds);

		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.lock(this)"]/*' />
		public virtual LedgerId? LedgerId
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }
		}
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.lock(this)_2"]/*' />
		public virtual int MaxNodeAttempts
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }

		} = DEFAULT_MAX_NODE_ATTEMPTS;
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.lock(this)_3"]/*' />
		public virtual TimeSpan MinNodeBackoff
		{
			get { lock (this) return field; }
			set 
            {
                lock (this)
                {
                    field = value;

                    foreach (var node in Nodes)
                        node.MinBackoff = field;
				} 
            }

		} = Client.DEFAULT_MIN_NODE_BACKOFF;
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.lock(this)_4"]/*' />
		public virtual TimeSpan MaxNodeBackoff
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					field = value;

					foreach (var node in Nodes)
						node.MaxBackoff = value;
				}
			}

		} = Client.DEFAULT_MAX_NODE_BACKOFF;
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.lock(this)_5"]/*' />
		public virtual TimeSpan MinNodeReadmitTime
		{
			get { lock (this) return field; }
			set 
            {
                lock (this)
                {
					field = value;

					foreach (var node in Nodes)
						node.ReadmitTime = DateTimeOffset.UtcNow;
				}
            }

		} = Client.DEFAULT_MIN_NODE_BACKOFF;
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.lock(this)_6"]/*' />
		public virtual TimeSpan MaxNodeReadmitTime
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }

		} = Client.DEFAULT_MAX_NODE_BACKOFF;
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.lock(this)_7"]/*' />
		public virtual TimeSpan CloseTimeout
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }

		} = Client.DEFAULT_CLOSE_TIMEOUT;
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="P:BaseNetwork.TransportSecurity"]/*' />
		public virtual bool TransportSecurity { get; set; }
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.lock(this)_8"]/*' />
		public virtual BaseNodeT RandomNode
		{
			get
			{
				lock (this)
				{

					// Attempt to readmit nodes each time a node is fetched.
					// Note: Readmitting nodes will only happen periodically so calling it each time should not harm
					// performance.
					ReadmitNodes();

					if (HealthyNodes.Count == 0)
					{
						throw new InvalidOperationException("No healthy node was found");
					}

					return HealthyNodes[random.Next(HealthyNodes.Count)];
				}
			}
		}
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="F:BaseNetwork.set"]/*' />
		public virtual Dictionary<KeyT, IList<BaseNodeT>> Network { protected get; set; } = [];
		public virtual ReadOnlyDictionary<KeyT, IList<BaseNodeT>> Network_Read
		{
			get => Network.AsReadOnly();
		}

		public virtual void BeginClose()
		{
			lock (this)
			{
				foreach (var node in Nodes)
					node.Channel?.ShutdownAsync().GetAwaiter().GetResult();
			}
		}
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.ReadmitNodes"]/*' />
		public virtual void ReadmitNodes()
		{
			lock (this)
			{
				var now = DateTimeOffset.UtcNow;
				if (now.ToUnixTimeSeconds() > EarliestReadmitTime.ToUnixTimeSeconds())
				{
					var nextEarliestReadmitTime = now.AddSeconds(MaxNodeReadmitTime.Seconds);
					foreach (var node in Nodes)
					{
						if (node.ReadmitTime.ToUnixTimeSeconds() > now.ToUnixTimeSeconds() && node.ReadmitTime.ToUnixTimeSeconds() < nextEarliestReadmitTime.ToUnixTimeSeconds())
						{
							nextEarliestReadmitTime = node.ReadmitTime;
						}
					}

					EarliestReadmitTime = nextEarliestReadmitTime;

					if (EarliestReadmitTime < now.Add(MinNodeReadmitTime))
						EarliestReadmitTime = now.Add(MinNodeReadmitTime);

					outer:
					for (var i = 0; i < Nodes.Count; i++)
					{

						// Check if `healthyNodes` already contains this node
						for (var j = 0; j < HealthyNodes.Count; j++)
							if (Nodes[i] == HealthyNodes[j])
								goto outer;

						// If `healthyNodes` doesn't contain the node, check the `readmitTime` on the node
						if (Nodes[i].ReadmitTime.ToUnixTimeSeconds() < now.ToUnixTimeSeconds())
							HealthyNodes.Add(Nodes[i]);
					}
				}
			}
		}
		public virtual void IncreaseBackoff(BaseNodeT node)
        {
            lock (this)
            {
                node.IncreaseBackoff();
                HealthyNodes.Remove(node);
            }
        }
        public virtual void DecreaseBackoff(BaseNodeT node)
        {
            lock (this)
            {
                node.DecreaseBackoff();
            }
        }
		public virtual Exception? AwaitClose(Timestamp deadline, Exception? previousError)
		{
			lock (this)
			{
				try
				{
					if (previousError != null)
					{
						throw previousError;
					}

					foreach (var node in Nodes)
					{
						if (node.Channel != null)
						{
							var timeoutMillis = (Timestamp.FromDateTime(DateTime.UtcNow) - deadline).ToTimeSpan().TotalMilliseconds;
							if (timeoutMillis <= 0 || node.Channel.ShutdownAsync().Wait(TimeSpan.FromMilliseconds(timeoutMillis)))
							{
								throw new TimeoutException("Failed to properly shutdown all channels");
							}
							else
							{
								node.ChannelReset();
							}
						}
					}

					return null;
				}
				catch (Exception error)
				{
					foreach (var node in Nodes)
						node.Channel.ShutdownAsync().GetAwaiter().GetResult();

					HasShutDownNow = true;
					return error;
				}
				finally
				{
					Nodes.Clear();
					Network.Clear();
				}
			}
		}
		public virtual BaseNetworkT SetNetwork(Dictionary<string, KeyT> network)
		{
			lock (this)
			{
				var newNodes = new List<BaseNodeT>();
				var newHealthyNodes = new List<BaseNodeT>();
				var newNetwork = new Dictionary<KeyT, IList<BaseNodeT>>();
				var newNodeKeys = new HashSet<KeyT>();
				var newNodeAddresses = new HashSet<string>();

				// getNodesToRemove() should always return the list in reverse order
				foreach (var index in GetNodesToRemove(network))
				{
					var stopAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + CloseTimeout.Seconds;
					var remainingTime = stopAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
					var node = Nodes[index];

					// Exit early if we have no time remaining
					if (remainingTime <= 0)
					{
						throw new TimeoutException("Failed to properly shutdown all channels");
					}

					RemoveNodeFromNetwork(node);
					node.Dispose(TimeSpan.FromSeconds(remainingTime));
					Nodes.RemoveAt(index);
				}

				foreach (var node in this.Nodes)
				{
					newNodes.Add(node);
					newNodeKeys.Add(node.Key);
					newNodeAddresses.Add(node.Address.ToString());
				}

				foreach (var entry in network)
				{
					var node = CreateNodeFromNetworkEntry(entry);

					if (newNodeKeys.Contains(node.Key) && newNodeAddresses.Contains(node.Address.ToString()))
						continue;

					newNodes.Add(node);
				}

				foreach (var node in newNodes)
				{
					if (newNetwork.TryGetValue(node.Key, out IList<BaseNodeT>? value)) value.Add(node);
					else newNetwork.Add(node.Key, [node]);

					newHealthyNodes.Add(node);
				}


				// Atomically set all the variables
				Nodes = newNodes;
				Network = newNetwork;
				HealthyNodes = newHealthyNodes;

				// noinspection unchecked
				return (BaseNetworkT)this;
			}
		}
		/// <include file="BaseNetwork.cs.xml" path='docs/member[@name="M:BaseNetwork.GetNodeProxies(KeyT)"]/*' />
		public virtual IList<BaseNodeT> GetNodeProxies(KeyT key)
		{
			lock (this)
			{

				// Attempt to readmit nodes each time a node is fetched.
				// Note: Readmitting nodes will only happen periodically so calling it each time should not harm
				// performance.
				ReadmitNodes();

				return Network[key];
			}
		}        
    }
}