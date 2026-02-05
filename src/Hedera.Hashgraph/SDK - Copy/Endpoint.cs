// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Utility class used internally by the sdk.
    /// </summary>
    public class Endpoint : ICloneable
    {
		/// <summary>
		/// The desired port.
		/// </summary>
		public virtual int Port { get; set; }
		/// <summary>
		/// The ipv4 address.
		/// </summary>
		public virtual byte[]? Address { get; set; }
        /// <summary>
        /// The desired port.
        /// </summary>
        public virtual string DomainName { get; set; } = string.Empty; 

		/// <summary>
		/// Create an endpoint object from a service endpoint protobuf.
		/// </summary>
		/// <param name="serviceEndpoint">the service endpoint protobuf</param>
		/// <returns>                         the endpoint object</returns>
		public static Endpoint FromProtobuf(Proto.ServiceEndpoint serviceEndpoint)
        {
            return new Endpoint
            {
                DomainName = serviceEndpoint.DomainName,
                Address = serviceEndpoint.IpAddressV4.ToByteArray(),
                Port = (int)(serviceEndpoint.Port & 0x00000000ffffffff),
            };
        }

        /// <summary>
        /// Validate that the endpoint does not contain both an IP address and a domain name.
        /// </summary>
        /// <param name="endpoint">the endpoint to validate</param>
        /// <exception cref="IllegalArgumentException">if both ipAddressV4 and domainName are present</exception>
        public static void ValidateNoIpAndDomain(Endpoint endpoint)
        {
            if (endpoint == null)
            {
                return;
            }

            if (endpoint.Address != null)
            {
                var dn = endpoint.DomainName;
                if (dn != null && dn.Length == 0)
                {
                    throw new ArgumentException("Endpoint must not contain both ipAddressV4 and domainName");
                }
            }
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        public virtual Proto.ServiceEndpoint ToProtobuf()
        {
            Proto.ServiceEndpoint proto = new()
            {
				Port = Port,
                DomainName = DomainName,
            };

			if (Address != null)
				proto.IpAddressV4 = ByteString.CopyFrom(Address);

			return proto;
        }
		public virtual object Clone()
		{
			return new Endpoint
			{
				Port = Port,
				Address = (byte[]?)Address?.Clone(),
				DomainName = (string)DomainName.Clone(),
			};
		}

		public override string ToString()
        {
            if (DomainName != null && DomainName.Length > 0)
            {
                return DomainName + ":" + Port;
            }
            else
            {
                return (Address?[0] & 0x000000FF) + "." + (Address?[1] & 0x000000FF) + "." + (Address?[2] & 0x000000FF) + "." + (Address?[3] & 0x000000FF) + ":" + Port;
            }
        }
    }
}