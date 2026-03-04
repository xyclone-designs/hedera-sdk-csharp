// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <include file="MirrorNetwork.cs.xml" path='docs/member[@name="T:MirrorNetwork"]/*' />
    public class MirrorNetwork : BaseNetwork<MirrorNetwork, BaseNodeAddress, MirrorNode>
    {
		internal MirrorNetwork(ExecutorService executor, IEnumerable<string> addresses) : base(executor)
        {
            TransportSecurity = true;

            try
            {
                Network = [..addresses];
            }
            catch (ThreadInterruptedException e) { }
            catch (TimeoutException) { }
        }

		/// <include file="MirrorNetwork.cs.xml" path='docs/member[@name="M:MirrorNetwork.ForMainnet(ExecutorService)"]/*' />
		internal static MirrorNetwork ForMainnet(ExecutorService executor)
        {
            return new MirrorNetwork(executor, [ "mainnet-public.mirrornode.hedera.com:443" ]);
        }
		/// <include file="MirrorNetwork.cs.xml" path='docs/member[@name="M:MirrorNetwork.ForTestnet(ExecutorService)"]/*' />
		internal static MirrorNetwork ForTestnet(ExecutorService executor)
        {
            return new MirrorNetwork(executor, [ "testnet.mirrornode.hedera.com:443" ]);
        }
		/// <include file="MirrorNetwork.cs.xml" path='docs/member[@name="M:MirrorNetwork.ForPreviewnet(ExecutorService)"]/*' />
		internal static MirrorNetwork ForPreviewnet(ExecutorService executor)
        {
            return new MirrorNetwork(executor, [ "previewnet.mirrornode.hedera.com:443" ]);
        }
		/// <include file="MirrorNetwork.cs.xml" path='docs/member[@name="M:MirrorNetwork.ForNetwork(ExecutorService,System.Collections.Generic.IEnumerable{System.String})"]/*' />
		internal static MirrorNetwork ForNetwork(ExecutorService executor, IEnumerable<string> addresses)
		{
			return new MirrorNetwork(executor, addresses);
		}

		/// <include file="MirrorNetwork.cs.xml" path='docs/member[@name="M:MirrorNetwork.lock(this)"]/*' />
		public new virtual IList<string> Network
		{
			get
			{
				lock (this)
				{
					return [.. base.Network.Keys.Select(k => k.ToString())];
				}
			}
			set
			{
                lock (this)
				{
					var map = new Dictionary<BaseNodeAddress, IList<MirrorNode>>(value.Count);
					foreach (var address in value)
						map[BaseNodeAddress.FromString(address)] = [new MirrorNode(address, Executor)];

					base.Network = map;
				}
			}
		}

		protected override MirrorNode CreateNodeFromNetworkEntry(KeyValuePair<string, BaseNodeAddress> entry)
        {
            return new MirrorNode(entry.Value, Executor);
        }

		/// <include file="MirrorNetwork.cs.xml" path='docs/member[@name="M:MirrorNetwork.GetRestBaseUrl"]/*' />
		public virtual string GetRestBaseUrl()
		{
			return GetNextMirrorNode().GetRestBaseUrl();
		}
		/// <include file="MirrorNetwork.cs.xml" path='docs/member[@name="M:MirrorNetwork.GetNextMirrorNode"]/*' />
		public virtual MirrorNode GetNextMirrorNode()
        {
            lock (this)
            {
                return GetNumberOfMostHealthyNodes(1)[0];
            }
        }
	}
}