// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Queries
{
    public class MirrorNodeContractCallQuery : MirrorNodeContractQuery<MirrorNodeContractCallQuery>
    {
        /// <summary>
        /// Does transient simulation of read-write operations and returns the result in hexadecimal string format.
        /// </summary>
        /// <param name="client">The Client instance to perform the operation with</param>
        /// <returns>The result of the contract call</returns>
        /// <exception cref="ExecutionException"></exception>
        /// <exception cref="InterruptedException"></exception>
        public virtual string Execute(Client client)
        {
            return Call(client);
        }
    }
}