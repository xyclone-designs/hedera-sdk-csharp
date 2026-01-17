using System.Text;

namespace Hedera.Hashgraph.SDK
{
	/**
     * An individual mirror node.
     */
    public class MirrorNode : BaseNode<MirrorNode, BaseNodeAddress> 
    {
        /**
         * Constructor.
         *
         * @param address                   the node address as a managed node address
         * @param executor                  the executor service
         */
        MirrorNode(BaseNodeAddress address, ExecutorService executor) : base(address, executor) { }
        /**
         * Constructor.
         *
         * @param address                   the node address as a string
         * @param executor                  the executor service
         */
        MirrorNode(string address, ExecutorService executor) : this(BaseNodeAddress.FromString(address), executor) { }

        @Override
        protected string getAuthority() {
            return null;
        }

        @Override
		BaseNodeAddress getKey() {
            return address;
        }

        /**
         * Build the REST base URL for this mirror node.
         *
         * @return scheme://host[:port]/api/v1
         */
        string getRestBaseUrl() 
        {
            string host = address.getAddress();
            int port = address.getPort();

            if (host == null) {
                throw new IllegalStateException("mirror node address is not set");
            }

            if (isLocalHost(host)) {
                // For localhost, always use port 5551 for general REST calls
                return "http://" + host + ":5551/api/v1";
            }

            string scheme = chooseScheme(port);

            StringBuilder base = new StringBuilder();
            base.append(scheme).append("://").append(host);
            // Omit default ports
            if (!isDefaultPort(scheme, port)) {
                base.append(":").append(port);
            }
            base.append("/api/v1");
            return base.toString();
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