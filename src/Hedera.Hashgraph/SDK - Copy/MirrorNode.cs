// SPDX-License-Identifier: Apache-2.0
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
    /// An individual mirror node.
    /// </summary>
    class MirrorNode : BaseNode<MirrorNode, BaseNodeAddress>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">the node address as a managed node address</param>
        /// <param name="executor">the executor service</param>
        MirrorNode(BaseNodeAddress address, ExecutorService executor) : base(address, executor)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">the node address as a string</param>
        /// <param name="executor">the executor service</param>
        MirrorNode(string address, ExecutorService executor) : this(BaseNodeAddress.FromString(address), executor)
        {
        }

        protected override string GetAuthority()
        {
            return null;
        }

        override BaseNodeAddress GetKey()
        {
            return address;
        }

        /// <summary>
        /// Build the REST base URL for this mirror node.
        /// </summary>
        /// <returns>scheme://host[:port]/api/v1</returns>
        public virtual string GetRestBaseUrl()
        {
            if (address.Address == null)
            {
                throw new InvalidOperationException("mirror node address is not set");
            }

            if (IsLocalHost(address.Address))
            {
                // For localhost, always use port 5551 for general REST calls
                return "http://" + address.Address + ":5551/api/v1";
            }

            string scheme = ChooseScheme(address.Port);
            StringBuilder @base = new ();
            @base.Append(scheme).Append("://").Append(address.Address);

            // Omit default ports
            if (!IsDefaultPort(scheme, address.Port))
            {
                @base.Append(':').Append(address.Port);
            }

            @base.Append("/api/v1");
            return @base.ToString();
        }

        private static bool IsLocalHost(string host)
        {
            return "localhost".Equals(host) || "127.0.0.1".Equals(host);
        }

        private static string ChooseScheme(int port)
        {
            return port == 80 ? "http" : "https";
        }

        private static bool IsDefaultPort(string scheme, int port)
        {
            return ("http".Equals(scheme) && port == 80) || ("https".Equals(scheme) && port == 443);
        }
    }
}