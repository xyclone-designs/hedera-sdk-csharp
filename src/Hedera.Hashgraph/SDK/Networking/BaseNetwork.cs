// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Annotations;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Ids;
using Java;
using Java.Time;
using Java.Util.Concurrent;
using Javax.Annotation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <summary>
	/// Abstracts away most of the similar functionality between {@link Network} and {@link MirrorNetwork}
	/// </summary>
	/// <param name="<BaseNetworkT>">- The network that is extending this class. This is used for builder pattern setter methods.</param>
	/// <param name="<KeyT>">- The identifying type for the network.</param>
	/// <param name="<BaseNodeT>">- The specific node type for this network.</param>
	public abstract class BaseNetwork<BaseNetworkT, KeyT, BaseNodeT>
        where BaseNetworkT : BaseNetwork<BaseNetworkT, KeyT, BaseNodeT> 
        where BaseNodeT : BaseNode<BaseNodeT, KeyT>
        where KeyT : notnull

	{
        protected static readonly int DEFAULT_MAX_NODE_ATTEMPTS = -1;
        protected static readonly Random random = new ();
        protected readonly ExecutorService executor;

		private bool HasShutDownNow = false;

		/// <summary>
		/// The list of all nodes.
		/// </summary>
		protected IList<BaseNodeT> Nodes = [];
        /// <summary>
        /// The list of currently healthy nodes.
        /// </summary>
        protected IList<BaseNodeT> HealthyNodes = [];        
        /// <summary>
        /// The instant that readmission will happen after.
        /// </summary>
        protected Timestamp earliestReadmitTime;
        /// <summary>
        /// The name of the network. This corresponds to ledger ID in entity ID checksum calculations
        /// </summary>
        
        internal BaseNetwork(ExecutorService executor)
        {
            executor = executor;
            earliestReadmitTime = Timestamp.Now().Plus(MinNodeReadmitTime);
        }

		/// <summary>
		/// Set the new LedgerId for this network. LedgerIds are used for TLS certificate checking and entity ID
		/// checksum validation.
		/// </summary>
		public virtual LedgerId LedgerId
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }
		}
		/// <summary>
		/// Limit for how many times we retry a node which has returned a bad gRPC status
		/// </summary>
		public virtual int MaxNodeAttempts
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }

		} = Client.DEFAULT_MAX_NODE_ATTEMPTS;
		/// <summary>
		/// The current minimum backoff for the nodes in the network. This backoff is used when nodes return a bad
		/// gRPC status.
		/// </summary>
		public virtual Duration MinNodeBackoff
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
		/// <summary>
		/// The current maximum backoff for the nodes in the network. This backoff is used when nodes return a bad
		/// gRPC status.
		/// </summary>
		public virtual Duration MaxNodeBackoff
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
		/// <summary>
		/// The min time to wait before attempting to readmit nodes.
		/// </summary>
		public virtual Duration MinNodeReadmitTime
		{
			get { lock (this) return field; }
			set 
            {
                lock (this)
                {
					field = value;

					foreach (var node in Nodes)
						node.ReadmitTime = Timestamp.FromDateTime(DateTime.UtcNow);
				}
            }
		} = Client.DEFAULT_MIN_NODE_BACKOFF;
		/// <summary>
		/// The max time to wait for readmitting nodes.
		/// </summary>
		public virtual Duration MaxNodeReadmitTime
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }
		} = Client.DEFAULT_MAX_NODE_BACKOFF;
		/// <summary>
		/// Timeout for closing either a single node when setting a new network, or closing the entire network.
		/// </summary>
		public virtual Duration CloseTimeout
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }
		} = Client.DEFAULT_CLOSE_TIMEOUT;
		/// <summary>
		/// Is the network using transport security
		/// </summary>
		public virtual bool TransportSecurity { get; set; }

		protected abstract BaseNodeT CreateNodeFromNetworkEntry(KeyValuePair<string, KeyT> entry);
        /// <summary>
        /// Returns a list of index in descending order to remove from the current node list.
        /// 
        /// Descending order is important here because {@link BaseNetwork#setNetwork(IDictionary<String, KeyT>)} uses a for-each loop.
        /// </summary>
        /// <param name="network">- the new network</param>
        /// <returns>- list of indexes in descending order</returns>
        protected virtual IList<int> GetNodesToRemove(Dictionary<string, KeyT> network)
        {
            var nodes = new List<int>(Nodes.Count);

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];

                if (!NodeIsInGivenNetwork(nodes, network))
                {
                    nodes.Add(i);
                }
            }

            return nodes;
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

		/// <summary>
		/// Intelligently overwrites the current network.
		/// 
		/// Shutdown and remove any node from the current network if the new network doesn't contain it. This includes
		/// checking both the URL and {@link AccountId} when the network is a {@link Network}.
		/// 
		/// Add any nodes from the new network that don't already exist in the network.
		/// </summary>
		/// <param name="network">- The new network</param>
		/// <returns>- {@code this}</returns>
		/// <exception cref="TimeoutException">- when shutting down nodes</exception>
		/// <exception cref="InterruptedException">- when acquiring the lock</exception>
		public virtual Dictionary<KeyT, IList<BaseNodeT>> Network
		{
            protected get;
			set
			{
				lock (this)
				{
					var newNodes = new List<BaseNodeT>();
					var newHealthyNodes = new List<BaseNodeT>();
					var newNetwork = new Dictionary<KeyT, IList<BaseNodeT>>();
					var newNodeKeys = new HashSet<KeyT>();
					var newNodeAddresses = new HashSet<string>();

					// getNodesToRemove() should always return the list in reverse order
					foreach (var index in GetNodesToRemove(value))
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
						node.Dispose(Duration.FromTimeSpan(TimeSpan.FromSeconds(remainingTime)));
						Nodes.RemoveAt(index);
					}

					foreach (var node in Nodes)
					{
						newNodes.Add(node);
						newNodeKeys.Add(node.Key);
						newNodeAddresses.Add(node.Address.ToString());
					}

					foreach (var entry in value)
					{
						var node = CreateNodeFromNetworkEntry(entry);
						if (newNodeKeys.Contains(node.Key) && newNodeAddresses.Contains(node.Address.ToString()))
						{
							continue;
						}

						newNodes.Add(node);
					}

					foreach (var node in newNodes)
					{
						if (newNetwork.TryGetValue(node.Key, out IList<BaseNodeT>? value1))
							value1.Add(node);
						else
							newNetwork.Add(node.Key, [node]);

						newHealthyNodes.Add(node);
					}


					// Atomically set all the variables
					Nodes = newNodes;
					field = newNetwork;
					HealthyNodes = newHealthyNodes;
				}
			}

		} = new Dictionary<KeyT, IList<BaseNodeT>>();

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

        /// <summary>
        /// Remove any nodes from the network when they've exceeded the {@link BaseNetwork#maxNodeAttempts} limit
        /// </summary>
        /// <exception cref="InterruptedException">- when shutting down nodes</exception>
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

        /// <summary>
        /// Readmits nodes from the `nodes` list into the `healthyNodes` list when the time is passed the
        /// {@code earliestReadmitTime}. While readmitting nodes the `earliestReadmitTime` will be updated to
        /// a new value. This value is either the value of the node with the smallest readmission time from now,
        /// or `minNodeReadmitTime` or `maxNodeReadmitTime`.
        /// </summary>
        public virtual void ReadmitNodes()
        {
            lock (this)
            {
                var now = Timestamp.Now();
                if (now.ToEpochMilli() > earliestReadmitTime.ToEpochMilli())
                {
                    var nextEarliestReadmitTime = now.Plus(MaxNodeReadmitTime);
                    foreach (var node in Nodes)
                    {
                        if (node.ReadmitTime.IsAfter(now) && node.ReadmitTime.IsBefore(nextEarliestReadmitTime))
                        {
                            nextEarliestReadmitTime = node.ReadmitTime;
                        }
                    }

                    earliestReadmitTime = nextEarliestReadmitTime;
                    if (earliestReadmitTime.IsBefore(now.Plus(MinNodeReadmitTime)))
                    {
                        earliestReadmitTime = now.Plus(MinNodeReadmitTime);
                    }

                    outer:
                        for (var i = 0; i < Nodes.Count; i++)
                        {

                            // Check if `healthyNodes` already contains this node
                            for (var j = 0; j < HealthyNodes.Count; j++)
                            {
                                if (Nodes[i] == HealthyNodes[j])
                                {
                                    continue;
                                }
                            }


                            // If `healthyNodes` doesn't contain the node, check the `readmitTime` on the node
                            if (Nodes[i].ReadmitTime.IsBefore(now))
                            {
                                HealthyNodes.Add(Nodes[i]);
                            }
                        }
                }
            }
        }

        /// <summary>
        /// Get a random healthy node.
        /// </summary>
        /// <returns>                         the node</returns>
        public virtual BaseNodeT GetRandomNode()
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
        /// <summary>
        /// Get all node proxies by key
        /// </summary>
        /// <param name="key">the desired key</param>
        /// <returns>                         the list of node proxies</returns>
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

        /// <summary>
        /// Returns `count` number of the most healthy nodes. Healthy-ness is determined by sort order; leftmost being most
        /// healthy. This will also remove any nodes which have hit or exceeded {@link BaseNetwork#maxNodeAttempts}.
        /// 
        /// Returns a list of nodes where each node has a unique key.
        /// </summary>
        /// <param name="count">number of nodes to return</param>
        /// <returns>                         List of nodes to use</returns>
        /// <exception cref="InterruptedException">when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
        protected virtual IList<BaseNodeT> GetNumberOfMostHealthyNodes(int count)
        {
            lock (this)
            {
                ReadmitNodes();
                RemoveDeadNodes();
                var returnNodes = new Dictionary<KeyT, BaseNodeT>(count);
                for (var i = 0; i < count; i++)
                {
                    var node = GetRandomNode();
                    
                    returnNodes.TryAdd(node.Key, node);
				}

                var returnList = new List<BaseNodeT>();
                
                returnList.AddRange(returnNodes.Values);

                return returnList;
            }
        }

        public virtual void BeginClose()
        {
            lock (this)
            {
                foreach (var node in Nodes)
                {
                    if (node.Channel != null)
                    {
                        node.Channel = node.Channel.Shutdown();
                    }
                }
            }
        }

        // returns null if successful, or Exception if error occurred
        public virtual Exception? AwaitClose(Timestamp deadline, Exception previousError)
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
                            if (timeoutMillis <= 0 || !node.Channel.AwaitTermination(timeoutMillis, TimeUnit.MILLISECONDS))
                            {
                                throw new TimeoutException("Failed to properly shutdown all channels");
                            }
                            else
                            {
                                node.Channel = null;
                            }
                        }
                    }

                    return null;
                }
                catch (Exception error)
                {
                    foreach (var node in Nodes)
						node.Channel?.ShutdownNow();

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
    }
}