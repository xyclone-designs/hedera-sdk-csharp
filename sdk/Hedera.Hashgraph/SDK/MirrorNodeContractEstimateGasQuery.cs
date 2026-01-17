namespace Hedera.Hashgraph.SDK
{
	public class MirrorNodeContractEstimateGasQuery : MirrorNodeContractQuery<MirrorNodeContractEstimateGasQuery>
    {

        /**
         * Returns gas estimation for the EVM execution.
         *
         * @param client The Client instance to perform the operation with
         * @return The estimated gas cost
         * @
         * @
         */
        public long Execute(Client client) 
        {
            return Estimate(client);
        }

        public override string ToString() 
        {
            return "MirrorNodeContractEstimateGasQuery" + base.ToString();
        }
    }

}