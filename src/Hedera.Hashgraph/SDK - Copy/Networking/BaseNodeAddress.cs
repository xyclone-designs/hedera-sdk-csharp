// SPDX-License-Identifier: Apache-2.0
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <summary>
    /// Internal utility class.
    /// </summary>
    internal class BaseNodeAddress
    {
		internal static readonly int PORT_MIRROR_TLS = 443;
		internal static readonly int PORT_NODE_PLAIN = 50211;
		internal static readonly int PORT_NODE_TLS = 50212;
		private static readonly Regex HOST_AND_PORT = new ("^(\\S+):(\\d+)$");
        private static readonly Regex IN_PROCESS = new ("^in-process:(\\S+)$");

        // If address is `in-process:.*` this will contain the right side of the `:`
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">the name part</param>
        /// <param name="address">the address part</param>
        /// <param name="port">the port part</param>
        public BaseNodeAddress(string? name, string? address, int port) : this(name, address, port, false) { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">the name part</param>
        /// <param name="address">the address part</param>
        /// <param name="port">the port part</param>
        /// <param name="secure">secure transport</param>
        public BaseNodeAddress(string? name, string? address, int port, bool secure)
        {
            Name = name;
            Address = address;
            Port = port;
            Secure = secure;
        }

        /// <summary>
        /// Create a managed node address fom a string.
        /// </summary>
        /// <param name="string">the string representation</param>
        /// <returns>                         the new managed node address</returns>
        public static BaseNodeAddress FromString(string @string)
        {
			MatchCollection hostAndPortMatcher = HOST_AND_PORT.Matches(@string);
            MatchCollection inProcessMatcher = IN_PROCESS.Matches(@string);

            if (hostAndPortMatcher.Count != 0 && hostAndPortMatcher.Count == 2)
            {
				Match address = hostAndPortMatcher.ElementAt(1);
                Match port = hostAndPortMatcher.ElementAt(2);

                return new BaseNodeAddress(null, address.Value, int.Parse(port.Value));
            }
            else if (inProcessMatcher.Count != 0 && inProcessMatcher.Count == 1)
			{
				Match address = hostAndPortMatcher.ElementAt(1);

				return new BaseNodeAddress(address.Value, null, 0);
            }
            else
            {
                throw new InvalidOperationException("failed to parse node address");
            }
        }

		/// <summary>
		/// Extract the name.
		/// </summary>
		public virtual string? Name { protected set; get; }
		/// <summary>
		/// Extract the address.
		/// </summary>
		public virtual string? Address { protected set; get; }
		/// <summary>
		/// Extract the port.
		/// </summary>
		public virtual int Port { protected set; get; }
		
		public virtual bool Secure { protected set; get; }
		/// <summary>
		/// Are we in process?
		/// </summary>
		public virtual bool IsInProcess 
        {
            get => Name != null;
		}
		/// <summary>
		/// Are we secure?
		/// </summary>
		public virtual bool IsTransportSecurity 
        {
            get => Port == PORT_NODE_TLS || Port == PORT_MIRROR_TLS || Secure;
		}

        /// <summary>
        /// Create a new insecure managed node.
        /// </summary>
        /// <returns>                         the insecure managed node address</returns>
        public virtual BaseNodeAddress ToInsecure()
        {
            return new BaseNodeAddress(Name, Address, Port == PORT_NODE_TLS ? PORT_NODE_PLAIN : Port, false);
        }
        /// <summary>
        /// Create a new managed node.
        /// </summary>
        /// <returns>                         the secure managed node address</returns>
        public virtual BaseNodeAddress ToSecure()
        {
            return new BaseNodeAddress(Name, Address, Port == PORT_NODE_PLAIN ? PORT_NODE_TLS : Port, true);
        }

        public override string ToString()
        {
            return Name ?? Address + ":" + Port;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Address, Port);
        }
		public override bool Equals(object? o)
		{
			if (this == o)
				return true;
			if (o == null || GetType() != o?.GetType())
				return false;
			
            BaseNodeAddress that = (BaseNodeAddress)o;

			return Equals(Name, that.Name) && Equals(Address, that.Address) && Port == that.Port;
		}
	}
}