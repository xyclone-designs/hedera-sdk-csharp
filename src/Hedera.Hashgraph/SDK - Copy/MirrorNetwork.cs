// SPDX-License-Identifier: Apache-2.0
using Java.Util;
using Java.Util.Concurrent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Utility class.
    /// </summary>
    class MirrorNetwork : BaseNetwork<MirrorNetwork, BaseNodeAddress, MirrorNode>
    {
        private MirrorNetwork(ExecutorService executor, IList<string> addresses) : base(executor)
        {
            this.transportSecurity = true;
            try
            {
                SetNetwork(addresses);
            }
            catch (InterruptedException e)
            {
            }
            catch (TimeoutException e)
            {
            }
        }

        /// <summary>
        /// Create an arbitrary mirror network.
        /// </summary>
        /// <param name="executor">the executor service</param>
        /// <param name="addresses">the arbitrary address for the network</param>
        /// <returns>the new mirror network object</returns>
        static MirrorNetwork ForNetwork(ExecutorService executor, IList<string> addresses)
        {
            return new MirrorNetwork(executor, addresses);
        }

        /// <summary>
        /// Create a mirror network for mainnet.
        /// </summary>
        /// <param name="executor">the executor service</param>
        /// <returns>the new mirror network for mainnet</returns>
        static MirrorNetwork ForMainnet(ExecutorService executor)
        {
            return new MirrorNetwork(executor, List.Of("mainnet-public.mirrornode.hedera.com:443"));
        }

        /// <summary>
        /// Create a mirror network for testnet.
        /// </summary>
        /// <param name="executor">the executor service</param>
        /// <returns>the new mirror network for testnet</returns>
        static MirrorNetwork ForTestnet(ExecutorService executor)
        {
            return new MirrorNetwork(executor, List.Of("testnet.mirrornode.hedera.com:443"));
        }

        /// <summary>
        /// Create a mirror network for previewnet.
        /// </summary>
        /// <param name="executor">the executor service</param>
        /// <returns>the new mirror network for previewnet</returns>
        static MirrorNetwork ForPreviewnet(ExecutorService executor)
        {
            return new MirrorNetwork(executor, List.Of("previewnet.mirrornode.hedera.com:443"));
        }

        /// <summary>
        /// Extract the network names.
        /// </summary>
        /// <returns>the network names</returns>
        virtual IList<string> GetNetwork()
        {
            lock (this)
            {
                IList<string> retval = new List(network.Count);
                foreach (var address in network.KeySet())
                {
                    retval.Add(address.ToString());
                }

                return retval;
            }
        }

        /// <summary>
        /// Assign the desired network.
        /// </summary>
        /// <param name="network">the desired network</param>
        /// <returns>the mirror network</returns>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        /// <exception cref="InterruptedException">when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
        virtual MirrorNetwork SetNetwork(IList<string> network)
        {
            lock (this)
            {
                var map = new Dictionary<string, BaseNodeAddress>(network.Count);
                foreach (var address in network)
                {
                    map.Put(address, BaseNodeAddress.FromString(address));
                }

                return base.SetNetwork(map);
            }
        }

        protected override MirrorNode CreateNodeFromNetworkEntry(Map.Entry<String, BaseNodeAddress> entry)
        {
            return new MirrorNode(entry.GetKey(), executor);
        }

        /// <summary>
        /// Extract the next healthy mirror node on the list.
        /// </summary>
        /// <returns>the next healthy mirror node on the list</returns>
        /// <exception cref="InterruptedException">when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
        virtual MirrorNode GetNextMirrorNode()
        {
            lock (this)
            {
                return GetNumberOfMostHealthyNodes(1)[0];
            }
        }

        /// <summary>
        /// Convenience to get the REST base URL from the next healthy mirror node.
        /// </summary>
        virtual string GetRestBaseUrl()
        {
            return GetNextMirrorNode().GetRestBaseUrl();
        }
    }
}