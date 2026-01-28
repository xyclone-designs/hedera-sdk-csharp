// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Annotations;
using Java.Time;
using Java;
using Java.Util.Concurrent;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Ids;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Abstracts away most of the similar functionality between {@link Network} and {@link MirrorNetwork}
    /// </summary>
    /// <param name="<BaseNetworkT>">- The network that is extending this class. This is used for builder pattern setter methods.</param>
    /// <param name="<KeyT>">- The identifying type for the network.</param>
    /// <param name="<BaseNodeT>">- The specific node type for this network.</param>
    abstract class BaseNetwork<BaseNetworkT, KeyT, BaseNodeT>
        where BaseNetworkT : BaseNetwork<BaseNetworkT, KeyT, BaseNodeT> where BaseNodeT : BaseNode<BaseNodeT, KeyT>
    {
        protected static readonly int DEFAULT_MAX_NODE_ATTEMPTS = -1;
        protected static readonly Random random = new Random();
        protected readonly ExecutorService executor;
        /// <summary>
        /// Map of node identifiers to nodes. Used to quickly fetch node for identifier.
        /// </summary>
        protected Dictionary<KeyT, IList<BaseNodeT>> network = new ConcurrentDictionary();
        /// <summary>
        /// The list of all nodes.
        /// </summary>
        protected IList<BaseNodeT> nodes = new ();
        /// <summary>
        /// The list of currently healthy nodes.
        /// </summary>
        protected IList<BaseNodeT> healthyNodes = new ();
        /// <summary>
        /// The current minimum backoff for the nodes in the network. This backoff is used when nodes return a bad
        /// gRPC status.
        /// </summary>
        protected Duration minNodeBackoff = Client.DEFAULT_MIN_NODE_BACKOFF;
        /// <summary>
        /// The current maximum backoff for the nodes in the network. This backoff is used when nodes return a bad
        /// gRPC status.
        /// </summary>
        protected Duration maxNodeBackoff = Client.DEFAULT_MAX_NODE_BACKOFF;
        /// <summary>
        /// Timeout for closing either a single node when setting a new network, or closing the entire network.
        /// </summary>
        protected Duration closeTimeout = Client.DEFAULT_CLOSE_TIMEOUT;
        /// <summary>
        /// Limit for how many times we retry a node which has returned a bad gRPC status
        /// </summary>
        protected int maxNodeAttempts = DEFAULT_MAX_NODE_ATTEMPTS;
        /// <summary>
        /// Is the network using transport security
        /// </summary>
        protected bool transportSecurity;
        /// <summary>
        /// The min time to wait before attempting to readmit nodes.
        /// </summary>
        protected Duration minNodeReadmitTime = Client.DEFAULT_MIN_NODE_BACKOFF;
        /// <summary>
        /// The max time to wait for readmitting nodes.
        /// </summary>
        protected Duration maxNodeReadmitTime = Client.DEFAULT_MAX_NODE_BACKOFF;
        /// <summary>
        /// The instant that readmission will happen after.
        /// </summary>
        protected Timestamp earliestReadmitTime;
        /// <summary>
        /// The name of the network. This corresponds to ledger ID in entity ID checksum calculations
        /// </summary>
        private LedgerId ledgerId;
        bool hasShutDownNow = false;
        protected BaseNetwork(ExecutorService executor)
        {
            executor = executor;
            earliestReadmitTime = Timestamp.Now().Plus(minNodeReadmitTime);
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
		/// Set the max number of times a node can return a bad gRPC status before we remove it from the list.
		/// </summary>
		public virtual int MaxNodeAttempts
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }
		}

		/// <summary>
		/// Extract the minimum node backoff time.
		/// </summary>
		/// <returns>                         the minimum node backoff time</returns>
		public virtual Duration Get()
        {
            lock (this)
            {
                return minNodeBackoff;
            }
        }

		/// <summary>
		/// Set the minimum backoff a node should use when receiving a bad gRPC status.
		/// </summary>
		public virtual Duration MinNodeBackoff
		{
			get { lock (this) return field; }
			set 
            {
                lock (this)
                {
                    field = value;
					foreach (var node in nodes)
					{
						node.SetMinBackoff(minNodeBackoff);
					}
				} 
            }
		}

		/// <summary>
		/// Extract the maximum node backoff time.
		/// </summary>
		public virtual Duration MaxNodeBackoff
		{
			get { lock (this) return field; }
			set
			{
				lock (this)
				{
					field = value;

					foreach (var node in nodes)
						node.SetMaxBackoff(value) = Timestamp.FromDateTime(DateTime.UtcNow);
				}
			}
		}

        /// <summary>
        /// Assign the minimum node readmit time.
        /// </summary>
		public virtual Duration MinNodeReadmitTime
		{
			get { lock (this) return field; }
			set 
            {
                lock (this)
                {
					field = value;

					foreach (var node in nodes)
						node.ReadmitTime = Timestamp.FromDateTime(DateTime.UtcNow);
				}
            }
		}

		public virtual Duration MaxNodeReadmitTime
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }
		}

		/// <summary>
		/// Is transport Security enabled?
		/// </summary>
		/// <returns>                         using transport security</returns>
		public virtual bool IsTransportSecurity()
        {
            return transportSecurity;
        }

		/// <summary>
		/// Assign the close timeout.
		/// </summary>
		public virtual Duration CloseTimeout
		{
			get { lock (this) return field; }
			set { lock (this) field = value; }
		}

        protected abstract BaseNodeT CreateNodeFromNetworkEntry(Map.Entry<String, KeyT> entry);
        /// <summary>
        /// Returns a list of index in descending order to remove from the current node list.
        /// 
        /// Descending order is important here because {@link BaseNetwork#setNetwork(IDictionary<String, KeyT>)} uses a for-each loop.
        /// </summary>
        /// <param name="network">- the new network</param>
        /// <returns>- list of indexes in descending order</returns>
        protected virtual IList<int> GetNodesToRemove(Dictionary<string, KeyT> network)
        {
            var nodes = new List<int>(nodes.Count);
            for (int i = nodes.size() - 1; i >= 0; i--)
            {
                var node = nodes[i];
                if (!NodeIsInGivenNetwork(node, network))
                {
                    nodes.Add(i);
                }
            }

            return nodes;
        }

        private bool NodeIsInGivenNetwork(BaseNodeT node, Dictionary<string, KeyT> network)
        {
            foreach (var entry in network.EntrySet())
            {
                if (node.GetKey().Equals(entry.GetValue()) && node.address.Equals(BaseNodeAddress.FromString(entry.GetKey())))
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
                    var stopAt = Timestamp.Now().GetEpochSecond() + closeTimeout.GetSeconds();
                    var remainingTime = stopAt - Timestamp.Now().GetEpochSecond();
                    var node = nodes[index];

                    // Exit early if we have no time remaining
                    if (remainingTime <= 0)
                    {
                        throw new TimeoutException("Failed to properly shutdown all channels");
                    }

                    RemoveNodeFromNetwork(node);
                    node.Dispose(Duration.OfSeconds(remainingTime));
                    nodes.Remove(index.IntValue());
                }

                foreach (var node in nodes)
                {
                    newNodes.Add(node);
                    newNodeKeys.Add(node.GetKey());
                    newNodeAddresses.Add(node.address.ToString());
                }

                foreach (var entry in network.EntrySet())
                {
                    var node = CreateNodeFromNetworkEntry(entry);
                    if (newNodeKeys.Contains(node.GetKey()) && newNodeAddresses.Contains(node.GetAddress().ToString()))
                    {
                        continue;
                    }

                    newNodes.Add(node);
                }

                foreach (var node in newNodes)
                {
                    if (newNetwork.ContainsKey(node.GetKey()))
                    {
                        newNetwork[node.GetKey()].Add(node);
                    }
                    else
                    {
                        var list = new List<BaseNodeT>();
                        list.Add(node);
                        newNetwork.Add(node.GetKey(), list);
                    }

                    newHealthyNodes.Add(node);
                }


                // Atomically set all the variables
                nodes = newNodes;
                network = newNetwork;
                healthyNodes = newHealthyNodes;

                // noinspection unchecked
                return (BaseNetworkT)this;
            }
        }

        public virtual void IncreaseBackoff(BaseNodeT node)
        {
            lock (this)
            {
                node.IncreaseBackoff();
                healthyNodes.Remove(node);
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
            var nodesForKey = network[node.GetKey()];
            nodesForKey.Remove(node);
            if (nodesForKey.IsEmpty)
            {
                network.Remove(node.GetKey());
            }
        }

        private bool AddressIsInNodeList(string addressString, IList<BaseNodeT> nodes)
        {
            var address = BaseNodeAddress.FromString(addressString);
            foreach (var node in nodes)
            {
                if (node.address.Equals(address))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove any nodes from the network when they've exceeded the {@link BaseNetwork#maxNodeAttempts} limit
        /// </summary>
        /// <exception cref="InterruptedException">- when shutting down nodes</exception>
        protected virtual void RemoveDeadNodes()
        {
            if (maxNodeAttempts > 0)
            {
                for (int i = nodes.size() - 1; i >= 0; i--)
                {
                    var node = ArgumentNullException.ThrowIfNull(nodes[i]);
                    if (node.GetBadGrpcStatusCount() >= maxNodeAttempts)
                    {
                        node.Dispose(closeTimeout);
                        RemoveNodeFromNetwork(node);
                        nodes.Remove(i);
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
                    var nextEarliestReadmitTime = now.Plus(maxNodeReadmitTime);
                    foreach (var node in nodes)
                    {
                        if (node.readmitTime.IsAfter(now) && node.readmitTime.IsBefore(nextEarliestReadmitTime))
                        {
                            nextEarliestReadmitTime = node.readmitTime;
                        }
                    }

                    earliestReadmitTime = nextEarliestReadmitTime;
                    if (earliestReadmitTime.IsBefore(now.Plus(minNodeReadmitTime)))
                    {
                        earliestReadmitTime = now.Plus(minNodeReadmitTime);
                    }

                    outer:
                        for (var i = 0; i < nodes.Count; i++)
                        {

                            // Check if `healthyNodes` already contains this node
                            for (var j = 0; j < healthyNodes.Count; j++)
                            {
                                if (nodes[i] == healthyNodes[j])
                                {
                                    continue;
                                }
                            }


                            // If `healthyNodes` doesn't contain the node, check the `readmitTime` on the node
                            if (nodes[i].readmitTime.IsBefore(now))
                            {
                                healthyNodes.Add(nodes[i]);
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
                if (healthyNodes.IsEmpty)
                {
                    throw new InvalidOperationException("No healthy node was found");
                }

                return healthyNodes[random.NextInt(healthyNodes.Count)];
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
                return network[key];
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
                    if (!returnNodes.ContainsKey(node.GetKey()))
                    {
                        returnNodes.Add(node.GetKey(), node);
                    }
                }

                var returnList = new List<BaseNodeT>();
                returnList.AddAll(returnNodes.Values());
                return returnList;
            }
        }

        public virtual void BeginClose()
        {
            lock (this)
            {
                foreach (var node in nodes)
                {
                    if (node.channel != null)
                    {
                        node.channel = node.channel.Shutdown();
                    }
                }
            }
        }

        // returns null if successful, or Exception if error occurred
        public virtual Exception AwaitClose(Timestamp deadline, Exception previousError)
        {
            lock (this)
            {
                try
                {
                    if (previousError != null)
                    {
                        throw previousError;
                    }

                    foreach (var node in nodes)
                    {
                        if (node.channel != null)
                        {
                            var timeoutMillis = Duration.Between(Timestamp.Now(), deadline).ToMillis();
                            if (timeoutMillis <= 0 || !node.channel.AwaitTermination(timeoutMillis, TimeUnit.MILLISECONDS))
                            {
                                throw new TimeoutException("Failed to properly shutdown all channels");
                            }
                            else
                            {
                                node.channel = null;
                            }
                        }
                    }

                    return null;
                }
                catch (Exception error)
                {
                    foreach (var node in nodes)
                    {
                        if (node.channel != null)
                        {
                            node.channel.ShutdownNow();
                        }
                    }

                    hasShutDownNow = true;
                    return error;
                }
                finally
                {
                    nodes.Clear();
                    network.Clear();
                }
            }
        }
    }
}