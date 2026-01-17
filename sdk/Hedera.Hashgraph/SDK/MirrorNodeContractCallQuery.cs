namespace Hedera.Hashgraph.SDK
{
	public class MirrorNodeContractCallQuery : MirrorNodeContractQuery<MirrorNodeContractCallQuery> 
    {
        /**
         * Does transient simulation of read-write operations and returns the result in hexadecimal string format.
         *
         * @param client The Client instance to perform the operation with
         * @return The result of the contract call
         * @
         * @
         */
        public string Execute(Client client) 
        {
            return Call(client);
        }

        public override string ToString() 
        {
            return "MirrorNodeContractCallQuery" + base.ToString();
        }
    }
}