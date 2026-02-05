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

// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/// <summary>
	/// Abstracts away most of the similar functionality between {@link Network} and {@link MirrorNetwork}
	/// </summary>
	/// <param name="<BaseNetworkT>">- The network that is extending this class. This is used for builder pattern setter methods.</param>
	/// <param name="<KeyT>">- The identifying type for the network.</param>
	/// <param name="<BaseNodeT>">- The specific node type for this network.</param>
	abstract class BaseNetwork<BaseNetworkT, KeyT, BaseNodeT>
		where BaseNetworkT : BaseNetwork<BaseNetworkT, KeyT, BaseNodeT>
		where BaseNodeT : BaseNode<BaseNodeT, KeyT>
		where KeyT : notnull
	{
		protected const int DEFAULT_MAX_NODE_ATTEMPTS = -1;
		protected static readonly Random Random = new();

		protected readonly ExecutorService Executor;

		/// <summary>
		/// Map of node identifiers to nodes.
		/// </summary>
		protected IDictionary<KeyT, List<BaseNodeT>> NetworkMap = new ConcurrentDictionary<KeyT, List<BaseNodeT>>();

		/// <summary>
		/// All nodes.
		/// </summary>
		protected List<BaseNodeT> Nodes = new();

		/// <summary>
		/// Healthy nodes.
		/// </summary>
		protected List<BaseNodeT> HealthyNodes = new();

		protected TimeSpan MinNodeBackoff = Client.DEFAULT_MIN_NODE_BACKOFF;
		protected TimeSpan MaxNodeBackoff = Client.DEFAULT_MAX_NODE_BACKOFF;
		protected TimeSpan CloseTimeout = Client.DEFAULT_CLOSE_TIMEOUT;

		protected int MaxNodeAttempts = DEFAULT_MAX_NODE_ATTEMPTS;
		protected bool TransportSecurity;

		protected TimeSpan MinNodeReadmitTime = Client.DEFAULT_MIN_NODE_BACKOFF;
		protected TimeSpan MaxNodeReadmitTime = Client.DEFAULT_MAX_NODE_BACKOFF;

		protected DateTimeOffset EarliestReadmitTime;

		private LedgerId? _ledgerId;

		internal bool HasShutDownNow;

		protected BaseNetwork(ExecutorService executor)
		{
			Executor = executor;
			EarliestReadmitTime = DateTimeOffset.UtcNow + MinNodeReadmitTime;
		}

		public LedgerId? LedgerId
		{
			get
			{
				lock (this)
				{
					return _ledgerId;
				}
			}
			set
			{
				lock (this)
				{
					_ledgerId = value;
				}
			}
		}

		public int NodeMaxAttempts
		{
			get
			{
				lock (this)
				{
					return MaxNodeAttempts;
				}
			}
			set
			{
				lock (this)
				{
					MaxNodeAttempts = value;
				}
			}
		}

		public TimeSpan MinimumNodeBackoff
		{
			get
			{
				lock (this)
				{
					return MinNodeBackoff;
				}
			}
			set
			{
				lock (this)
				{
					MinNodeBackoff = value;
					foreach (var node in Nodes)
					{
						node.SetMinBackoff(value);
					}
				}
			}
		}

		public TimeSpan MaximumNodeBackoff
		{
			get
			{
				lock (this)
				{
					return MaxNodeBackoff;
				}
			}
			set
			{
				lock (this)
				{
					MaxNodeBackoff = value;
					foreach (var node in Nodes)
					{
						node.SetMaxBackoff(value);
					}
				}
			}
		}

		public TimeSpan MinimumNodeReadmitTime
		{
			get => MinNodeReadmitTime;
			set
			{
				MinNodeReadmitTime = value;
				foreach (var node in Nodes)
				{
					node.ReadmitTime = DateTimeOffset.UtcNow;
				}
			}
		}

		public TimeSpan MaximumNodeReadmitTime
		{
			get => MaxNodeReadmitTime;
			set => MaxNodeReadmitTime = value;
		}

		public TimeSpan NetworkCloseTimeout
		{
			get
			{
				lock (this)
				{
					return CloseTimeout;
				}
			}
			set
			{
				lock (this)
				{
					CloseTimeout = value;
				}
			}
		}

		protected abstract BaseNodeT CreateNodeFromNetworkEntry(KeyValuePair<string, KeyT> entry);

		protected IList<int> GeBaseNodeTsToRemove(IDictionary<string, KeyT> network)
		{
			var result = new List<int>();

			for (var i = Nodes.Count - 1; i >= 0; i--)
			{
				if (!NodeExistsInNetwork(Nodes[i], network))
				{
					result.Add(i);
				}
			}

			return result;
		}

		private static bool NodeExistsInNetwork(BaseNodeT node, IDictionary<string, KeyT> network)
		{
			foreach (var entry in network)
			{
				if (node.Key!.Equals(entry.Value) &&
					node.Address.Equals(BaseNodeAddress.FromString(entry.Key)))
				{
					return true;
				}
			}

			return false;
		}

		internal BaseNetworkT SetNetwork(IDictionary<string, KeyT> network)
		{
			lock (this)
			{
				var newNodes = new List<BaseNodeT>();
				var newHealthyNodes = new List<BaseNodeT>();
				var newNetwork = new Dictionary<KeyT, List<BaseNodeT>>();

				foreach (var index in GeBaseNodeTsToRemove(network))
				{
					var node = Nodes[index];
					RemoveNode(node);
					node.Close(CloseTimeout);
					Nodes.RemoveAt(index);
				}

				var existingKeys = new HashSet<KeyT>(Nodes.Select(n => n.Key));
				var existingAddresses = new HashSet<string>(Nodes.Select(n => n.Address.ToString()));

				newNodes.AddRange(Nodes);

				foreach (var entry in network)
				{
					var node = CreateNodeFromNetworkEntry(entry);

					if (existingKeys.Contains(node.Key) &&
						existingAddresses.Contains(node.Address.ToString()))
					{
						continue;
					}

					newNodes.Add(node);
				}

				foreach (var node in newNodes)
				{
					if (!newNetwork.TryGetValue(node.Key, out var list))
					{
						list = new List<BaseNodeT>();
						newNetwork[node.Key] = list;
					}

					list.Add(node);
					newHealthyNodes.Add(node);
				}

				Nodes = newNodes;
				NetworkMap = newNetwork;
				HealthyNodes = newHealthyNodes;

				return (BaseNetworkT)this;
			}
		}

		protected void IncreaseBackoff(BaseNodeT node)
		{
			lock (this)
			{
				node.IncreaseBackoff();
				HealthyNodes.Remove(node);
			}
		}

		protected void DecreaseBackoff(BaseNodeT node)
		{
			lock (this)
			{
				node.DecreaseBackoff();
			}
		}

		private void RemoveNode(BaseNodeT node)
		{
			if (NetworkMap.TryGetValue(node.Key, out var nodes))
			{
				nodes.Remove(node);
				if (nodes.Count == 0)
				{
					NetworkMap.Remove(node.Key);
				}
			}
		}

		protected void RemoveDeadNodes()
		{
			lock (this)
			{
				if (MaxNodeAttempts <= 0)
				{
					return;
				}

				for (var i = Nodes.Count - 1; i >= 0; i--)
				{
					var node = Nodes[i];
					if (node.BadGrpcStatusCount >= MaxNodeAttempts)
					{
						node.Close(CloseTimeout);
						RemoveNode(node);
						Nodes.RemoveAt(i);
					}
				}
			}
		}

		protected void ReadmiBaseNodeTs()
		{
			lock (this)
			{
				var now = DateTimeOffset.UtcNow;

				if (now <= EarliestReadmitTime)
				{
					return;
				}

				var next = now + MaxNodeReadmitTime;

				foreach (var node in Nodes)
				{
					if (node.ReadmitTime > now && node.ReadmitTime < next)
					{
						next = node.ReadmitTime;
					}
				}

				EarliestReadmitTime = next < now + MinNodeReadmitTime
					? now + MinNodeReadmitTime
					: next;

				foreach (var node in Nodes)
				{
					if (!HealthyNodes.Contains(node) && node.ReadmitTime <= now)
					{
						HealthyNodes.Add(node);
					}
				}
			}
		}

		protected BaseNodeT GetRandomNode()
		{
			lock (this)
			{
				ReadmiBaseNodeTs();

				if (HealthyNodes.Count == 0)
				{
					throw new InvalidOperationException("No healthy node was found");
				}

				return HealthyNodes[Random.Next(HealthyNodes.Count)];
			}
		}

		protected IReadOnlyList<BaseNodeT> GeBaseNodeTProxies(KeyT key)
		{
			lock (this)
			{
				ReadmiBaseNodeTs();
				return NetworkMap[key];
			}
		}

		protected IReadOnlyList<BaseNodeT> GetMostHealthyNodes(int count)
		{
			lock (this)
			{
				ReadmiBaseNodeTs();
				RemoveDeadNodes();

				var map = new Dictionary<KeyT, BaseNodeT>();

				while (map.Count < count)
				{
					var node = GetRandomNode();
					map.TryAdd(node.Key, node);
				}

				return map.Values.ToList();
			}
		}

		protected void BeginClose()
		{
			foreach (var node in Nodes)
			{
				node.Channel?.Shutdown();
			}
		}

		protected Exception? AwaitClose(DateTimeOffset deadline, Exception? previousError)
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
						var timeout = deadline - DateTimeOffset.UtcNow;
						if (timeout <= TimeSpan.Zero ||
							!node.Channel.AwaitTermination(timeout))
						{
							throw new TimeoutException("Failed to properly shutdown all channels");
						}

						node.Channel = null;
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				foreach (var node in Nodes)
				{
					node.Channel?.ShutdownNow();
				}

				HasShutDownNow = true;
				return ex;
			}
			finally
			{
				Nodes.Clear();
				NetworkMap.Clear();
			}
		}
	}
}
