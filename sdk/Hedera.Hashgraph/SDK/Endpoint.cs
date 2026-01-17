using Google.Protobuf;
using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Utility class used internally by the sdk.
     */
    public class Endpoint : ICloneable 
    {
		/**
         * The ipv4 address.
         */
		public byte[]? Address { get; set; }
		/**
         * The port number.
         */
		public int? Port { get; set; }
		/**
         * The domain name.
         */
		public int? DomainName { get; set; }

        /**
         * Constructor.
         */
        public Endpoint() {}

        /**
         * Create an endpoint object from a service endpoint protobuf.
         *
         * @param serviceEndpoint           the service endpoint protobuf
         * @return                          the endpoint object
         */
        static Endpoint FromProtobuf(ServiceEndpoint serviceEndpoint) 
        {
            var port = (int) (serviceEndpoint.getPort() & 0x00000000ffffffffL);

            return new Endpoint
            {
                Address = serviceEndpoint.getIpAddressV4().ToByteArray(),
                Port = port,
                DomainName = serviceEndpoint.getDomainName(),
            };           
        }

        /**
         * Validate that the endpoint does not contain both an IP address and a domain name.
         *
         * @param endpoint the endpoint to validate
         * @ if both ipAddressV4 and domainName are present
         */
        public static void ValidateNoIpAndDomain(Endpoint endpoint) 
        {
            if (endpoint == null) 
                return;

			if (endpoint.Address is not null && endpoint.DomainName.HasValue)
				throw new ArgumentException("Endpoint must not contain both ipAddressV4 and domainName");
		}

        /**
         * Create the protobuf.
         *
         * @return                          the protobuf representation
         */
        public Proto.ServiceEndpoint ToProtobuf() 
        {
			Proto.ServiceEndpoint serviceendpoint = new ()
            {
                Port = Port,
				DomainName = DomainName,
				IpAddressV4 = ByteString.CopyFrom(Address),
			};

            if (Port != null) serviceendpoint.Port = Port.Value;
            if (DomainName != null) serviceendpoint.DomainName = DomainName.Value;
            if (Address != null) serviceendpoint.IpAddressV4 = ByteString.CopyFrom(Address);


			builder.;

            return builder.setPort(port).build();
        }

        public override string ToString() 
        {
            if (this.domainName != null && !this.domainName.isEmpty()) {
                return domainName + ":" + port;
            } else {
                return ((int) address[0] & 0x000000FF) + "." + ((int) address[1] & 0x000000FF) + "."
                        + ((int) address[2] & 0x000000FF)
                        + "." + ((int) address[3] & 0x000000FF) + ":"
                        + port;
            }
        }

        public override Endpoint Clone()
        {
            try {
                Endpoint clone = (Endpoint) base.clone();
                clone.Address = address != null ? address.clone() : null;
                return clone;
            } catch (CloneNotSupportedException e) {
                throw new AssertionError();
            }
        }
    }
}