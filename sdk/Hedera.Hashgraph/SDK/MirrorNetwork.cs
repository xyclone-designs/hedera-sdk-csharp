using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Utility class.
 */
    public class MirrorNetwork : BaseNetwork<MirrorNetwork, BaseNodeAddress, MirrorNode> 
    {
        private MirrorNetwork(ExecutorService executor, List<string> addresses) : base(executor)
		{
            transportSecurity = true;

            try { SetNetwork(addresses); }
            catch (InterruptedException e) { }
            catch (TimeoutException e)
            {
                // This should never occur. The network is empty.
            }
        }

        /**
         * Create a mirror network for mainnet.
         *
         * @param executor the executor service
         * @return the new mirror network for mainnet
         */
        public static MirrorNetwork ForMainnet(ExecutorService executor) 
        {
            return new MirrorNetwork(executor, [ "mainnet-public.mirrornode.hedera.com:443" ]));
        }
        /**
         * Create a mirror network for testnet.
         *
         * @param executor the executor service
         * @return the new mirror network for testnet
         */
        public static MirrorNetwork ForTestnet(ExecutorService executor) 
        {
            return new MirrorNetwork(executor, [ "testnet.mirrornode.hedera.com:443" ]));
        }
        /**
         * Create a mirror network for previewnet.
         *
         * @param executor the executor service
         * @return the new mirror network for previewnet
         */
        public static MirrorNetwork ForPreviewnet(ExecutorService executor) 
        {
            return new MirrorNetwork(executor, [ "previewnet.mirrornode.hedera.com:443" ]));
        }
		/**
         * Create an arbitrary mirror network.
         *
         * @param executor  the executor service
         * @param addresses the arbitrary address for the network
         * @return the new mirror network object
         */
		public static MirrorNetwork ForNetwork(ExecutorService executor, List<string> addresses)
		{
			return new MirrorNetwork(executor, addresses);
		}

        /**
         * Extract the network names.
         *
         * @return the network names
         */
        List<string> GetNetwork() 
        {
            lock (this) { return [.. network.Select(_ => _.ToString())]; }
		}

        /**
         * Assign the desired network.
         *
         * @param network the desired network
         * @return the mirror network
         * @     when the transaction times out
         * @ when a thread is interrupted while it's waiting, sleeping, or otherwise occupied
         */
        public MirrorNetwork SetNetwork(List<string> network)
		{
			lock (this) 
            {
                return base.SetNetwork(network.ToDictionary(_ => _, _ => BaseNodeAddress.FromString(_))); ;
			}
		}
		/**
         * Extract the next healthy mirror node on the list.
         *
         * @return the next healthy mirror node on the list
         * @ when a thread is interrupted while it's waiting, sleeping, or otherwise occupied
         */
		public MirrorNetwork getNextMirrorNode()
		{
			lock (this) 
            {
                Num

                return base.SetNetwork(network.ToDictionary(_ => _, _ => BaseNodeAddress.FromString(_))); ;
			}
		}
        
        protected override MirrorNode CreateNodeFromNetworkEntry(KeyValuePair<string, BaseNodeAddress> entry)
        {
            return new MirrorNode(entry.Key, executor);
        }

        MirrorNode GetNextMirrorNode()  
        {
            lock (this) { return GetNumberOfMostHealthyNodes(1).get(0); }
        }

        /**
         * Convenience to get the REST base URL from the next healthy mirror node.
         */
        string getRestBaseUrl()  {
            return getNextMirrorNode().getRestBaseUrl();
        }
    }

}