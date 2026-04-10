// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Networking
{
    public class MirrorNodeContractCallQuery : MirrorNodeContractQuery<MirrorNodeContractCallQuery>
    {
        /// <include file="MirrorNodeContractCallQuery.cs.xml" path='docs/member[@name="M:Execute(Client)"]/*' />
        public virtual string Execute(Client client)
        {
            return Call(client);
        }
    }
}