// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Networking
{
    public class MirrorNodeContractEstimateGasQuery : MirrorNodeContractQuery<MirrorNodeContractEstimateGasQuery>
    {
        /// <include file="MirrorNodeContractEstimateGasQuery.cs.xml" path='docs/member[@name="M:Execute(Client)"]/*' />
        public virtual long Execute(Client client)
        {
            return Estimate(client);
        }
    }
}