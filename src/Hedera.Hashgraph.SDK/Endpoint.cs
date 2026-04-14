// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="Endpoint.cs.xml" path='docs/member[@name="T:Endpoint"]/*' />
    public class Endpoint : ICloneable
    {
		/// <include file="Endpoint.cs.xml" path='docs/member[@name="P:Endpoint.Port"]/*' />
		public virtual int Port { get; set; }
		/// <include file="Endpoint.cs.xml" path='docs/member[@name="P:Endpoint.Address"]/*' />
		public virtual byte[]? Address { get; set; }
        /// <include file="Endpoint.cs.xml" path='docs/member[@name="P:Endpoint.DomainName"]/*' />
        public virtual string DomainName { get; set; } = string.Empty; 

		/// <include file="Endpoint.cs.xml" path='docs/member[@name="M:Endpoint.FromProtobuf(Proto.Services.ServiceEndpoint)"]/*' />
		public static Endpoint FromProtobuf(Proto.Services.ServiceEndpoint serviceEndpoint)
        {
            return new Endpoint
            {
                DomainName = serviceEndpoint.DomainName,
                Address = serviceEndpoint.IpAddressV4.ToByteArray(),
                Port = (int)(serviceEndpoint.Port & 0x00000000ffffffff),
            };
        }

        /// <include file="Endpoint.cs.xml" path='docs/member[@name="M:Endpoint.ValidateNoIpAndDomain(Endpoint)"]/*' />
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

        /// <include file="Endpoint.cs.xml" path='docs/member[@name="M:Endpoint.ToProtobuf"]/*' />
        public virtual Proto.Services.ServiceEndpoint ToProtobuf()
        {
            Proto.Services.ServiceEndpoint proto = new()
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
