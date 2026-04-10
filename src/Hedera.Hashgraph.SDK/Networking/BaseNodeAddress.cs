// SPDX-License-Identifier: Apache-2.0
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.SDK.Networking
{
    /// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="T:BaseNodeAddress"]/*' />
    public class BaseNodeAddress
    {
		internal static readonly int PORT_MIRROR_TLS = 443;
		internal static readonly int PORT_NODE_PLAIN = 50211;
		internal static readonly int PORT_NODE_TLS = 50212;
		private static readonly Regex HOST_AND_PORT = new ("^(\\S+):(\\d+)$");
        private static readonly Regex IN_PROCESS = new ("^in-process:(\\S+)$");

		// If address is `in-process:.*` this will contain the right side of the `:`

		/// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="M:BaseNodeAddress.#ctor(System.String,System.String,System.Int32)"]/*' />
		internal BaseNodeAddress(string? name, string? address, int port) : this(name, address, port, false) { }
		/// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="M:BaseNodeAddress.#ctor(System.String,System.String,System.Int32,System.Boolean)"]/*' />
		internal BaseNodeAddress(string? name, string? address, int port, bool secure)
        {
            Name = name;
            Address = address;
            Port = port;
            Secure = secure;
        }

        /// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="M:BaseNodeAddress.FromString(string @)"]/*' />
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

		/// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="P:BaseNodeAddress.Name"]/*' />
		public virtual string? Name { protected set; get; }
		/// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="P:BaseNodeAddress.Address"]/*' />
		public virtual string? Address { protected set; get; }
		/// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="P:BaseNodeAddress.Port"]/*' />
		public virtual int Port { protected set; get; }
		
		public virtual bool Secure { protected set; get; }
		/// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="T:BaseNodeAddress_2"]/*' />
		public virtual bool IsInProcess 
        {
            get => Name != null;
		}
		/// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="M:BaseNodeAddress.ToInsecure"]/*' />
		public virtual bool IsTransportSecurity 
        {
            get => Port == PORT_NODE_TLS || Port == PORT_MIRROR_TLS || Secure;
		}

        /// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="M:BaseNodeAddress.ToInsecure_2"]/*' />
        public virtual BaseNodeAddress ToInsecure()
        {
            return new BaseNodeAddress(Name, Address, Port == PORT_NODE_TLS ? PORT_NODE_PLAIN : Port, false);
        }
        /// <include file="BaseNodeAddress.cs.xml" path='docs/member[@name="M:BaseNodeAddress.ToSecure"]/*' />
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