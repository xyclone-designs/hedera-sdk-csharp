// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <summary>
	/// An individual mirror node.
	/// </summary>
	internal class MirrorNode : BaseNode<MirrorNode, BaseNodeAddress>
    {
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="address">the node address as a managed node address</param>
		/// <param name="executor">the executor service</param>
		public MirrorNode(BaseNodeAddress address, ExecutorService executor) : base(address, executor) { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">the node address as a string</param>
        /// <param name="executor">the executor service</param>
        public MirrorNode(string address, ExecutorService executor) : this(BaseNodeAddress.FromString(address), executor) { }

        public override string? Authority
        {
            get => null;
        }
        public override BaseNodeAddress Key
        {
            get => Address;
		}

        /// <summary>
        /// Build the REST base URL for this mirror node.
        /// </summary>
        /// <returns>scheme://host[:port]/api/v1</returns>
        public virtual string GetRestBaseUrl()
        {
            if (Address.Address == null)
            {
                throw new InvalidOperationException("mirror node address is not set");
            }

            if (IsLocalHost(Address.Address))
            {
                // For localhost, always use port 5551 for general REST calls
                return "http://" + Address.Address + ":5551/api/v1";
            }

            string scheme = ChooseScheme(Address.Port);
            StringBuilder @base = new ();
            @base.Append(scheme).Append("://").Append(Address.Address);

            // Omit default ports
            if (!IsDefaultPort(scheme, Address.Port))
            {
                @base.Append(':').Append(Address.Port);
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