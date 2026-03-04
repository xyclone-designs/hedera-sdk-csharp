// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <include file="MirrorNode.cs.xml" path='docs/member[@name="T:MirrorNode"]/*' />
	public class MirrorNode : BaseNode<MirrorNode, BaseNodeAddress>
    {
		/// <include file="MirrorNode.cs.xml" path='docs/member[@name="M:MirrorNode.#ctor(BaseNodeAddress,ExecutorService)"]/*' />
		internal MirrorNode(BaseNodeAddress address, ExecutorService executor) : base(address, executor) { }
        /// <include file="MirrorNode.cs.xml" path='docs/member[@name="M:MirrorNode.#ctor(System.String,ExecutorService)"]/*' />
        internal MirrorNode(string address, ExecutorService executor) : this(BaseNodeAddress.FromString(address), executor) { }

        public override string? Authority
        {
            get => null;
        }
        public override BaseNodeAddress Key
        {
            get => Address;
		}

        /// <include file="MirrorNode.cs.xml" path='docs/member[@name="M:MirrorNode.GetRestBaseUrl"]/*' />
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