// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Queries
{
    public class MirrorNodeContractEstimateGasQuery : MirrorNodeContractQuery<MirrorNodeContractEstimateGasQuery>
    {
        /// <summary>
        /// Returns gas estimation for the EVM execution.
        /// </summary>
        /// <param name="client">The Client instance to perform the operation with</param>
        /// <returns>The estimated gas cost</returns>
        /// <exception cref="ExecutionException"></exception>
        /// <exception cref="InterruptedException"></exception>
        public virtual long Execute(Client client)
        {
            return Estimate(client);
        }
    }
}