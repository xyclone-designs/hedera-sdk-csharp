// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Utility class.
    /// </summary>
    internal class MirrorNetwork : BaseNetwork<MirrorNetwork, BaseNodeAddress, MirrorNode>
    {
        private MirrorNetwork(ExecutorService executor, IList<string> addresses) : base(executor)
        {
            TransportSecurity = true;

            try
            {
                SetNetwork(addresses);
            }
            catch (ThreadInterruptedException e) { }
            catch (TimeoutException) { }
        }

		/// <summary>
		/// Create a mirror network for mainnet.
		/// </summary>
		/// <param name="executor">the executor service</param>
		/// <returns>the new mirror network for mainnet</returns>
		internal static MirrorNetwork ForMainnet(ExecutorService executor)
        {
            return new MirrorNetwork(executor, [ "mainnet-public.mirrornode.hedera.com:443" ]);
        }
		/// <summary>
		/// Create a mirror network for testnet.
		/// </summary>
		/// <param name="executor">the executor service</param>
		/// <returns>the new mirror network for testnet</returns>
		internal static MirrorNetwork ForTestnet(ExecutorService executor)
        {
            return new MirrorNetwork(executor, [ "testnet.mirrornode.hedera.com:443" ]);
        }
		/// <summary>
		/// Create a mirror network for previewnet.
		/// </summary>
		/// <param name="executor">the executor service</param>
		/// <returns>the new mirror network for previewnet</returns>
		internal static MirrorNetwork ForPreviewnet(ExecutorService executor)
        {
            return new MirrorNetwork(executor, [ "previewnet.mirrornode.hedera.com:443" ]);
        }
		/// <summary>
		/// Create an arbitrary mirror network.
		/// </summary>
		/// <param name="executor">the executor service</param>
		/// <param name="addresses">the arbitrary address for the network</param>
		/// <returns>the new mirror network object</returns>
		internal static MirrorNetwork ForNetwork(ExecutorService executor, IList<string> addresses)
		{
			return new MirrorNetwork(executor, addresses);
		}

		/// <summary>
		/// Gets or sets the network names.
		/// </summary>
		/// <exception cref="TimeoutException">
		/// When the transaction times out.
		/// </exception>
		/// <exception cref="InterruptedException">
		/// When a thread is interrupted while it's waiting, sleeping, or otherwise occupied.
		/// </exception>
		public virtual IList<string> Network
		{
			get
			{
				lock (this)
				{
					return Network.Keys
						.Select(k => k.ToString())
						.ToList();
				}
			}
			set
			{
                lock (this)
				{
					var map = new Dictionary<string, BaseNodeAddress>(value.Count);
					foreach (var address in value)
						map[address] = BaseNodeAddress.FromString(address);

					base.SetNetwork(map);
				}
			}
		}

		protected override MirrorNode CreateNodeFromNetworkEntry(KeyValuePair<String, BaseNodeAddress> entry)
        {
            return new MirrorNode(entry.Key, executor);
        }

		/// <summary>
		/// Convenience to get the REST base URL from the next healthy mirror node.
		/// </summary>
		public virtual string GetRestBaseUrl()
		{
			return GetNextMirrorNode().GetRestBaseUrl();
		}
		/// <summary>
		/// Extract the next healthy mirror node on the list.
		/// </summary>
		/// <returns>the next healthy mirror node on the list</returns>
		/// <exception cref="InterruptedException">when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
		public virtual MirrorNode GetNextMirrorNode()
        {
            lock (this)
            {
                return GetNumberOfMostHealthyNodes(1)[0];
            }
        }
    }
}