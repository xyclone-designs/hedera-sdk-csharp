using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Internal utility class.
	 */
	public class BaseNodeAddress
	{
		private static readonly Regex HOST_AND_PORT = new ("^(\\S+):(\\d+)$");
		private static readonly Regex IN_PROCESS  = new ("^in-process:(\\S+)$");

		private static readonly int PORT_MIRROR_TLS = 443;
		private static readonly int PORT_NODE_PLAIN = 50211;
		private static readonly int PORT_NODE_TLS = 50212;


		/**
		 * Constructor.
		 *
		 * @param name                      the name part
		 * @param address                   the address part
		 * @param port                      the port part
		 */
		public BaseNodeAddress(string? name, string? address, int port) : this(name, address, port, false) { }
		/**
		 * Constructor.
		 *
		 * @param name                      the name part
		 * @param address                   the address part
		 * @param port                      the port part
		 * @param secure                    secure transport
		 */
		public BaseNodeAddress(string? name, string? address, int port, bool secure)
		{
			Name = name;
			Address = address;
			Port = port;
			Secure = secure;
		}

		/**
		 * Create a managed node address fom a string.
		 *
		 * @param string                    the string representation
		 * @return                          the new managed node address
		 */
		public static BaseNodeAddress FromString(string str)
		{
			var hostAndPortMatcher = HOST_AND_PORT.Matches(str);
			var inProcessMatcher = IN_PROCESS.Matches(str);

			if (hostAndPortMatcher.Count == 2)
			{
				Match address = hostAndPortMatcher.ElementAt(1);
				Match port = hostAndPortMatcher.ElementAt(2);

				return new BaseNodeAddress(null, address.Value, int.Parse(port.Value));
			}
			else if (inProcessMatcher.Count == 1)
			{
				Match name = hostAndPortMatcher.ElementAt(1);

				return new BaseNodeAddress(name.Value, null, 0);
			}
			else throw new InvalidOperationException("failed to parse node address");
		}

		private string? Name { get; }
		private string? Address { get; }
		private int Port { get; }
		private bool Secure { get; }
		private bool IsTransportSecurity { get => Port == PORT_NODE_TLS || Port == PORT_MIRROR_TLS || Secure; }

		/**
		 * Create a new insecure managed node.
		 *
		 * @return                          
		 * the insecure managed node address
		 */
		public BaseNodeAddress ToInsecure()
		{
			int newPort = (Port == PORT_NODE_TLS) ? PORT_NODE_PLAIN : Port;

			return new BaseNodeAddress(Name, Address, newPort, false);
		}
		/**
		 * Create a new managed node.
		 *
		 * @return                          
		 * the secure managed node address
		 */
		public BaseNodeAddress ToSecure()
		{
			int newPort = (Port == PORT_NODE_PLAIN) ? PORT_NODE_TLS : Port;

			return new BaseNodeAddress(Name, Address, newPort, true);
		}

        public override string ToString()
        {
			return Name ?? Address + ":" + Port;
		}
        public override int GetHashCode()
        {
			return HashCode.Combine(Name, Address, Port);
		}
		public override bool Equals(object? obj)
		{
			if (this == obj) return true;
			if (obj == null || GetType() != obj.GetType()) return false;

			BaseNodeAddress that = (BaseNodeAddress)obj;

			return
				Equals(Name, that.Name) &&
				Equals(Address, that.Address) &&
				Port == that.Port;
		}
	}

}